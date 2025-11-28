using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TrailRenderer trailRenderer;
    
    private int damage;
    private float speed;
    private float kbForce;
    private float kbRadius;
    private float damageRadius;
    private Vector3 direction;
    private LayerMask detectLayerMask;
    
    private readonly Collider[] kbCollider = new Collider[10];
    private readonly Collider[] damageCollider = new Collider[10];

    private float timer;
    
    private Rigidbody rb;

    private void Awake()
    {
        rb =  GetComponent<Rigidbody>();
    }

    public void Initialize(BulletData bulletData, Vector3 moveDirection)
    {
        speed = bulletData.speed;
        direction = moveDirection;
        damage = bulletData.damage;
        kbForce = bulletData.kbForce;
        kbRadius = bulletData.kbRadius;
        damageRadius = bulletData.damageRadius;
        detectLayerMask = bulletData.hitMask;
        meshRenderer.material = bulletData.material;
        trailRenderer.material = bulletData.material;
        timer = lifetime;
    }

    private void Update()
    {
        float delta = Time.deltaTime;
        if (!gameObject.activeSelf) return;

        if (rb)
        {
            rb.MovePosition(transform.position + direction * (speed * delta));
        }
        else
        {
            transform.position += direction * (speed * delta);
        }

        timer -= delta;
        if (timer <= 0)
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        
        bool hitSomething = false;
        
        int kbEntities = EntityHelper.EntityInRange(transform.position, kbRadius, kbCollider, detectLayerMask);
        if (kbEntities > 0)
        {
            for (int i = 0; i < kbEntities; i++)
            {
                Collider entity = kbCollider[i];
                
                Vector3 kbDirection = EntityHelper.GetKnockbackDirection(transform.position, entity.transform.position, 0.05f);
                entity.GetComponent<IKnockback>()?.ApplyKnockback(kbDirection, kbForce);
                
                hitSomething = true;
            }
        }
        
        int damageableEntities = EntityHelper.EntityInRange(transform.position, damageRadius, damageCollider, detectLayerMask);
        if (damageableEntities > 0)
        {
            for (int i = 0; i < damageableEntities; i++)
            {
                Collider entity = damageCollider[i];
                entity.GetComponent<IDamageable>()?.TakeDamage(damage);
                hitSomething = true;
            }
        }

        if (hitSomething)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.Library.explosionSound, transform.position, Quaternion.identity);
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        PoolSystem.Return(PoolName.BulletPool, gameObject);
    }

    public void OnSpawn() { }

    public void OnDespawn()
    {
        // Clean up
        timer = lifetime;
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir;
    }
}