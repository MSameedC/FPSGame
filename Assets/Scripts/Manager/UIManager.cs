using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private UIDocument gameHudDoc;
    [SerializeField] private UIDocument crosshairDoc;
    [SerializeField] private UIDocument pauseDoc;
    
    private VisualElement gameHudRoot;
    private VisualElement crosshairRoot;
    private VisualElement pauseRoot;

    private Label scoreLabel;
    private Label waveLabel;

    private GameManager GameManager;
    private WaveManager WaveManager;
    
    private bool isPaused;

    // ---

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Components
        GameManager = GameManager.Instance;
        WaveManager = WaveManager.Instance;
        // Initialize
        InitializeGameHud();
        InitializeCrosshair();
        InitializePauseMenu();
        // Subscribe
        GameManager.OnScoreChanged += UpdateScore;
        WaveManager.OnWaveChanged += UpdateWave;
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
   
    private void InitializeGameHud()
    {
        gameHudRoot = gameHudDoc.rootVisualElement;
        
        scoreLabel = gameHudRoot.Q<Label>("score-label");
        waveLabel = gameHudRoot.Q<Label>("wave-label");
        
        gameHudRoot.style.display = DisplayStyle.Flex;
    }
    private void InitializeCrosshair()
    {
        crosshairRoot = crosshairDoc.rootVisualElement;
        
        InputManager.OnAimPressed += () => crosshairRoot.style.display = DisplayStyle.None;
        InputManager.OnAimReleased += () => crosshairRoot.style.display = DisplayStyle.Flex;
        
        crosshairRoot.style.display = DisplayStyle.Flex;
    }
    

    #region PauseMenu
    
    private void InitializePauseMenu()
    {
        pauseRoot = pauseDoc.rootVisualElement;
        
        pauseRoot.Q<Button>("continue-btn").clicked += () => StartCoroutine(ExitPauseMenu());
        pauseRoot.Q<Button>("settings-btn").clicked += EnterSettings;
        pauseRoot.Q<Button>("exit-btn").clicked += () => StartCoroutine(ExitGame());
        
        InputManager.OnPausePressed += ManagePause;
        
        pauseRoot.Q<VisualElement>("Buttons").AddToClassList("transitionLeft");
        pauseRoot.style.display = DisplayStyle.None;
    }

    private void EnterSettings()
    {
        SettingsUi.Instance.EnterSettings();
    }

    private void ManagePause()
    {
        Debug.Log("Game State: " + isPaused + "Input: " + InputManager.IsGamePaused);
        if (isPaused)
        {
            if (!SettingsUi.Instance.Active)
            {
                StartCoroutine(ExitPauseMenu());
            }
        }
        else
        {
            StartCoroutine(EnterPauseMenu());
        }
    }
    
    private IEnumerator EnterPauseMenu()
    {
        pauseRoot.style.display = DisplayStyle.Flex;
    
        yield return null; // Wait one frame for UI to update
    
        pauseRoot.Q<VisualElement>("Buttons").RemoveFromClassList("transitionLeft");
        
        isPaused = true;
        InputManager.IsGamePaused = true;
        GameManager.Instance.SetState(GameState.Paused);
    
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    
        Time.timeScale = 0;
    }

    private IEnumerator ExitPauseMenu()
    {
        Time.timeScale = 1;
    
        pauseRoot.Q<VisualElement>("Buttons").AddToClassList("transitionLeft");
        
        isPaused = false;
        InputManager.IsGamePaused = false;
        GameManager.Instance.SetState(GameState.Playing);
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    
        yield return new WaitForSeconds(0.2f); // Wait for animation
    
        pauseRoot.style.display = DisplayStyle.None;
    }

    #endregion
    
    private IEnumerator ExitGame()
    {
        yield return StartCoroutine(TransitionManager.Instance.StartLoading());
        SceneLoader.Instance.LoadScene("MainMenu");
    }
    
}
