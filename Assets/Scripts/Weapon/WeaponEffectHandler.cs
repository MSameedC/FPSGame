using System;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponEffectHandler : MonoBehaviour
{
    [SerializeField] private WeaponBase weapon;
    [Space]
    [SerializeField] private Transform muzzlePoint;
    [Space]
    [SerializeField] private VisualEffectAsset muzzleFlash;

    private void Start()
    {
        weapon.OnWeaponShoot += () => VFXManager.Instance.PlayVFX(muzzleFlash, muzzlePoint.position, Quaternion.identity);
    }
}
