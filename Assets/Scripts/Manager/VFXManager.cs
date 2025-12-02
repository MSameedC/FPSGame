using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    [SerializeField] private VfxLibrary library;
    [SerializeField] private LineRenderer bulletTrail;
    
    private readonly string trailPoolId = PoolName.BulletTrailPool;
    
    // ---

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void Start()
    {
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

    public void PlayVFX(VisualEffect effect, Vector3 position, Quaternion rotation, float duration = 10)
    {
        VisualEffect visualEffect = Instantiate(effect, position, rotation);
        visualEffect.Play();
        
        // The PooledVFX component will handle auto-return
        StartCoroutine(ReturnVFXAfterDelay(visualEffect.gameObject, duration));
    }
    
    private IEnumerator ReturnVFXAfterDelay(GameObject vfxObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(vfxObj);
    }

    public void PlaySpawnEffect(Vector3 position, Quaternion rotation)
    {
        PlayVFX(library.spawn, position, rotation);
    }
    
    public void PlayEntityHit(Vector3 position)
    {
        PlayVFX(library.entityHit, position, Quaternion.identity, 5);
    }
    
    public void PLayLightWallHit(Vector3 position, Quaternion rotation)
    {
        PlayVFX(library.wallHitLight, position, rotation, 3);
    }
    
    public void PLayHeavyWallHit(Vector3 position, Quaternion rotation)
    {
        PlayVFX(library.wallHitLight, position, rotation, 5);
    }
    
    public void PLayExplosionEffect(Vector3 position)
    {
        PlayVFX(library.explosion, position, Quaternion.identity, 5);
    }
    
    public void PlayMuzzleFlash(Vector3 position, Quaternion rotation) => PlayVFX(library.muzzleFlash, position, rotation, 3);
    
    
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
