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
    private const string enemyPoolName = "Enemy";
    private const string bulletPoolName = "EnemyBullet";

    // ---

    #region Unity

    private void Awake() => Instance = this;

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
        PoolSystem.CreatePool(enemyPoolName, enemyPrefab[0], 20);
        PoolSystem.CreatePool(bulletPoolName, bulletPrefab, 100);
    }
    
    // Add enemy to tracking list
    public void RegisterEnemy(EnemyBase enemy)
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
        PoolSystem.Return(enemyPoolName, enemy.gameObject);

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
        GameObject enemy = PoolSystem.Get(enemyPoolName, pos, Quaternion.identity);

        if (enemy)
        {
            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase)
            {
                RegisterEnemy(enemyBase);
                enemyBase.OnSpawn(); // Reset enemy state if needed
            }
        }
    }

    public void SpawnBullet(Vector3 position, Vector3 direction, int damage, float speed, float knockback)
    {
        GameObject bulletObj = PoolSystem.Get(bulletPoolName, position, Quaternion.LookRotation(direction));
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
