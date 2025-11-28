using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;
    
    [SerializeField] private LineRenderer bulletTrail;
    [SerializeField] private GameObject vfxObject;
    
    private readonly string VfxPoolId = PoolName.VfxPool;
    private readonly string trailPoolId = PoolName.BulletTrailPool;
    
    // ---

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void Start()
    {
        vfxObject.SetActive(false);
        bulletTrail.gameObject.SetActive(false);
        
        PoolSystem.CreatePool(VfxPoolId, vfxObject, 10);
        PoolSystem.CreatePool(trailPoolId, bulletTrail.gameObject, 10);
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
        if (!PoolSystem.HasAvailable(VfxPoolId))
        {
            PoolSystem.Add(VfxPoolId, vfxObject, 5);
            return;
        }
        
        GameObject vfxObj = PoolSystem.Get(VfxPoolId, position, rotation);
        VisualEffect visualEffect = vfxObj.GetComponent<VisualEffect>();
        visualEffect.visualEffectAsset = effect;
        visualEffect.Play();
        
        // The PooledVFX component will handle auto-return
        PooledVFX pooledVFX = vfxObj.GetComponent<PooledVFX>();
        pooledVFX.StartLifeTimer(duration);
    }
    
    public void ShowTrail(Vector3 start, Vector3 end)
    {
        if (!PoolSystem.HasAvailable(trailPoolId))
        {
            PoolSystem.Add(trailPoolId, bulletTrail.gameObject, 5);
            return;
        }
        
        GameObject trail = PoolSystem.Get(trailPoolId, start, Quaternion.identity);
        LineRenderer line = trail.GetComponent<LineRenderer>();
        
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    
        // Instead of coroutine, use simple destroy
        StartCoroutine(ReturnTrailAfterDelay(trail, 0.05f));
    }
    
    private IEnumerator ReturnTrailAfterDelay(GameObject trail, float delay)
    {
        yield return new WaitForSeconds(delay);
        PoolSystem.Return(trailPoolId, trail);
    }
}
