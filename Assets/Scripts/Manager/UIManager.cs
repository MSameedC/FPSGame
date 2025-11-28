using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private UIDocument uiDoc;
    private VisualElement root;

    private Label scoreLabel;
    private Label waveLabel;

    private GameManager GameManager;
    private WaveManager WaveManager;

    // ---

    private void Awake()
    {
        uiDoc = GetComponent<UIDocument>();
    }

    private void Start()
    {
        root = uiDoc.rootVisualElement;

        scoreLabel = root.Q<Label>("score-label");
        waveLabel = root.Q<Label>("wave-label");
        // Components
        GameManager = GameManager.Instance;
        WaveManager = WaveManager.Instance;
        // Subscribe
        GameManager.OnScoreChanged += UpdateScore;
        WaveManager.OnWaveChanged += UpdateWave;
        // Initialize
        UpdateScore(0);
        UpdateWave(0);
    }

    private void OnDisable()
    {
        GameManager.OnScoreChanged -= UpdateScore;
        WaveManager.OnWaveChanged -= UpdateWave;
    }

    private void UpdateScore(int currentScore)
    {
        scoreLabel.text = "SCORE: " + currentScore;
    }

    private void UpdateWave(int currentWave)
    {
        waveLabel.text = currentWave.ToString();
    }
}
