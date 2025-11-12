using System;
using UnityEngine;

public class PlayerController : MonoBehaviour, IMoveable, IKnockback
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float moveSmoothness = 0.12f;

    [Header("Drag")]
    [SerializeField] private float groundDrag = 0.1f;
    [SerializeField] private float airDrag = 0.015f;
    [SerializeField] private float dragSnappiness = 10f;

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
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;
    
    [Header("Enemy Check")]
    [SerializeField] private float detectRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;

    // Timers
    private float slamTimer;
    private float jumpTimer;
    private float coyoteTimer;
    private float currentSpeed;
    private float jumpBufferTimer;
    private float dashCooldownTimer;

    // Float
    private float currentSmoothSpeed;
    private float targetDragSnappiness;

    public float DashDuration => dashDuration;
    public float MoveMagnitude { get; private set; }

    // Vectors
    private Vector3 dashVelocity;
    private Vector3 inputVelocity;
    private Vector3 finalVelocity;
    private Vector3 lastWallNormal;
    private Vector3 wallJumpVelocity;
    private Vector3 knockbackVelocity;
    
    private bool isSlaming;
    
    // Collider
    private Collider[] enemyCollider;

    // Ground info
    public bool IsGrounded { get; private set; }
    private Vector3 GroundNormal;

    private float velocityY { get; set; }
    public bool IsDashing { get; set; }

    private PlayerAudio PlayerAudio { get; set; }
    private PlayerStamina PlayerStamina { get; set; }

    // Components
    private PlayerState currentState;
    private CharacterController CC;
    private Transform Player;
    private Camera Cam;

    // Events
    public event Action OnJumped;
    public event Action OnLanded;
    public event Action OnSlamImpact;

    // ---

    #region Unity

    private void Awake()
    {
        CC = GetComponent<CharacterController>();
        PlayerAudio = GetComponent<PlayerAudio>();
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

            if (InputManager.MoveInput != Vector2.zero)
                PlayerAudio.PlayFootStep(delta);
            else
                PlayerAudio.ResetFootStep();
        }
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
        if (IsDashing || isSlaming)
        {
            Vector3 finalMove = dashVelocity + Vector3.up * velocityY;
            CC.Move(finalMove * delta);
            return;
        }

        Vector2 moveInput = input;
        float targetSpeed = moveInput.magnitude > 0 ? walkSpeed : 0;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref currentSmoothSpeed, delta * moveSmoothness);

        Vector3 moveDirection = Player.forward * moveInput.y + Player.right * moveInput.x;
        targetDragSnappiness = Mathf.Lerp(targetDragSnappiness, IsGrounded ? groundDrag : airDrag, delta * dragSnappiness);

        Vector3 targetPosition = moveDirection * currentSpeed;
        inputVelocity = Vector3.Lerp(inputVelocity, targetPosition, targetDragSnappiness);

        // FIXED: Wall jump handling with NaN protection
        if (wallJumpVelocity != Vector3.zero)
        {
            wallJumpVelocity = Vector3.Lerp(wallJumpVelocity, Vector3.zero, delta * wallJumpDecay);

            float inputStrength = moveInput.magnitude;
            if (inputStrength > 0.1f)
            {
                wallJumpVelocity = Vector3.Lerp(wallJumpVelocity, inputVelocity, targetDragSnappiness);
            }

            // CRITICAL FIX: Check if wallJumpVelocity is effectively zero or NaN
            if (wallJumpVelocity.sqrMagnitude < 0.001f || 
                float.IsNaN(wallJumpVelocity.x) || 
                float.IsNaN(wallJumpVelocity.y) || 
                float.IsNaN(wallJumpVelocity.z))
            {
                wallJumpVelocity = Vector3.zero;
            }
        }

        // ALWAYS combine velocities and move
        finalVelocity = inputVelocity + wallJumpVelocity + knockbackVelocity + Vector3.up * velocityY;
        
        if (knockbackVelocity.magnitude > 0.1f)
            knockbackVelocity = Vector3.Slerp(knockbackVelocity, Vector3.zero, 3f * delta);

        // CRITICAL FIX: NaN check before moving
        if (float.IsNaN(finalVelocity.x) || float.IsNaN(finalVelocity.y) || float.IsNaN(finalVelocity.z))
        {
            Debug.LogError("NaN detected in finalVelocity! Resetting velocities.");
            finalVelocity = Vector3.zero;
            inputVelocity = Vector3.zero;
            wallJumpVelocity = Vector3.zero;
            velocityY = 0f;
        }

        CC.Move(finalVelocity * delta);
        
        // Calculate Magnitude
        float rawSpeed = new Vector3(inputVelocity.x, 0, inputVelocity.z).magnitude;
        MoveMagnitude = Mathf.InverseLerp(0f, dashSpeed, rawSpeed);
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
        // Remove any Y velocity to avoid diagonal slam
        velocityY = 0;
        // Perform Slam
        velocityY += -slamForce;
        // Reset Cooldown
        slamTimer = slamCooldown;
        // Update stamina
        TakeStamina(slamValue);
        isSlaming = true;
    }
    public void ApplySlamImpact()
    {
        // Give enemy damage
        if (EnemyInRange())
        {
            PerformSlamDamage(50);
            HitStop();
        }
        // Effects
        isSlaming = false;
        OnLanded?.Invoke();
        OnSlamImpact?.Invoke();
        PlayerAudio.PlaySlamSound();
    }
    private void PerformSlamDamage(int amount)
    {
        Collider[] hitColliders = enemyCollider;
    
        foreach (Collider collider in hitColliders)
        {
            IDamageable damageable = collider.GetComponent<IDamageable>();
            IKnockback knockbackable = collider.GetComponent<IKnockback>();

            // Apply Damage 
            damageable?.TakeDamage(amount);
            // Apply Knockback
            Vector3 knockbackDir =  EntityHelper.GetKnockbackDirection(transform.position, collider.transform.position, 0.4f);
            knockbackable?.ApplyKnockback(knockbackDir, 50);
        }
    }
    
    // Dash
    public void ApplyDashImpulse(Vector3 dir)
    {
        dashCooldownTimer = dashCooldown;
        dashVelocity += dir.normalized * dashSpeed;
        TakeStamina(dashValue);
        PlayerAudio.PlayDashSound();
        IsDashing = true;
    }
    public void ResetDash()
    {
        dashVelocity = Vector3.zero; // IMPORTANT RESET
        IsDashing = false;
    }
    
    // Other
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
    public bool EnemyInRange()
    {
        enemyCollider = Physics.OverlapSphere(groundCheck.position, detectRadius, enemyLayer);
        return enemyCollider.Length > 0;
    }
    // Movement
    public bool IsMoving() => MoveMagnitude > 0.01f;
    // Special
    public bool CanSlam() => !IsGrounded && slamTimer <= 0 && PlayerStamina.CurrentStamina >= slamValue;
    public bool CanDash() => dashCooldownTimer <= 0 && IsMoving() && PlayerStamina.CurrentStamina >= dashValue;

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
    }
    
    #endregion

    public void ApplyKnockback(Vector3 direction, float force)
    {
        knockbackVelocity = direction.normalized * force;
    }
}