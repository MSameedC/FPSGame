using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    // ---

    public event Action OnAllEnemiesDead;
    public event Action<int> OnEnemyDied;

    [SerializeField] private GameObject[] enemyPrefab;
    [SerializeField] private GameObject bulletPrefab;

    private readonly List<EnemyBase> activeEnemies = new();
    
    private int enemiesKilled;
    private const string enemyPoolId = PoolName.EnemyPool;
    private const string bulletPoolId = PoolName.BulletPool;

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
        PoolSystem.CreatePool(enemyPoolId, enemyPrefab[0], 50);
        PoolSystem.CreatePool(bulletPoolId, bulletPrefab, 100);
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
                enemy.transform.position = pos; // Force position again
                cc.enabled = true;
            }
                
            enemyBase.OnSpawn(); // Reset enemy state if needed
        }
        else
        {
            Debug.LogError("Failed to get enemy from pool!");
        }
    }

    public void SpawnBullet(Vector3 position, Vector3 direction, int damage, float speed, float knockback)
    {
        GameObject bulletObj = PoolSystem.Get(bulletPoolId, position, Quaternion.LookRotation(direction));
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.Initialize(damage, speed, knockback, direction);
    }

    #endregion

    #region Data

    public void ResetData() => enemiesKilled = 0;
    public float GetCurrentKillCount() => enemiesKilled;
    public bool AreAllEnemiesDead() => activeEnemies.Count == 0;

    #endregion
}
