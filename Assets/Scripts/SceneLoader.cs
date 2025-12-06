using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    
    // ---
    
    private void Awake()
    {
        Instance = this;
    }
    
    public void LoadScene(string sceneName, bool useTransition = true)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, useTransition));
    }
    
    private IEnumerator LoadSceneRoutine(string sceneName, bool useTransition)
    {
        if (useTransition)
        {
            // Fade OUT (hide current scene)
            yield return StartCoroutine(TransitionManager.Instance.StartLoading());
        }
        
        // // Load new scene ASYNC
        // AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        // asyncLoad.allowSceneActivation = false;
        //
        // // Wait for scene to load (90%)
        // while (asyncLoad.progress < 0.9f)
        //     yield return null;
        //
        // // Now activate the scene
        // asyncLoad.allowSceneActivation = true;
        //
        // // Wait one frame for scene to initialize
        // yield return null;
        
        SceneManager.LoadScene(sceneName);
        
        if (useTransition)
        {
            // Fade IN (show new scene)
            yield return StartCoroutine(TransitionManager.Instance.ExitLoading());
        }
        
        // CRITICAL: Reset Time.timeScale after scene is fully loaded
        // Time.timeScale = 1f;
    }
}