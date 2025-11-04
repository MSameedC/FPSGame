using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    // ---

    public event Action OnAllEnemiesDead;
    public event Action<int> OnEnemyDied;

    [SerializeField] private GameObject[] basicEnemyPrefab;
    [SerializeField] private GameObject[] moderateEnemyPrefab;
    [SerializeField] private GameObject[] bossEnemyPrefab;

    private List<EnemyBase> activeEnemies = new List<EnemyBase>();
    private Queue<GameObject> enemyPool = new Queue<GameObject>();

    int poolLimit = 50;
    private int enemiesKilled;

    // ---

    #region Unity

    private void Awake() => Instance = this;

    private void Start()
    {
        InitializeEnemyPool();
    }

    #endregion

    #region Registry

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
        if (activeEnemies.Remove(enemy))
        {
            // Notify GameManager about the death
            ReturnEnemy(enemy.gameObject);

            // Add to player kill count
            enemiesKilled++;
            OnEnemyDied?.Invoke(enemy.ScoreValue);

            // Check wave completion
            if (AreAllEnemiesDead())
            {
                OnAllEnemiesDead?.Invoke();
            }
        }
    }

    #endregion

    #region Methods

    private void InitializeEnemyPool()
    {
        GameObject parentObj = new GameObject("EnemyPool");

        for (int i = 0; i <= poolLimit; i++)
        {
            GameObject enemy = Instantiate(basicEnemyPrefab[0], parentObj.transform);
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }
        
    }

    public void SpawnEnemy(Vector3 pos)
    {
        if (enemyPool.Count <= 0) return;

        GameObject enemy = enemyPool.Dequeue();
        enemy.transform.position = pos;
        enemy.SetActive(true);

        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        if (enemyBase != null)
        {
            RegisterEnemy(enemyBase);
        }
    }

    private void ReturnEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        enemyPool.Enqueue(enemy);
    }

    public void ResetData()
    {
        enemiesKilled = 0;
    }

    #endregion

    #region Data

    public float GetCurrentKillCount() => enemiesKilled;
    public bool AreAllEnemiesDead() => activeEnemies.Count == 0;

    #endregion
}
