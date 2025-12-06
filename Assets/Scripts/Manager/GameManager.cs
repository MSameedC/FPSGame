using System;
using UnityEngine;

public enum GameState { Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameState CurrentState { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<GameState> OnStateChanged;
    
    public bool startWave = false;

    public int Score { get; private set; }

    private WaveManager WaveManager;
    private EnemyManager EnemyManager;
    private PlayerData playerData;

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
        
        // Start Game
        if (startWave)
            StartCoroutine(WaveManager.StartNextWave());
    }

    private void OnDisable()
    {
        // Clean up event subscriptions
        if (EnemyManager != null)
        {
            EnemyManager.OnAllEnemiesDead -= HandleNextWave;
            EnemyManager.OnEnemyDied -= UpdateScore;
        }
    }

    #endregion
    
    public void SetState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }

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