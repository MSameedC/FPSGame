using UnityEngine;

public abstract class WeaponState
{
    protected WeaponController weapon;

    protected WeaponState(WeaponController weapon)
    {
        this.weapon = weapon;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }

    public virtual void OnShootPressed() { }
    public virtual void OnShootReleased() { }
    public virtual void Update(float delta) { }
}
