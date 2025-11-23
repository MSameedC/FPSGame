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
    [SerializeField] protected EnemyData Data;
    [Header("Movement")]
    [SerializeField] protected float moveSmoothness = 10;
    [SerializeField] protected float rotationSpeed = 6;
    [Header("Detection")]
    [SerializeField] protected float predictionStrength = 1.2f;
    [Header("State")]
    [Range(0, 1)][SerializeField] protected float knockbackWeight;
    [SerializeField] public bool useGravity = true;

    #region Enemy Data Variables

    public float MinAttackRange => Data.attackRange.x;
    public float MaxAttackRange => Data.attackRange.y;
    public float DetectRange => Data.detectRange;
    public float AttackRate => Data.attackRate;
    public int ScoreValue => Data.scoreValue;

    #endregion

    public float CurrentHealth { get; private set; }
    
    public Vector3 lastKnownPlayerPosition { get; private set; }

    private Transform player;
    protected CharacterController CharacterController;

    public float MoveMagnitude { get; private set; }
    public bool IsGrounded => CharacterController.isGrounded;

    protected abstract Vector3 moveVelocity { get; set; }

    private float attackTimer;

    private float bufferTimer;
    private float patrolTimer;
    private float waitTimer;
    private Vector3 randomDir;

    private float gravityVelocity;
    private Vector3 knockbackVelocity;
    private Vector3 totalVelocity;

    private PlayerRegistry PlayerRegistry;
    protected EnemyManager enemyManager;

    // State machine
    private BaseState currentState;

    #region Unity

    protected virtual void Start()
    {
        // Get Component
        PlayerRegistry = PlayerRegistry.Instance;
        enemyManager = EnemyManager.Instance;

        CharacterController = GetComponent<CharacterController>();

        OnSpawn();
        FindPlayer();
    }

    public void OnSpawn()
    {
        OnSpawned?.Invoke();
        // Set Data
        CurrentHealth = Data.maxHealth;
        SetState(new EnemyPatrolState(this));
    }

    public void OnDespawn()
    {
        OnDespawned?.Invoke();
    }

    protected virtual void Update()
    {
        float delta = Time.deltaTime;

        if (!CharacterController) return;

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

        if (!CharacterController) return;
        
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
        CharacterController.Move(totalVelocity * delta);
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

        PlayerData playerData = PlayerRegistry.GetLocalPlayer();
        
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
        if (CharacterController.isGrounded && gravityVelocity < 0)
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
        Vector2 predictInput = InputManager.MoveInput;
        
        Vector3 predictOffset = new Vector3(predictInput.x, 0, predictInput.y) * predictionStrength;
        Vector3 predictedPlayerPosition = player.position + predictOffset;
        Vector3 predictionDirection = (predictedPlayerPosition - transform.position).normalized;
        
        return predictionDirection;
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
        MoveMagnitude = Mathf.InverseLerp(0f, Data.moveSpeed, rawSpeed);
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
        enemyManager.UnregisterEnemy(this);
    }

    #endregion

    #endregion
}