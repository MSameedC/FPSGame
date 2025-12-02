using System;
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

        transitionBg.AddToClassList("transitionUp");
    }

    public IEnumerator StartLoading()
    {
        transitionBg.RemoveFromClassList("transitionUp");
        
        yield return new WaitForSeconds(1);
    }

    public IEnumerator ExitLoading()
    {
        transitionBg.RemoveFromClassList("transitionUp");
        yield return new WaitForSeconds(1);
        
        transitionBg.AddToClassList("transitionUp");
    }
}
