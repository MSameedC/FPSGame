using UnityEngine;

public abstract class WeaponState : SystemState
{
    protected readonly WeaponBase weapon;
    protected WeaponState(WeaponBase weapon)
    {
        this.weapon = weapon;
    }
    public virtual void OnReloadPressed() { }
}
