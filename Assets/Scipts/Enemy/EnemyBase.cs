using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [SerializeField] protected EnemyData Data;

    [Header("Movement")]
    [SerializeField] protected float moveSmoothness = 10;
    [SerializeField] protected float rotationSpeed = 6;

    #region Enemy Data Variables

    public float MinAttackRange => Data.attackRange.x;
    public float MaxAttackRange => Data.attackRange.y;
    public float DetectRange => Data.detectRange;
    public float AttackRate => Data.attackRate;
    public int ScoreValue => Data.scoreValue;

    #endregion
    
    public float CurrentHealth { get; protected set; }
    public Transform player { get; protected set; }

    // Private
    private float attackTimer;
    private PlayerRegistry PlayerRegistry;
    private EnemyManager EnemyManager;


    // State machine
    private EnemyState currentState;

    #region Unity

    protected virtual void Start()
    {
        // Get Component
        PlayerRegistry = PlayerRegistry.Instance;
        EnemyManager = EnemyManager.Instance;

        // Set Data
        CurrentHealth = Data.maxHealth;

        FindPlayer();
        SetState(new PatrolState(this));
    }

    protected virtual void Update()
    {
        float delta = Time.deltaTime;

        // If out of world, kill itself
        if (transform.position.y < -200) TakeDamage(99999999);

        // Common state machine update
        currentState?.Update(delta);

        // Common timer updates
        if (attackTimer > 0) attackTimer -= delta;
    }

    #endregion

    #region State Machine

    public void SetState(EnemyState state)
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

    // Detection
    public float GetDistanceToPlayer()
    {
        return !player ? Mathf.Infinity : Vector3.Distance(transform.position, player.position);
    }
    public Vector3 GetDirectionTowardsPlayer()
    {
        return (player.position - transform.position).normalized;
    }
    public Vector3 GetDirectionAwayFromPlayer()
    {
        return (transform.position - player.position).normalized;
    }

    protected Vector3 GetRandomDirection()
    {
        return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
    }
    public bool IsInSight()
    {
        return GetDistanceToPlayer() <= DetectRange;
    }
    public bool IsTooClose()
    {
        return GetDistanceToPlayer() < MinAttackRange;
    }
    public bool IsTooFar()
    {
        return GetDistanceToPlayer() > MaxAttackRange;
    }

    /// bool IsMoveComplete()
    /// 
    /// This is useful but only additional to use for a simple game
    /// Mainly can be used to improve AI. Move for a duration like 1 sec,
    /// movement complete, check environment again, move.
    /// Or
    /// Chase player, if player is too far, move to last position,
    /// move complete, switch to patrol state
    /// 
    /// bool IsMoveComplete()

    // Movement
    public virtual void Move(float delta, Vector3 direction)
    {
        if (direction.magnitude > 1f)
            direction.Normalize();
    }
    public virtual void LookAt(float delta, Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * delta);
    }
    public virtual void Retreat(float delta) 
    {
        Move(delta, GetDirectionAwayFromPlayer());
        LookAt(delta, GetDirectionTowardsPlayer());
    } // This may be same for every enemy, Just move away
    public virtual void Patrol(float delta) { } // This may be same for every enemy, Just move around and look for player
    public virtual void Stop() { } // This may be same for every enemy, Just stop moving

    #endregion

    #region Attack

    public virtual bool CanAttack() 
    { 
        return attackTimer <= 0 && IsInAttackRange(); 
    }
    public virtual bool IsAttackComplete() { return true; } // By default, set true for non-animated enemies
    public virtual bool IsInAttackRange()
    {
        float distance = GetDistanceToPlayer();
        return distance >= MinAttackRange && distance <= MaxAttackRange;
    }

    public abstract void Attack();
    public virtual void UpdateAttackPosition(float delta) { }
    public virtual void ResetAttackData()
    {
        attackTimer = AttackRate;
    }
    
    #endregion
    
    #region Damage/Death
    
    // Damage
    public virtual void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0) Die();
    }

    // Death
    protected abstract void OnDeathEnter();
    
    private void Die()
    {
        OnDeathEnter();
        SetState(new DeadState(this));
        StartCoroutine(ReturnToPoolAfterDeath());
    }

    protected abstract IEnumerator WaitForDeathCompletion();
    private IEnumerator ReturnToPoolAfterDeath()
    {
        yield return StartCoroutine(WaitForDeathCompletion());  // Separately defined per enemy
        yield return null;  // Wait one frame after animation death completion
        EnemyManager.UnregisterEnemy(this);
    }

    #endregion

    #endregion
}