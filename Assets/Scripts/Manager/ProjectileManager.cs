using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance;
    
    [SerializeField] private GameObject bulletPrefab;
    private const string bulletPoolId = PoolName.BulletPool;
    
    // ---
    
    void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void Start()
    {
        PoolSystem.CreatePool(bulletPoolId, bulletPrefab, 10);
    }
    
    public void SpawnBullet(Vector3 position, Vector3 direction, BulletData bulletData)
    {
        if (!PoolSystem.HasAvailable(bulletPoolId))
        {
            PoolSystem.Add(bulletPoolId, bulletPrefab, 5);
        }
        
        GameObject bulletObj = PoolSystem.Get(bulletPoolId, position, Quaternion.LookRotation(direction));
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.Initialize(bulletData, direction);
    }
}
