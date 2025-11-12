public class IdleState : WeaponState
{
    public IdleState(WeaponController weapon) : base(weapon) { }

    public override void Update(float delta)
    {
        // Check overheating first (cheaper check)
        if (weapon.OverHeating)
        {
            weapon.SetState(new CooldownState(weapon));
            return;
        }

        if (weapon.CanShoot())
        {
            weapon.SetState(new ShootState(weapon));
            return;
        }

        // weapon.SetSliderLocked(true);
    }
}