using UnityEngine;

public static class WeaponAnimHash
{
    public static readonly int Idle = Animator.StringToHash("IdleBlend");
    public static readonly int Shoot = Animator.StringToHash("ShootBlend");
    public static readonly int Reload = Animator.StringToHash("Reload");
    public static readonly int Equip = Animator.StringToHash("Equip");
    public static readonly int Unequip = Animator.StringToHash("Unequip");
    public static readonly int Sprint = Animator.StringToHash("SprintBlend");
}