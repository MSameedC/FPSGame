using UnityEngine;

public abstract class WeaponState : SystemState
{
    protected readonly WeaponController weapon;
    protected WeaponState(WeaponController weapon)
    {
        this.weapon = weapon;
    }
    public virtual void OnReloadPressed() { }
}
