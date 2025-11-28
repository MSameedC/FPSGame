using UnityEngine;

public class Pistol : WeaponBase
{
    public override void PerformShoot()
    {
        // RayCast Bullets
        Vector3 origin = CamTransform.position;
        Vector3 direction = CamTransform.forward;
        Vector3 hitPoint = origin + direction * weaponData.range;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, weaponData.range))
        {
            hitPoint = hit.point;
            
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            damageable?.TakeDamage(weaponData.damage);

            if (damageable == null)
            {
                audioManager.PlaySFX(audioManager.GetRandomClip(audioManager.Library.wallHitSound), hitPoint, Quaternion.identity, 1, 1, 1.2f);
            }
        }
        
        vfxManager.ShowTrail(muzzlePoint.position, hitPoint);
    }
}