using System;
using UnityEngine;

public class PlayerController : MonoBehaviour, IMoveable, IKnockback
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 10f;
    [Space]
    [SerializeField] private float groundAcc = 0.1f;
    [SerializeField] private float airAcc = 0.015f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private int dashValue = 50;

    [Header("Slam")]
    [SerializeField] private float slamForce = 100f;
    [SerializeField] private float slamCooldown = 0.2f;
    [SerializeField] private int slamValue = 25;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpCooldown = 0.2f;

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpHorizontalForce = 20f;
    [SerializeField] private float wallJumpVerticalForce = 8f;
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private float wallJumpDecay = 5f;
    [SerializeField] private LayerMask wallMask;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheck;
    
    [Header("Enemy Check")]
    [SerializeField] private float dashRadius = 1f;
    [SerializeField] private float slamRadius = 5f;
    [Space]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask bulletLayer;
    [Space]
    [SerializeField] private Transform dashHitCheck;
    
    [Header("Enemy Hit")]
    [SerializeField] private int slamDamage = 50;
    [SerializeField] private int dashDamage = 40;
    [SerializeField] private float playerRepelForce = 60f;
    [SerializeField] private float enemyRepelForce = 100f;

    // Timers
    private float slamTimer;
    private float jumpTimer;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float dashCooldownTimer;
    
    // Movement
    public float DashDuration => dashDuration;
    public float MoveMagnitude { get; private set; }
    public bool DashHit { get; private set; }
    
    private float currentSpeed;
    private bool IsSlamming;
    private bool IsDashing;

    // Vectors
    private Vector2 moveInput;
    private Vector3 dashVelocity;
    private Vector3 inputVelocity;
    private Vector3 finalVelocity;
    private Vector3 lastWallNormal;
    private Vector3 wallJumpVelocity;
    private Vector3 knockbackVelocity;

    // Ground info
    private float gravityVelocity;
    private Vector3 GroundNormal;
    
    public bool IsGrounded { get; private set; }

    // Components
    private PlayerStamina PlayerStamina { get; set; }
    
    private VFXManager VfxManager;
    private PlayerState currentState;
    private CharacterController cc;
    private Transform Player;
    private Camera Cam;

    // Events
    public event Action OnDash; 
    public event Action OnJumped;
    public event Action OnLanded;
    public event Action OnSlamImpact;

    // ---

    #region Unity

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        PlayerStamina = GetComponent<PlayerStamina>();
    }

    private void Start()
    {
        Player = transform;
        Cam = Camera.main;
        VfxManager = VFXManager.Instance;

        // Subscribe to input events
        InputManager.OnJumpPressed += OnJumpInput;
        InputManager.OnDashPressed += OnDashInput;
        InputManager.OnSlamPressed += OnSlamInput;

        SetState(new PlayerGroundedState(this));
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        InputManager.OnJumpPressed -= OnJumpInput;
        InputManager.OnDashPressed -= OnDashInput;
        InputManager.OnSlamPressed -= OnSlamInput;
    }

    private void Update()
    {
        float delta = Time.deltaTime;
        if (!cc) return;
        
        // TIMERS COME FIRST!
        // Update timer regardless of grounded state
        #region Timers
        
        if (jumpTimer > 0f) jumpTimer -= delta;
        if (dashCooldownTimer > 0f) dashCooldownTimer -= delta;
        if (slamTimer > 0f) slamTimer -= delta;
        if (jumpBufferTimer > 0f) jumpBufferTimer -= delta;
        if (coyoteTimer > 0f) coyoteTimer -= delta;

        #endregion
        // State update
        currentState?.Update(delta);
        UpdateGrounded();
        ApplyGravity(delta);
        // Set timer
        if (IsGrounded)
        {
            wallJumpVelocity = Vector3.zero;
            coyoteTimer = coyoteTime;
        }
        moveInput = InputManager.MoveInput;
    }

    private void LateUpdate()
    {
        float delta = Time.deltaTime;
        
        currentState?.LateUpdate(delta);
        MoveTo(moveInput, delta);
    }

    #endregion

    public void SetState(PlayerState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    #region  Input event handlers

    private void OnJumpInput() => jumpBufferTimer = jumpBufferTime;
    private void OnDashInput() => currentState?.OnDashInput();
    private void OnSlamInput() => currentState?.OnSlamInput();

    #endregion
    
    #region Behaviour
    
    // Gravity
    private void ApplyGravity(float delta)
    {
        if (IsDashing) return;

        if (IsGrounded && gravityVelocity < 0)
        {
            gravityVelocity = GravityData.groundStickForce;
            return;
        }

        float gravityMultiplier = ((gravityVelocity < 0.1f) ? GravityData.fallStrength : 1f) * GravityData.gravity;
        gravityVelocity += gravityMultiplier * delta;
    }
    
    // Move
    public void MoveTo(Vector3 input, float delta)
    {
        if (IsDashing || IsSlamming)
        {
            Vector3 finalMove = dashVelocity + Vector3.up * gravityVelocity;
            cc.Move(finalMove * delta);
            return;
        }

        // 1. Handle primary movement input
        float targetSpeed = input.magnitude > 0 ? walkSpeed : 0;
        currentSpeed = targetSpeed;

        Vector3 moveDirection = Player.forward * input.y + Player.right * moveInput.x;
        Vector3 targetVelocity = moveDirection * currentSpeed;
    
        // Only apply drag when NOT providing input
        float acceleration = IsGrounded ? groundAcc : airAcc;
        if (input.magnitude < 0.1f)
            inputVelocity = Vector3.Lerp(inputVelocity, Vector3.zero, 10 * delta);
        else
            inputVelocity = Vector3.MoveTowards(inputVelocity, targetVelocity, acceleration * delta);
            
        // 2. Handle external velocities (wall jump, knockback)
        if (wallJumpVelocity.magnitude > 0.5f && moveDirection.magnitude > 0.1f) {
            // Transfer wall jump energy to input velocity
            inputVelocity += wallJumpVelocity * 0.5f;
            wallJumpVelocity *= 0.5f; // Reduce wall jump since we transferred some
        }
        
        if (wallJumpVelocity.magnitude > 0.01f)
            wallJumpVelocity = Vector3.Lerp(wallJumpVelocity, Vector3.zero, wallJumpDecay * delta);
        
        if (knockbackVelocity.magnitude > 0.5f && moveDirection.magnitude > 0.1f) {
            // Transfer wall knockback energy to input velocity
            inputVelocity += knockbackVelocity * 0.5f;
            knockbackVelocity *= 0.5f; // Reduce knockback since we transferred some
        }
        
        if (knockbackVelocity.magnitude > 0.1f)
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, 3f * delta);
        
        // 3. Calculate magnitude for animation/feedback
        float rawSpeed = new Vector3(inputVelocity.x, 0, inputVelocity.z).magnitude;
        MoveMagnitude = Mathf.InverseLerp(0f, dashSpeed, rawSpeed);

        // 4. Combine all velocities
        finalVelocity = inputVelocity + knockbackVelocity + wallJumpVelocity + Vector3.up * gravityVelocity;
        
        // 5. Perform Movement
        cc.Move(finalVelocity * delta);
    }
    
    // Jump
    public void PerformJump()
    {
        // Perform Jump
        gravityVelocity = jumpHeight;
        // Update Data
        jumpTimer = jumpCooldown;
        coyoteTimer = 0f;
        // Event
        OnJumped?.Invoke();
    }
    public void PerformWallJump()
    {
        // Apply Vertical Velocity
        gravityVelocity = wallJumpVerticalForce;
        // Calculate Horizontal Velocity
        Vector3 pushDir = lastWallNormal + Vector3.up * 0.2f;
        pushDir.Normalize();
        // Perform Wall Jump
        wallJumpVelocity = pushDir * wallJumpHorizontalForce;
        // Update Data
        jumpTimer = jumpCooldown;
        // Event
        OnJumped?.Invoke();
    }
    public void OnLand() => OnLanded?.Invoke();
    
    // Slam
    public void ApplySlamStart()
    {
        // Perform Slam
        gravityVelocity = -slamForce;
        // Reset Cooldown
        slamTimer = slamCooldown;
        // Update stamina
        TakeStamina(slamValue);
        // Update Data
        IsSlamming = true;
    }
    public void ApplySlamImpact()
    {
        // Check Enemy in Range
        Collider[] enemyColliders = new Collider[20];
        int enemyCount = EntityHelper.EntityInRange(groundCheck.position, slamRadius, enemyColliders, enemyLayer);
        
        // Perform Slam Impact
        if (enemyCount > 0)
        {
            for(int i = 0; i < enemyCount; i++)
            {
                Collider enemy = enemyColliders[i];
                if (enemy.gameObject == gameObject) continue;
        
                ApplyDamageToEnemy(enemy, slamDamage, enemyRepelForce);
                HitStop();
            }
        }
        
        // Update Data
        IsSlamming = false;
        // Event
        OnSlamImpact?.Invoke();
    }
    
    // Dash
    public void ApplyDashImpulse(Vector3 dir)
    {
        // Perform Dash
        dashVelocity += dir.normalized * dashSpeed;
        // Update Stamina
        TakeStamina(dashValue);
        // Update Data
        dashCooldownTimer = dashCooldown;
        IsDashing = true;
        // Event
        OnDash?.Invoke();
    }

    public void ApplyDashImpact()
    {
        bool hitSomething = false;
        
        // Check Enemy in Range
        Collider[] enemyColliders = new Collider[5];
        int enemyCount = EntityHelper.EntityInRange(dashHitCheck.position, dashRadius, enemyColliders, enemyLayer);
        // Perform Dash Impact
        if (enemyCount > 0)
        {
            for (int i = 0; i < enemyCount; i++)
            {
                Collider enemy = enemyColliders[i];
                if (enemy.gameObject == gameObject) continue;
        
                ApplyDamageToEnemy(enemy, dashDamage, enemyRepelForce);
                Vector3 knockbackDir = EntityHelper.GetKnockbackDirection(enemy.transform.position, transform.position, 0.3f);
                ApplyKnockback(knockbackDir, playerRepelForce);
                hitSomething = true;
            }
        }
        
        // Check Bullet in Range
        Collider[] bulletColliders = new Collider[5];
        int bulletCount = EntityHelper.EntityInRange(dashHitCheck.position, dashRadius, bulletColliders, bulletLayer);
        // Perform Dash Impact
        if (bulletCount > 0)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                GameObject bullet = bulletColliders[i].gameObject;
                
                if (bullet) bullet.GetComponent<Bullet>().SetDirection(Cam.transform.forward);
                hitSomething = true;
            }
        }
        
        // Perform Common Effects
        if (hitSomething)
            HitStop();
        // Update Data
        DashHit = hitSomething;
    }
    
    public void ResetDash()
    {
        dashVelocity = Vector3.zero;
        IsDashing = false;
        DashHit = false;
    }
    
    private void ApplyDamageToEnemy(Collider enemy, int damage, float knockback)
    {
        enemy.GetComponent<IDamageable>()?.TakeDamage(damage);
        Vector3 knockbackDir = EntityHelper.GetKnockbackDirection(transform.position, enemy.transform.position, 0);
        enemy.GetComponent<IKnockback>()?.ApplyKnockback(knockbackDir, 
            knockback
        );
    }
    
    // Grounded
    private void UpdateGrounded()
    {
        IsGrounded = EntityHelper.IsGrounded(groundCheck.position, groundCheckRadius, out RaycastHit hit, groundCheckDistance, groundMask);
    }
    
    #endregion
    
    private void HitStop() => VfxManager.TriggerHitStop(0.1f);
    private void TakeStamina(int amount) => PlayerStamina.TakeStamina(amount);

    #region Bools
    
    // Jump
    private bool CheckWall(out Vector3 wallNormal)
    {
        Vector3 origin = transform.position + Vector3.up * 1f;

        if (Physics.SphereCast(origin, cc.radius * 0.9f, Player.forward, out RaycastHit hit, wallCheckDistance, wallMask))
        {
            wallNormal = hit.normal;
            return true;
        }

        wallNormal = Vector3.zero;
        return false;
    }
    public bool CanWallJump()
    {
        if (IsGrounded) return false;
        if (!CheckWall(out Vector3 normal)) return false;
        
        lastWallNormal = normal;
        return jumpBufferTimer > 0f;
    }
    public bool CanJump()
    {
        if (!(jumpBufferTimer > 0f) || !(coyoteTimer > 0f) || !(jumpTimer <= 0f)) return false;
        
        jumpBufferTimer = 0f;
        return true;
    }

    // Special
    public bool CanSlam() => !IsGrounded && slamTimer <= 0 && PlayerStamina.CurrentStamina >= slamValue;
    public bool CanDash() => dashCooldownTimer <= 0 && !(moveInput.y < -0.1f) && moveInput != Vector2.zero  && PlayerStamina.CurrentStamina >= dashValue;

    #endregion
    
    #region Helpers

    public Vector3 CaptureDirection()
    {
        return moveInput != Vector2.zero ? (Cam.transform.forward * moveInput.y + Cam.transform.right * moveInput.x).normalized : Cam.transform.forward.normalized;
    }

    #endregion
    
    #region Gizmos
    
    private void OnDrawGizmos()
    {
        // Grounded sphere
        Gizmos.DrawSphere(groundCheck.position + (Vector3.down * groundCheckDistance), groundCheckRadius);
        // Enemy detect sphere
        Gizmos.DrawWireSphere(groundCheck.position, slamRadius);
        Gizmos.DrawWireSphere(dashHitCheck.position, dashRadius);
    }
    
    #endregion

    public void ApplyKnockback(Vector3 direction, float force)
    {
        knockbackVelocity = direction.normalized * force;
    }
}