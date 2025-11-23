using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class PooledVFX : MonoBehaviour, IPoolable
{
    private VisualEffect visualEffect;
    
    public void OnSpawn()
    {
        visualEffect = GetComponent<VisualEffect>();
    }
    
    public void OnDespawn()
    {
        // Clean up when returning to pool
        visualEffect.Stop();
        visualEffect.visualEffectAsset = null;
    }
    
    // Call this when you play the VFX
    public void StartLifeTimer(float duration)
    {
        StartCoroutine(ReturnAfterDuration(duration));
    }
    
    private IEnumerator ReturnAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        PoolSystem.Return(PoolName.VfxPool, gameObject);
    }
}