public class IdleState : WeaponState
{
    public IdleState(WeaponBase weapon) : base(weapon) { }

    public override void Update(float delta)
    {
        // Check overheating first (cheaper check)
        if (weapon.OverHeating)
        {
            weapon.SetState(new CooldownState(weapon));
            return;
        }

        if (InputManager.IsShooting && weapon.CanShoot())
        {
            weapon.SetState(new ShootState(weapon));
        }

        // weapon.SetSliderLocked(true);
    }
}