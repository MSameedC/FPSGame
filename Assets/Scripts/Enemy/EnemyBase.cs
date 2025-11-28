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
        // Set Enemy State
        SetState(new EnemyChaseState(this));
        // Update Data
        CurrentHealth = enemyData.maxHealth;
        // Event
        OnSpawned?.Invoke();
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
        
        // Apply gravity only if enabled
        if (useGravity)
        {
            ApplyGravity(delta);
        }
        else
        {
            gravityVelocity = 0f; // No gravity for flying enemies
        }
        
        if (moveVelocity.magnitude > 0.01f)
            moveVelocity = Vector3.Lerp(moveVelocity, Vector3.zero, 10f * delta);

        if (knockbackVelocity.magnitude > 0.1f)
            knockbackVelocity = Vector3.Slerp(knockbackVelocity, Vector3.zero, 3f * delta);

        // Velocity
        totalVelocity = moveVelocity + knockbackVelocity + Vector3.up * gravityVelocity;
        cc.Move(totalVelocity * delta);
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
        Vector3 baseOffset = transform.forward * 0.5f; // Small forward prediction
        Vector3 predictOffset = new Vector3(smoothedInput.x, 0, smoothedInput.y) * (predictionStrength * predictionMultiplier) + baseOffset;
    
        Vector3 predictedPlayerPosition = player.position + predictOffset;
        return (predictedPlayerPosition - transform.position).normalized;
    }

    private Vector3 GetDirectionToPlayer()
    {
        return (player.position - transform.position).normalized;
    }

    private Vector3 GetDirectionAwayFromPlayer()
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

        Vector3 directionToPlayer = GetDirectionToPlayer();
        float distanceToPlayer = GetDistanceToPlayer();
    
        // Raycast to check for obstacles (including other enemies)
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer))
        {
            // If we hit something that's NOT the player, line of sight is blocked
            return hit.transform == player;
        }
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
    public virtual void MoveTo(Vector3 direction, float delta)
    {
        if (direction.magnitude > 1f)
            direction.Normalize();

        float rawSpeed = new Vector3(moveVelocity.x, 0, moveVelocity.z).magnitude;
        MoveMagnitude = Mathf.InverseLerp(0f, enemyData.moveSpeed, rawSpeed);
    }

    public void LookAt(Vector3 direction, float delta)
    {
        if (direction.magnitude > 1f)
            direction.Normalize();
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * delta);
    }
    
    public void Retreat(float delta)
    {
        MoveTo(GetDirectionAwayFromPlayer(), delta);
        LookAt(GetDirectionToPlayer(), delta);
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

    public void Chase(Vector3 direction, float delta)
    {
        MoveTo(direction, delta);
        LookAt(direction, delta);
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