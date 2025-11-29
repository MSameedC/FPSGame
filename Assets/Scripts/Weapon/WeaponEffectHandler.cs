using UnityEngine;

public class WeaponEffectHandler : MonoBehaviour
{
    [SerializeField] private WeaponBase weapon;
    [Space]
    [SerializeField] private Transform muzzlePoint;

    private void Start()
    {
        weapon.OnWeaponShoot += () => VFXManager.Instance.PlayMuzzleFlash(muzzlePoint.position, muzzlePoint.rotation);
    }
}
