using System;
using System.Collections;
using UnityEngine;

public enum GameState { Menu, Playing, Paused, GameOver }

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
    private TransitionManager TransitionManager;

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
        TransitionManager =  TransitionManager.Instance;

        EnemyManager.OnAllEnemiesDead += HandleNextWave;
        EnemyManager.OnEnemyDied += UpdateScore;
        
        
        // Start Game
        StartCoroutine(GameStartRoutine());
    }

    private IEnumerator GameStartRoutine()
    {
        yield return StartCoroutine(TransitionManager.ExitLoading());
        yield return new WaitForSeconds(1);
        Debug.Log("Game Start!");
        yield return new WaitForSeconds(1);
        if (startWave)
            yield return StartCoroutine(WaveManager.StartNextWave());
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
        
        // Handle UI transitions
        UIManager.Instance.ShowScreen(newState.ToString() + "Screen");
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