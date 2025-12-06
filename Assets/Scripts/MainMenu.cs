using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    private UIDocument uiDoc;
    private VisualElement root;
    
    private Button startBtn;
    private Button settingsBtn;
    private Button quitBtn;

    private void Awake()
    {
        uiDoc = GetComponent<UIDocument>();
        
        root = uiDoc.rootVisualElement;

        startBtn = root.Q<Button>("start-btn");
        settingsBtn = root.Q<Button>("settings-btn");
        quitBtn = root.Q<Button>("quit-btn");
    }
    private void Start()
    {
        startBtn.clicked += StartGame;
        settingsBtn.clicked += OpenSettings;
        quitBtn.clicked += QuitGame;
    }
    
    private void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }
    private IEnumerator StartGameRoutine()
    {
        yield return StartCoroutine(TransitionManager.Instance.StartLoading());
        SceneLoader.Instance.LoadScene("GameScene");
    }

    private void OpenSettings()
    {
        // Open Settings
        SettingsUi.Instance.EnterSettings();
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
