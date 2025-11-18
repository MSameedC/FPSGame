public class ShootState : WeaponState
{
    public ShootState(WeaponController weapon) : base(weapon) { }

    public override void Enter()
    {
        if (weapon.CurrentFireMode == FireMode.Burst)
        {
            weapon.StartCoroutine(weapon.BurstFire());
        }
        
        weapon.OnShoot();
    }

    public override void Update(float delta)
    {
        // Check overheating first
        if (weapon.OverHeating)
        {
            weapon.SetState(new CooldownState(weapon));
            return;
        }

        // For single/burst modes: exit immediately after shooting once
        if (!InputManager.IsShooting)
        {
            weapon.SetState(new IdleState(weapon));
            return;
        }

        if (weapon.CurrentFireMode == FireMode.Auto)
        {
            switch (InputManager.IsShooting)
            {
                // Auto mode: keep shooting while holding
                case true when weapon.CanShoot():
                    weapon.OnShoot();
                    break;
                // Exit auto mode if stopped shooting
                case false:
                    weapon.SetState(new IdleState(weapon));
                    break;
            }
        }
        else
        {
            weapon.ResetShooting();
        }
    }

    public override void Exit()
    {
        weapon.ResetShooting();
    }
}