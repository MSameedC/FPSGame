using UnityEngine;

public class WeaponAnimationHandler : MonoBehaviour
{
    [SerializeField] private WeaponBase weapon;
    [SerializeField] private Animator animator;

    private void Start()
    {
        weapon.OnWeaponShoot += () => animator.SetTrigger("Shoot");
    }
}
