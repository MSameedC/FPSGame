using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public event Action<int> OnScoreChanged;

    public int Score { get; private set; }

    private WaveManager WaveManager;
    private EnemyManager EnemyManager;

    // ---

    #region Unity

    void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void Start()
    {
        WaveManager = WaveManager.Instance;
        EnemyManager = EnemyManager.Instance;

        EnemyManager.OnAllEnemiesDead += HandleNextWave;
        EnemyManager.OnEnemyDied += UpdateScore;
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (EnemyManager != null)
        {
            EnemyManager.OnAllEnemiesDead -= HandleNextWave;
            EnemyManager.OnEnemyDied -= UpdateScore;
        }
    }

    #endregion

    private void HandleNextWave()
    {
        StartCoroutine(WaveManager.WaveCompleteSequence());
        StartCoroutine(WaveManager.StartNextWave());
    }

    private void UpdateScore(int scoreValue)
    {
        Score += scoreValue;
        OnScoreChanged?.Invoke(Score);
    }
}