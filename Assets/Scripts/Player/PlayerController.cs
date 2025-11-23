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
    [SerializeField] private float detectRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask bulletLayer;
    [SerializeField] private Transform dashHitCheck;
    [Space]
    [SerializeField] private float playerRepelForce = 60f;
    [SerializeField] private float enemyRepelForce = 100f;

    // Timers
    private float slamTimer;
    private float jumpTimer;
    private float coyoteTimer;
    private float currentSpeed;
    private float jumpBufferTimer;
    private float dashCooldownTimer;

    // Float
    private float currentSmoothSpeed;
    private float acceleration;

    public float DashDuration => dashDuration;
    public float MoveMagnitude { get; private set; }

    // Vectors
    private Vector3 dashVelocity;
    private Vector3 inputVelocity;
    private Vector3 finalVelocity;
    private Vector3 lastWallNormal;
    private Vector3 wallJumpVelocity;
    private Vector3 knockbackVelocity;
    
    private bool IsSlamming;

    // Ground info
    public bool IsGrounded { get; private set; }
    private Vector3 GroundNormal;

    private float velocityY;
    private bool IsDashing;
    
    private PlayerStamina PlayerStamina { get; set; }

    // Components
    private PlayerState currentState;
    private CharacterController CC;
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
        CC = GetComponent<CharacterController>();
        PlayerStamina = GetComponent<PlayerStamina>();
    }

    private void Start()
    {
        Player = transform;
        Cam = Camera.main;

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

        UpdateGrounded();

        // TIMERS COME FIRST!
        #region Timers

        // Update timer regardless of grounded state
        if (jumpTimer > 0f) jumpTimer -= delta;
        if (dashCooldownTimer > 0f) dashCooldownTimer -= delta;
        if (slamTimer > 0f) slamTimer -= delta;
        if (jumpBufferTimer > 0f) jumpBufferTimer -= delta;
        if (coyoteTimer > 0f) coyoteTimer -= delta;

        #endregion

        // State update
        currentState?.Update(delta);

        // Update gravity
        ApplyGravity(delta);

        // Set timer
        if (IsGrounded)
        {
            wallJumpVelocity = Vector3.zero;
            coyoteTimer = coyoteTime;
        }
        
        Debug.Log(currentState);
    }

    private void LateUpdate()
    {
        float delta = Time.deltaTime;

        currentState?.LateUpdate(delta);
        
        // Safety check for NaN position
        if (float.IsNaN(transform.position.x) || float.IsNaN(transform.position.y) || float.IsNaN(transform.position.z))
        {
            Debug.LogError("Player position corrupted! Resetting...");
            transform.position = Vector3.up * 2f; // Reset above ground
            finalVelocity = Vector3.zero;
            inputVelocity = Vector3.zero;
            wallJumpVelocity = Vector3.zero;
            knockbackVelocity = Vector3.zero;
            velocityY = 0f;
        }
        
        MoveTo(InputManager.MoveInput, delta);
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

        if (IsGrounded && velocityY < 0)
        {
            velocityY = GravityData.groundStickForce;
            return;
        }

        float gravityMultiplier = ((velocityY < 0.1f) ? GravityData.fallStrength : 1f) * GravityData.gravity;
        velocityY += gravityMultiplier * delta;
    }
    
    // Move
    public void MoveTo(Vector3 input, float delta)
    {
        acceleration = IsGrounded ? groundAcc : airAcc;
        
        if (IsDashing || IsSlamming)
        {
            Vector3 finalMove = dashVelocity + Vector3.up * velocityY;
            CC.Move(finalMove * delta);
            return;
        }

        // 1. Handle primary movement input
        Vector2 moveInput = input;
        float targetSpeed = moveInput.magnitude > 0 ? walkSpeed : 0;
        currentSpeed = targetSpeed;

        Vector3 moveDirection = Player.forward * moveInput.y + Player.right * moveInput.x;
        Vector3 targetVelocity = moveDirection * currentSpeed;
    
        // Only apply drag when NOT providing input
        if (moveInput.magnitude < 0.1f)
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
        finalVelocity = inputVelocity + knockbackVelocity + wallJumpVelocity + Vector3.up * velocityY;

        // 5. Final NaN safety check
        if (float.IsNaN(finalVelocity.x) || float.IsNaN(finalVelocity.y) || float.IsNaN(finalVelocity.z)) {
            Debug.LogError("NaN detected in finalVelocity! Resetting.");
            finalVelocity = Vector3.zero;
            inputVelocity = Vector3.zero;
            wallJumpVelocity = Vector3.zero; 
            knockbackVelocity = Vector3.zero;
            velocityY = 0f;
        }

        CC.Move(finalVelocity * delta);
    }
    
    // Jump
    public void PerformJump()
    {
        velocityY = Mathf.Sqrt(-GravityData.gravity * jumpHeight);
        jumpTimer = jumpCooldown;
        coyoteTimer = 0f; // Consume coyote time
        OnJumped?.Invoke();
    }
    public void PerformWallJump()
    {
        velocityY = Mathf.Sqrt(-GravityData.gravity * wallJumpVerticalForce);
        Vector3 pushDir = lastWallNormal + Vector3.up * 0.2f;
        pushDir.Normalize();
        wallJumpVelocity = pushDir * wallJumpHorizontalForce;
        jumpTimer = jumpCooldown;
        OnJumped?.Invoke();
    }
    public void OnLand() => OnLanded?.Invoke();
    
    // Slam
    public void ApplySlamStart()
    {
        // Perform Slam
        velocityY = -slamForce;
        // Reset Cooldown
        slamTimer = slamCooldown;
        // Update stamina
        TakeStamina(slamValue);
        IsSlamming = true;
    }
    public void ApplySlamImpact()
    {
        // Give enemy damage
        Collider[] enemyColliders = EntityInRange(groundCheck.position, detectRadius, enemyLayer);
        
        if (enemyColliders.Length > 0)
        {
            foreach (var enemy in enemyColliders)
            {
                ApplyDamageToEnemy(enemy ,50, enemyRepelForce);
                HitStop();
            }
        }
        
        // Effects
        IsSlamming = false;
        OnSlamImpact?.Invoke();
    }
    
    // Dash
    public void ApplyDashImpulse(Vector3 dir)
    {
        dashCooldownTimer = dashCooldown;
        dashVelocity += dir.normalized * dashSpeed;
        TakeStamina(dashValue);
        IsDashing = true;
        OnDash?.Invoke();
    }

    public bool DashHit;

    public void ApplyDashImpact()
    {
        bool hitSomething = false;

        Collider[] enemyColliders = EntityInRange(dashHitCheck.position, 1, enemyLayer);
        Collider[] bulletColliders = EntityInRange(dashHitCheck.position, 1, bulletLayer);
        
        if (enemyColliders.Length > 0)
        {
            foreach (var enemy in enemyColliders)
            {
                if (enemy.gameObject == gameObject) continue;
                
                ApplyDamageToEnemy(enemy ,50, enemyRepelForce);
                Vector3 knockbackDir = EntityHelper.GetKnockbackDirection(enemy.transform.position, transform.position, 0.3f);
                ApplyKnockback(knockbackDir, playerRepelForce);
                hitSomething = true;
            }
        }

        // Process bullets
        if (bulletColliders.Length > 0)
        {
            foreach (var bullet in bulletColliders)
            {
                if (bullet) bullet.GetComponent<Bullet>().SetDirection(Cam.transform.forward);
                hitSomething = true;
            }
        }
        
        if (hitSomething)
        {
            HitStop();
            DashHit = true;
            ResetDash();
        }
    }
    
    public void ResetDash()
    {
        dashVelocity = Vector3.zero; // IMPORTANT RESET
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
    
    private static void HitStop() => VFXManager.Instance.TriggerHitStop(0.15f);
    private void TakeStamina(int amount) => PlayerStamina.TakeStamina(amount);

    #region Bools
    
    // Jump
    private bool CheckWall(out Vector3 wallNormal)
    {
        Vector3 origin = transform.position + Vector3.up * 1f;

        if (Physics.SphereCast(origin, CC.radius * 0.9f, Player.forward, out RaycastHit hit, wallCheckDistance, wallMask))
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
    
    // Enemy
    // private int EntityInRange(Vector3 pos, float detectRadius, LayerMask layerMask)
    // {
    //     int enemyCount = Physics.OverlapSphereNonAlloc(pos, detectRadius, enemyColliders, layerMask);
    //     return enemyCount;
    // }
    
    private Collider[] EntityInRange(Vector3 pos, float detectRadius, LayerMask layerMask)
    {
        Collider[] colliders = Physics.OverlapSphere(pos, detectRadius, layerMask);
        return colliders;
    }

    // Special
    public bool CanSlam() => !IsGrounded && slamTimer <= 0 && PlayerStamina.CurrentStamina >= slamValue;
    public bool CanDash() => dashCooldownTimer <= 0 && !(InputManager.MoveInput.y < -0.1f) && PlayerStamina.CurrentStamina >= dashValue;

    #endregion
    
    #region Helpers

    public Vector3 CaptureDirection()
    {
        Vector2 moveInput = InputManager.MoveInput;
        return moveInput != Vector2.zero ? (Cam.transform.forward * moveInput.y + Cam.transform.right * moveInput.x).normalized : Cam.transform.forward.normalized;
    }

    #endregion
    
    #region 
    
    private void OnDrawGizmos()
    {
        // Grounded sphere
        Gizmos.DrawSphere(groundCheck.position + (Vector3.down * groundCheckDistance), groundCheckRadius);
        // Enemy detect sphere
        Gizmos.DrawWireSphere(groundCheck.position, detectRadius);
        Gizmos.DrawWireSphere(dashHitCheck.position, 1);
    }
    
    #endregion

    public void ApplyKnockback(Vector3 direction, float force)
    {
        knockbackVelocity = direction.normalized * force;
    }
}