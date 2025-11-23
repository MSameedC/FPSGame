using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifetime = 10f;

    private float knockbackForce;
    private float speed;
    private int damage;
    private Vector3 direction;

    private float timer;

    public void Initialize(int bulletDamage, float bulletSpeed, float bulletKnockback, Vector3 moveDirection)
    {
        damage = bulletDamage;
        speed = bulletSpeed;
        knockbackForce = bulletKnockback;
        direction = moveDirection;
        timer = lifetime;
    }

    private void Update()
    {
        float delta = Time.deltaTime;
        if (!gameObject.activeSelf) return;

        transform.position += direction * (speed * delta);

        timer -= delta;
        if (timer <= 0)
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.GetMask("Enemy")) return;

        var damageable = other.transform.GetComponent<IDamageable>();
        var knockbackable = other.transform.GetComponent<IKnockback>();

        damageable?.TakeDamage(damage);

        var kbDirection = EntityHelper.GetKnockbackDirection(transform.position, other.transform.position, 0);
        knockbackable?.ApplyKnockback(kbDirection, knockbackForce);

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