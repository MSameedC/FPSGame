using System;
using UnityEngine;

public class WeaponAnimationHandler : MonoBehaviour
{
    [SerializeField] private WeaponBase weapon;
    [SerializeField] private Animator animator;

    private void Start()
    {
        weapon.OnWeaponShoot += PlayShootAnimation;
    }

    private void OnDisable()
    {
        weapon.OnWeaponShoot -= PlayShootAnimation;
    }

    private void PlayShootAnimation()
    {
        if (InputManager.IsAiming) return;
        animator.SetTrigger(AnimationName.Shoot);
    }
}
