using UnityEngine;

public class WorldBorder : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        damageable.TakeDamage(999999);
    }
}
