using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    // ---

    public event Action<int> OnWaveChanged;
    public event Action OnWaveCompleted;

    [SerializeField] private float spawnRadius = 15f;
    [SerializeField] private LayerMask groundLayer;

    public int CurrentWave { get; private set; }
    public int EnemiesToSpawn { get; private set; }

    // Components
    private PlayerRegistry PlayerRegistry;
    private EnemyManager EnemyManager;
    private Transform Player;

    // ---

    #region Unity

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void Start()
    {
        PlayerRegistry = PlayerRegistry.Instance;
        EnemyManager = EnemyManager.Instance;
        
        if (!PlayerRegistry) return;
        if (!Player)
            Player = PlayerRegistry.GetLocalPlayer().PlayerObj.transform;
        
        // Start Game
        StartCoroutine(StartNextWave());
    }

    #endregion

    #region Wave Sequence

    public IEnumerator StartNextWave()
    {
        // Initialize countdown
        float countDownTime = 3.75f;
        // int countDownIntTime = Mathf.RoundToInt(countDownTime);
        // Display Ui Timer
        
        yield return new WaitForSeconds(countDownTime);

        // When countdown complete, execute this sequence
        CurrentWave++;

        // Update amount of enemies to spawn
        EnemiesToSpawn = 3 + (CurrentWave * 2);

        // Spawn enemies
        for (int i = 0; i < EnemiesToSpawn; i++)
        {
            float spawnDelay = 0.2f;
            yield return new WaitForSeconds(spawnDelay);
            SpawnEnemy(GetSpawnPosition());
        }

        OnWaveChanged?.Invoke(CurrentWave);
    }
    
    public IEnumerator WaveCompleteSequence()
    {
        // Show end wave score and kills
        // yield return new WaveEndSequence() { }

        // Once that sequence is completed then show upgrade menu
        float upgradeDelay = 3.75f;
        yield return new WaitForSeconds(upgradeDelay);

        // Trigger upgrade screen, etc.
        OnWaveCompleted?.Invoke();
    }

    #endregion

    #region Methods

    private Vector3 GetSpawnPosition()
    {
        if (!Player) 
        {
            Debug.LogError("Player is null in GetSpawnPosition!");
            return Vector3.zero;
        }
        
        Vector3 playerPos = Player.transform.position;
        Vector2 randomDir = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos = playerPos + new Vector3(randomDir.x, 0, randomDir.y);

        // Optional: Ensure it's on ground
        if (Physics.Raycast(spawnPos + Vector3.up * 10, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            spawnPos = hit.point;
        }

        return spawnPos;
    }

    private void SpawnEnemy(Vector3 spawnPos) => EnemyManager.SpawnEnemy(spawnPos);

    #endregion
}
