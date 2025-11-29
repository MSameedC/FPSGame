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

            if (damageable != null)
            {
                VFXManager.PlayEntityHit(hitPoint);
            }
            else
            {
                AudioManager.PlaySFX(AudioManager.GetRandomClip(AudioManager.Library.wallHitSound), hitPoint, Quaternion.identity, 1, 1, 1.2f);
                VFXManager.PLayLightWallHit(hitPoint, Quaternion.LookRotation(hit.normal));
            }
        }
        
        VFXManager.ShowTrail(muzzlePoint.position, hitPoint);
    }
}