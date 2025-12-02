using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private UIDocument gameHudDoc;
    [SerializeField] private UIDocument crosshairDoc;
    [SerializeField] private UIDocument pauseDoc;
    [SerializeField] private UIDocument settingsDoc;
    
    private VisualElement gameHudRoot;
    private VisualElement crosshairRoot;
    private VisualElement pauseRoot;
    private VisualElement settingsRoot;

    private Label scoreLabel;
    private Label waveLabel;

    private GameManager GameManager;
    private WaveManager WaveManager;

    // ---

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameHudRoot = gameHudDoc.rootVisualElement;
        crosshairRoot = crosshairDoc.rootVisualElement;
        
        scoreLabel = gameHudRoot.Q<Label>("score-label");
        waveLabel = gameHudRoot.Q<Label>("wave-label");
        // Components
        GameManager = GameManager.Instance;
        WaveManager = WaveManager.Instance;
        // Subscribe
        GameManager.OnScoreChanged += UpdateScore;
        WaveManager.OnWaveChanged += UpdateWave;
        InputManager.OnAimPressed += () => crosshairRoot.style.display = DisplayStyle.None;
        InputManager.OnAimReleased += () => crosshairRoot.style.display = DisplayStyle.Flex;
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

    public void ShowScreen(string screenName)
    {
        
    }
}
