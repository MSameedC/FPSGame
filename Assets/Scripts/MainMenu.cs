using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    private UIDocument uiDoc;
    private VisualElement root;
    
    private Button startBtn;
    private Button settingsBtn;
    private Button quitBtn;
    
    private TransitionManager TransitionManager;

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
        TransitionManager = TransitionManager.Instance;
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
        yield return StartCoroutine(TransitionManager.StartLoading());
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OpenSettings()
    {
        // Open Settings
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
