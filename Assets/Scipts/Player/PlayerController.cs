using System;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPlayerState
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
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    // Timers
    private float slamTimer = 0;
    private float jumpTimer = 0;
    private float coyoteTimer = 0;
    private float currentSpeed = 0;
    private float jumpBufferTimer = 0;
    private float dashCooldownTimer = 0;

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

    private bool jumpConsumed;

    // Ground info
    public bool IsGrounded { get; private set; }
    public Vector3 GroundNormal { get; private set; }

    private float velocityY { get; set; }
    public bool IsDashing { get; set; }
    public bool IsSlaming { get; private set; }

    private PlayerAudio PlayerAudio { get; set; }
    private PlayerStamina PlayerStamina { get; set; }

    // Components
    private PlayerState currentState;
    private CharacterController CC;
    private IInputProvider input;
    private Transform Player;
    private Camera Cam;

    // Events
    public event Action OnJumped;
    public event Action OnLanded;
    public event Action OnSlamImpact;

    // Input flags (auto-reset after use)
    private bool jumpPressedThisFrame;
    private bool dashPressedThisFrame;
    private bool slamPressedThisFrame;

    // ---

    #region Unity

    private void Awake()
    {
        input = GetComponent<IInputProvider>();
        CC = GetComponent<CharacterController>();
        PlayerAudio = GetComponent<PlayerAudio>();
        PlayerStamina = GetComponent<PlayerStamina>();
    }

    private void Start()
    {
        Player = transform;
        Cam = Camera.main;

        // Subscribe to input events
        input.OnJumpPressed += OnJumpInput;
        input.OnDashPressed += OnDashInput;
        input.OnSlamPressed += OnSlamInput;

        SetState(new GroundedState(this));
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (input != null)
        {
            input.OnJumpPressed -= OnJumpInput;
            input.OnDashPressed -= OnDashInput;
            input.OnSlamPressed -= OnSlamInput;
        }
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

        // Process input flags
        ProcessInputs();

        // State update
        currentState?.Update(delta);

        // Update gravity
        ApplyGravity(delta);

        // Set timer
        if (IsGrounded)
        {
            wallJumpVelocity = Vector3.zero;
            coyoteTimer = coyoteTime;

            if (input.MoveInput != Vector2.zero)
                PlayerAudio.PlayFootStep(delta);
            else
                PlayerAudio.ResetFootStep();
        }
    }

    private void LateUpdate()
    {
        float delta = Time.deltaTime;

        currentState?.LateUpdate(delta);
        Movement(delta);
    }

    #endregion

    public void SetState(PlayerState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    #region  Input event handlers

    private void OnJumpInput() => jumpPressedThisFrame = true;
    private void OnDashInput() => dashPressedThisFrame = true;
    private void OnSlamInput() => slamPressedThisFrame = true;

    private void ProcessInputs()
    {
        // Jump buffer from input events
        if (jumpPressedThisFrame)
        {
            jumpBufferTimer = jumpBufferTime;
            jumpPressedThisFrame = false;
        }

        // Let the current state handle dash/slam inputs
        if (dashPressedThisFrame)
        {
            currentState?.OnDashInput();
            dashPressedThisFrame = false;
        }

        if (slamPressedThisFrame)
        {
            currentState?.OnSlamInput();
            slamPressedThisFrame = false;
        }
    }

    #endregion

    private void Movement(float delta)
    {
        if (IsDashing || IsSlaming)
        {
            Vector3 finalMove = dashVelocity + Vector3.up * velocityY;
            CC.Move(finalMove * delta);
            return;
        }

        Vector2 moveInput = input.MoveInput;
        float targetSpeed = moveInput.magnitude > 0 ? walkSpeed : 0;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref currentSmoothSpeed, delta * moveSmoothness);

        Vector3 moveDirection = Player.forward * moveInput.y + Player.right * moveInput.x;
        targetDragSnappiness = Mathf.Lerp(targetDragSnappiness, IsGrounded ? groundDrag : airDrag, delta * dragSnappiness);

        Vector3 targetPosition = moveDirection * currentSpeed;
        inputVelocity = Vector3.Lerp(inputVelocity, targetPosition, targetDragSnappiness);

        // Wall jump handling
        if (wallJumpVelocity != Vector3.zero)
        {
            wallJumpVelocity = Vector3.Lerp(wallJumpVelocity, Vector3.zero, delta * wallJumpDecay);

            float inputStrength = moveInput.magnitude;
            if (inputStrength > 0.1f)
            {
                wallJumpVelocity = Vector3.Lerp(wallJumpVelocity, inputVelocity, targetDragSnappiness);
            }
        }

        // ALWAYS combine velocities and move
        finalVelocity = inputVelocity + wallJumpVelocity + Vector3.up * velocityY;
        CC.Move(finalVelocity * delta);

        float rawSpeed = new Vector3(inputVelocity.x, 0, inputVelocity.z).magnitude;
        MoveMagnitude = Mathf.InverseLerp(0f, dashSpeed, rawSpeed);
    }

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

    public void PerformJump()
    {
        velocityY = Mathf.Sqrt(-GravityData.gravity * jumpHeight);
        jumpTimer = jumpCooldown;
        coyoteTimer = 0f; // Consume coyote time
        OnJumped?.Invoke();
    }

    public void OnLand()
    {
        OnLanded?.Invoke();
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

    private void TakeStamina(int amount) => PlayerStamina.TakeStamina(amount);

    public void ApplySlamStart()
    {
        velocityY = 0;
        velocityY += -slamForce;
        slamTimer = slamCooldown;
        TakeStamina(slamValue);
        IsSlaming = true;
    }

    public void ApplySlamImpact()
    {
        IsSlaming = false;
        OnLanded?.Invoke();
        OnSlamImpact?.Invoke();
        PlayerAudio.PlaySlamSound();
    }

    public void ApplyDashImpulse(Vector3 dir)
    {
        dashCooldownTimer = dashCooldown;
        dashVelocity = dir.normalized * dashSpeed;
        TakeStamina(dashValue);
        PlayerAudio.PlayDashSound();
    }

    public Vector3 CaptureDirection()
    {
        Vector2 moveInput = input.MoveInput;
        return moveInput != Vector2.zero ? (Cam.transform.forward * moveInput.y + Cam.transform.right * moveInput.x).normalized : Cam.transform.forward.normalized;
    }

    private void UpdateGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        if (Physics.SphereCast(origin, CC.radius * 0.9f, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask))
        {
            IsGrounded = true;
            GroundNormal = hit.normal;
        }
        else
        {
            IsGrounded = false;
            GroundNormal = Vector3.up;
        }
    }

    #region Bools

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
        if (IsGrounded)
        {
            jumpConsumed = false;
            return false;
        }

        if (CheckWall(out Vector3 normal))
        {
            lastWallNormal = normal;
            if (input.IsJumping && !jumpConsumed && jumpTimer <= 0f)
            {
                jumpConsumed = true;
                return true;
            }
        }

        if (!CheckWall(out _) || !input.IsJumping)
        {
            jumpConsumed = false;
        }

        return false;
    }

    public bool CanJump()
    {
        if (jumpBufferTimer > 0f && coyoteTimer > 0f && jumpTimer <= 0f)
        {
            jumpBufferTimer = 0f;
            return true;
        }
        return false;
    }

    public bool IsMoving() => MoveMagnitude > 0.01f;
    public bool CanSlam() => !IsGrounded && slamTimer <= 0 && PlayerStamina.CurrentStamina >= slamValue;
    public bool CanDash() => dashCooldownTimer <= 0 && IsMoving() && PlayerStamina.CurrentStamina >= dashValue;

    #endregion
}