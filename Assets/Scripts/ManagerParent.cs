using UnityEngine;

public class ManagerParent : MonoBehaviour
{
    private static ManagerParent instance;
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);   // Already exists → destroy duplicate
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
