using System.Collections;
using UnityEngine;

public static class PlayerTrailManager
{
    private const string poolName = "PlayerBulletTrail";
    private static bool isInitialized;
    private static MonoBehaviour coroutineRunner;

    // Initialize the system
    public static void Initialize(MonoBehaviour runner, TrailRenderer trailPrefab, int poolSize = 20)
    {
        if (isInitialized) return;
        
        coroutineRunner = runner;
        PoolSystem.CreatePool(poolName, trailPrefab.gameObject, poolSize);
        isInitialized = true;
        
        Debug.Log("PlayerTrailManager initialized!");
    }

    // Show a bullet trail
    public static void ShowTrail(Vector3 start, Vector3 end, float speed = 100f)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("PlayerTrailManager not initialized!");
            return;
        }

        GameObject trailObj = PoolSystem.Get(poolName, start, Quaternion.identity);
        coroutineRunner.StartCoroutine(AnimateTrail(trailObj.GetComponent<TrailRenderer>(), end, speed));
    }

    private static IEnumerator AnimateTrail(TrailRenderer trail, Vector3 endPoint, float speed)
    {
        Vector3 startPos = trail.transform.position;
        float distance = Vector3.Distance(startPos, endPoint);
        float duration = distance / speed;
        
        float time = 0;
        while (time < 1)
        {
            if (trail)
            {
                trail.transform.position = Vector3.Lerp(startPos, endPoint, time);
                time += Time.deltaTime / duration;
            }
            yield return null;
        }

        if (!trail) yield break;
        trail.transform.position = endPoint;
        yield return new WaitForSeconds(trail.time);
        PoolSystem.Return(poolName, trail.gameObject);
    }
}