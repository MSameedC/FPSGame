using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class EnemyBase : MonoBehaviour, IMoveable, IDamageable, IKnockback, IPoolable
{
    public event Action OnPlayerSpotted;
    public event Action OnPlayerLost;
    public event Action OnSpawned;
    public event Action OnDespawned;
    public event Action OnAttack;
    public event Action OnHurt;
    public event Action OnDeath; 
    
    [Header("Data")]
    [SerializeField] protected EnemyData enemyData;
    [Header("Movement")]
    [SerializeField] protected float moveSmoothness = 10;
    [SerializeField] protected float rotationSpeed = 6;
    [Header("Detection")]
    [SerializeField] protected float predictionStrength = 1.2f;
    [Header("State")]
    [Range(0, 1)][SerializeField] protected float knockbackWeight;
    [Header("Flags")]
    [SerializeField] public bool useGravity = true;
    [SerializeField] private LayerMask losMask;

    #region Enemy Data Variables

    public float MinAttackRange => enemyData.attackRange.x;
    public float MaxAttackRange => enemyData.attackRange.y;
    public float DetectRange => enemyData.detectRange;
    public float AttackRate => enemyData.attackRate;
    public int ScoreValue => enemyData.scoreValue;

    #endregion

    public float CurrentHealth { get; private set; }
    
    // Movement
    public float MoveMagnitude { get; private set; }
    protected abstract Vector3 moveVelocity { get; set; }
    
    // Ground Check
    public bool IsGrounded => cc.isGrounded;
    
    // Timers
    private float attackTimer;
    private float bufferTimer;
    private float patrolTimer;
    private float waitTimer;
    private Vector3 randomDir;
    
    // Velocities
    private float gravityVelocity;
    private Vector3 knockbackVelocity;
    private Vector3 totalVelocity;
    
    // Prediction
    private Vector2 smoothedInput;
    private Vector2 inputVelocity;
    
    private Vector3 desiredMove;
    private Vector3 desiredLook;
    
    public Vector3 lastKnownPlayerPosition { get; private set; }
    
    // Components
    private Transform player;
    private PlayerRegistry playerRegistry;
    protected ProjectileManager projectileManager;
    protected CharacterController cc;

    // State machine
    private BaseState currentState;

    #region Unity

    protected virtual void Start()
    {
        // Get Component
        playerRegistry = PlayerRegistry.Instance;
        projectileManager = ProjectileManager.Instance;
        cc = GetComponent<CharacterController>();

        OnSpawn();
        FindPlayer();
    }

    public void OnSpawn() => StartCoroutine(SpawnRoutine());
    public void OnDespawn() => OnDespawned?.Invoke();

    private IEnumerator SpawnRoutine()
    {
        // Wait one frame
        yield return null;
        // Update Data
        CurrentHealth = enemyData.maxHealth;
        // Event
        OnSpawned?.Invoke();

        yield return new WaitForSeconds(0.5f);
        // Set Enemy State
        SetState(new EnemyChaseState(this));
    }

    protected virtual void Update()
    {
        float delta = Time.deltaTime;

        if (!cc) return;

        // If out of world, kill itself
        if (transform.position.y < -200) TakeDamage(99999999);

        // Common state machine update
        currentState?.Update(delta);
        UpdatePlayerSight();
        
        // Common timer updates
        if (attackTimer > 0) attackTimer -= delta;
    }

    protected virtual void LateUpdate()
    {
        float delta = Time.deltaTime;
        if (!cc) return;
        
        // Common state machine update
        currentState?.LateUpdate(delta);
        ApplyGravity(delta);
        ApplyMovementSmooth(delta);
        // Velocity
        if (moveVelocity.magnitude > 0.01f)
            moveVelocity = Vector3.Lerp(moveVelocity, Vector3.zero, 10f * delta);
        if (knockbackVelocity.magnitude > 0.1f)
            knockbackVelocity = Vector3.Slerp(knockbackVelocity, Vector3.zero, 3f * delta);
        
        totalVelocity = moveVelocity + knockbackVelocity + Vector3.up * gravityVelocity;
        cc.Move(totalVelocity * delta);
    }

    private void ApplyMovementSmooth(float delta)
    {
        // Smooth movement
        moveVelocity = Vector3.Lerp(moveVelocity,
            desiredMove * enemyData.moveSpeed,
            moveSmoothness * delta);

        // Smooth rotation
        if (desiredLook.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(desiredLook);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * delta
            );
        }
    }
    #endregion

    #region State Machine

    public void SetState(BaseState state)
    {
        currentState?.Exit();
        currentState = state;
        currentState.Enter();
    }

    #endregion

    private void FindPlayer()
    {
        if (player != null) return; // If we have player, don't execute

        PlayerData playerData = playerRegistry.GetLocalPlayer();
        
        if (playerData == null)
        {
            // Fallback: Find by tag
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        else
        {
            // Assuming PlayerData has reference to GameObject
            player = playerData.PlayerObj.transform;
        }
    }

    #region Behaviour

    #region Movement/Detection

    private void ApplyGravity(float delta)
    {
        if (useGravity)
        {
            if (cc.isGrounded && gravityVelocity < 0)
            {
                gravityVelocity = GravityData.groundStickForce;
                return;
            }
        
            float maxFallSpeed = 20;
            float gravityMultiplier = gravityVelocity < 0 ? GravityData.fallStrength : 1f;
            gravityVelocity += GravityData.gravity * gravityMultiplier * delta;
            gravityVelocity = Mathf.Max(gravityVelocity, -maxFallSpeed);
        }
        else
        {
            gravityVelocity = 0f; 
        }
    }

    // Detection
    private float GetDistanceToPlayer()
    {
        return !player ? Mathf.Infinity : Vector3.Distance(transform.position, player.position);
    }
    public Vector3 GetPredictedDirectionToPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
    
        // Smooth input
        float smoothTime = Mathf.Lerp(0.1f, 0.5f, distanceToPlayer / 20f); // Smoother when farther
        smoothedInput = Vector2.SmoothDamp(smoothedInput, InputManager.MoveInput, ref inputVelocity, smoothTime);
    
        // Distance-based prediction strength
        float predictionMultiplier = Mathf.Clamp(distanceToPlayer / 8f, 0.05f, 1f);
        
        // Always predict slightly ahead, even when player is standing still
        Vector3 baseOffset = transform.forward * 0.1f; // Small forward prediction
        Vector3 predictOffset = new Vector3(smoothedInput.x, 0, smoothedInput.y) *
                                (predictionStrength * predictionMultiplier);
    
        Vector3 predictedPlayerPosition = player.position + predictOffset;
        return (predictedPlayerPosition - transform.position).normalized;
    }

    private Vector3 GetDirectionToPlayer()
    {
        return (player.position - transform.position).normalized;
    }

    public Vector3 GetDirectionAwayFromPlayer()
    {
        return (transform.position - player.position).normalized;
    }

    private Vector3 GetRandomDirection()
    {
        return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
    }

    public bool PlayerSpotted()
    {
        return GetDistanceToPlayer() <= DetectRange;
    }
    
    public bool HasClearLineOfSight()
    {
        if (!player) return false;

        Vector3 origin = transform.position + Vector3.up * 1.6f;  // eye height
        Vector3 dir = (player.position - origin).normalized;
        float dist = Vector3.Distance(origin, player.position);

        // Only raycast against layers that can block sight
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, losMask, QueryTriggerInteraction.Ignore))
        {
            // If the FIRST thing hit is player, LOS = true
            return hit.collider.CompareTag("Player");
        }

        // Nothing hit? = Player is visible
        return true;
    }
    
    public void InvokeOnPlayerSpotted() => OnPlayerSpotted?.Invoke();
    public void InvokeOnPlayerLost() => OnPlayerLost?.Invoke();

    private void UpdatePlayerSight()
    {
        if (!PlayerSpotted()) return;
        lastKnownPlayerPosition = player.position;
    }
    
    public bool IsTooClose()
    {
        return GetDistanceToPlayer() < MinAttackRange;
    }
    public bool IsTooFar()
    {
        return GetDistanceToPlayer() > MaxAttackRange;
    }

    // Movement
    public void SetDesiredMove(Vector3 dir) => desiredMove = dir;
    public void SetDesiredLook(Vector3 dir) => desiredLook = dir;
    public virtual void MoveTo(Vector3 direction, float delta)
    {
        if (direction.magnitude > 1f)
            direction.Normalize();

        float rawSpeed = new Vector3(moveVelocity.x, 0, moveVelocity.z).magnitude;
        MoveMagnitude = Mathf.InverseLerp(0f, enemyData.moveSpeed, rawSpeed);
    }

    private void LookAt(Vector3 direction, float delta)
    {
        if (direction.magnitude > 1f)
            direction.Normalize();
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * delta);
    }

    public void Patrol(float delta)
    {
        // Handle direction change
        if (bufferTimer <= 0)
        {
            randomDir = GetRandomDirection();
            bufferTimer = Random.Range(2f, 6f);
        }
        else
        {
            bufferTimer -= delta;
        }

        // Patrol state machine
        if (patrolTimer > 0)
        {
            patrolTimer -= delta;
            MoveTo(randomDir, delta);
            LookAt(randomDir, delta);

            if (!(patrolTimer <= 0)) return;
            waitTimer = Random.Range(2f, 6f);
            Stop();
        }
        else if (waitTimer > 0)
        {
            waitTimer -= delta;
            Stop();

            if (waitTimer <= 0)
            {
                patrolTimer = Random.Range(2f, 6f);
            }
        }
        else
        {
            patrolTimer = Random.Range(2f, 6f);
        }
    }
    
    public virtual void Stop()
    {
        moveVelocity = Vector3.zero;
    }

    #endregion

    #region Attack

    public bool CanAttack()
    {
        return attackTimer <= 0 && IsInAttackRange();
    }
    public virtual bool IsAttackComplete() { return true; } // By default, set true for non-animated enemies
    public bool IsInAttackRange()
    {
        float distance = GetDistanceToPlayer();
        return distance >= MinAttackRange && distance <= MaxAttackRange;
    }
    
    public void InvokeAttackEvent() => OnAttack?.Invoke();

    public abstract void Attack();
    
    public virtual void ResetAttackData()
    {
        attackTimer = AttackRate;
    }

    #endregion

    #region Knockback

    public void ApplyKnockback(Vector3 direction, float force)
    {
        knockbackVelocity = direction.normalized * (force * knockbackWeight);
    }

    #endregion

    #region Damage/Death

    public void InvokeHurtEvent() => OnHurt?.Invoke();
    public void InvokeDeathEvent() => OnDeath?.Invoke();

    // Damage
    public virtual void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        SetState(new EnemyHurtState(this));
    }

    // Death
    public abstract void OnDeathEnter();
    protected abstract IEnumerator WaitForDeathCompletion();
    public IEnumerator ReturnToPoolAfterDeath()
    {
        yield return StartCoroutine(WaitForDeathCompletion());  // Separately defined per enemy
        yield return null;  // Wait one frame after animation death completion
        EnemyManager.Instance.UnregisterEnemy(this);
    }

    #endregion

    #endregion
}