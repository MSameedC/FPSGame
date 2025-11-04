using UnityEngine;

public class ShootState : WeaponState
{
    public ShootState(WeaponController weapon) : base(weapon) { }

    private bool queuedNextShot;

    public override void Enter()
    {
        if (!weapon.CanShoot())
        {
            weapon.SetState(new IdleState(weapon));
            return;
        }

        OnShoot(); // fire instantly when entering
    }

    public override void Update(float delta)
    {
        if (weapon.FireTimer > 0f) return;

        if (weapon.isAutomatic && weapon.IsShooting && weapon.CanShoot())
        {
            OnShoot();
            return;
        }

        if (!weapon.isAutomatic && queuedNextShot && weapon.CanShoot())
        {
            OnShoot();
            queuedNextShot = false;
            return;
        }

        // Add safety check to prevent immediate re-entry
        if (!weapon.IsShooting || weapon.OverHeating || !weapon.CanShoot())
        {
            weapon.SetState(new IdleState(weapon));
        }
    }


    public override void OnShootPressed()
    {
        if (!weapon.CanShoot()) return;
        queuedNextShot = true;
        weapon.ResetShooting();
    }

    public override void OnShootReleased()
    {
        queuedNextShot = false;
        weapon.ResetShooting();
    }
    
    private void OnShoot()
    {
        weapon.OnShoot();
        queuedNextShot = false;
    }

    public override void Exit()
    {
        queuedNextShot = false;
        weapon.ResetShooting();
    }
}

