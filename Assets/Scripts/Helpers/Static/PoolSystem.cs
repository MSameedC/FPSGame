using System.Collections.Generic;
using UnityEngine;

public static class PoolSystem
{
    private static Transform poolParent;
    private static readonly Dictionary<string, Queue<GameObject>> pools = new();
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        poolParent = new GameObject("PoolManager").transform;
        Object.DontDestroyOnLoad(poolParent);
        Debug.Log("Pool system initialized!");
    }
    
    // Create a new pool
    public static void CreatePool(string poolId, GameObject prefab, int size)
    {
        if (pools.ContainsKey(poolId)) return;
        
        GameObject poolContainer = new GameObject(poolId);
        poolContainer.transform.SetParent(poolParent);
        
        Queue<GameObject> queue = new();
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Object.Instantiate(prefab, poolContainer.transform);
            obj.name = $"{poolId}_{i}";
            obj.SetActive(false);
            queue.Enqueue(obj);
        }

        pools[poolId] = queue;
    }

    public static void Add(string poolId, GameObject prefab, int additionalSize)
    {
        if (!pools.ContainsKey(poolId))
        {
            Debug.LogWarning($"Pool {poolId} is empty or doesn't exist!");
            CreatePool(poolId, prefab, additionalSize);
            return;
        }
        
        Transform poolContainer = poolParent.Find(poolId);
        if (!poolContainer)
        {
            Debug.LogWarning($"Pool {poolId} is empty or doesn't exist!");
            return;
        }
            
        Queue<GameObject> queue = pools[poolId];
        int startCount = queue.Count;
            
        for (int i = 0; i < additionalSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab, poolContainer.transform);
            obj.name = $"{poolId}_{startCount + i}";
            obj.SetActive(false);
            queue.Enqueue(obj);
        }
    }
    
    // Get object from pool
    public static GameObject Get(string poolId, Vector3 position, Quaternion rotation)
    {
        if (pools.ContainsKey(poolId) && pools[poolId].Count > 0)
        {
            GameObject obj = pools[poolId].Dequeue();
            obj.SetActive(true);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            
            IPoolable poolable = obj.GetComponent<IPoolable>();
            poolable?.OnSpawn();
            
            return obj;
        }
        
        Debug.LogWarning($"Pool {poolId} is empty or doesn't exist!");
        return null;
    }

    // Return object to pool
    public static void Return(string poolId, GameObject obj)
    {
        if (!pools.ContainsKey(poolId)) return;
        obj.SetActive(false);
        pools[poolId].Enqueue(obj);
        
        IPoolable poolable = obj.GetComponent<IPoolable>();
        poolable?.OnDespawn();
    }
    
    public static bool HasAvailable(string poolId)
    {
        return pools.ContainsKey(poolId) && pools[poolId].Count > 0;
    }

}