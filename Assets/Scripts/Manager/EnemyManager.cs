using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    // ---

    public event Action OnAllEnemiesDead;
    public event Action<int> OnEnemyDied;

    [SerializeField] private GameObject[] enemyPrefab;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private LayerMask groundLayer;

    private readonly List<EnemyBase> activeEnemies = new();
    
    private int enemiesKilled;
    private const string enemyPoolId = PoolName.EnemyPool;

    // ---

    #region Unity

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void Start()
    {
        InitializePool();
    }

    private void OnDestroy()
    {
        ResetData();
    }

    #endregion

    #region Registry
    
    private void InitializePool()
    {
        PoolSystem.CreatePool(enemyPoolId, enemyPrefab[0], 10);
    }
    
    // Add enemy to tracking list
    private void RegisterEnemy(EnemyBase enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    // Remove enemy and handle death
    public void UnregisterEnemy(EnemyBase enemy)
    {
        if (!activeEnemies.Remove(enemy)) return;
        
        // Notify GameManager about the death
        PoolSystem.Return(enemyPoolId, enemy.gameObject);

        // Add to player kill count
        enemiesKilled++;
        OnEnemyDied?.Invoke(enemy.ScoreValue);

        // Check wave completion
        if (AreAllEnemiesDead())
        {
            OnAllEnemiesDead?.Invoke();
        }
    }

    #endregion

    #region Methods

    public void SpawnEnemy(Vector3 pos)
    {
        if (!PoolSystem.HasAvailable(enemyPoolId))
        {
            PoolSystem.Add(enemyPoolId, enemyPrefab[0], 10);
        }
        
        GameObject enemy = PoolSystem.Get(enemyPoolId, pos, Quaternion.identity);
        if (enemy)
        {
            CharacterController cc = enemy.GetComponent<CharacterController>();
            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();

            if (!enemyBase) return;
            
            RegisterEnemy(enemyBase);
                
            if (cc) 
            {
                cc.enabled = false;
                enemy.transform.position = GetAdjustedSpawnPosition(pos, enemyBase.useGravity); // Force position again
                cc.enabled = true;
            }
                
            enemyBase.OnSpawn(); // Reset enemy state if needed
            // Play Spawn VFX
        }
        else
        {
            Debug.LogError("Failed to get enemy from pool!");
        }
    }

    private Vector3 GetAdjustedSpawnPosition(Vector3 basePos, bool useGravity)
    {
        if (!useGravity)
        {
            // Flying enemy - spawn in air
            basePos.y += Random.Range(3f, 8f);
        }
        else
        {
            // Ground enemy - ensure on ground
            if (Physics.Raycast(basePos + Vector3.up * 10, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                basePos = hit.point + Vector3.up * 0.5f;
            }
        }
        return basePos;
    }

    #endregion

    #region Data

    public void ResetData() => enemiesKilled = 0;
    public float GetCurrentKillCount() => enemiesKilled;
    public bool AreAllEnemiesDead() => activeEnemies.Count == 0;

    #endregion
}
