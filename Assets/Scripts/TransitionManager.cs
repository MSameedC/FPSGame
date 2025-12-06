using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;
    
    private UIDocument uiDoc;
    private VisualElement root;
    private VisualElement transitionBg;

    private void Awake()
    {
        Instance = this;
        uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;
    }

    private void Start()
    {
        transitionBg = root.Q<VisualElement>("background");
        // transitionBg.style.display = DisplayStyle.None;
        transitionBg.AddToClassList("transitionUp");
    }

    public IEnumerator StartLoading()
    {
        // Fade IN (enter scene)
        // transitionBg.style.display = DisplayStyle.Flex;
        yield return null;
        transitionBg.RemoveFromClassList("transitionUp");
        yield return new WaitForSecondsRealtime(0.5f);
    }

    public IEnumerator ExitLoading()
    {
        // Fade OUT (exit scene)
        transitionBg.AddToClassList("transitionUp");
        yield return new WaitForSecondsRealtime(0.5f);
        // transitionBg.style.display = DisplayStyle.None;
    }
}
