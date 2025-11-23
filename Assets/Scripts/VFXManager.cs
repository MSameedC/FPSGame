using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;
    private readonly string poolName = PoolName.VfxPool;
    
    // ---

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void Start()
    {
        GameObject vfxObject = new GameObject("VFXObject");
        vfxObject.AddComponent<VisualEffect>();
        vfxObject.AddComponent<PooledVFX>();
        
        PoolSystem.CreatePool(poolName, vfxObject, 100);
    }

    public void TriggerHitStop(float duration)
    {
        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }

    public void PlayVFX(VisualEffectAsset effect, Vector3 position, Quaternion rotation, float duration = 10)
    {
        GameObject vfxObj = PoolSystem.Get(poolName, position, rotation);
        VisualEffect visualEffect = vfxObj.GetComponent<VisualEffect>();
        visualEffect.visualEffectAsset = effect;
        visualEffect.Play();
        
        // The PooledVFX component will handle auto-return
        PooledVFX pooledVFX = vfxObj.GetComponent<PooledVFX>();
        pooledVFX.StartLifeTimer(duration);
    }
}
