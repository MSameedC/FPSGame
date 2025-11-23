public class CooldownState : WeaponState
{
    public CooldownState(WeaponController weapon) : base(weapon) { }

    private WeaponData data => weapon.GetWeaponData();
    private UpgradeableWeaponData upgradedData => weapon.GetUpgradedData();

    private float cooldownDelay;

    public override void Enter()
    {
        cooldownDelay = data.postOverheatDelay;
        weapon.OnCoolDownEnter();
    }

    public override void Update(float delta)
    {
        if (cooldownDelay > 0)
        {
            cooldownDelay -= delta;
            return; // Wait for delay to complete
        }
        
        weapon.OnCoolDownExit();

        // Only transition when cooled down sufficiently
        if (weapon.CurrentHeat <= upgradedData.MaxHeat * 0.3f) // 30% threshold
        {
            weapon.SetState(new IdleState(weapon));
        }
    }

    public override void OnReloadPressed()
    {
        weapon.OnCoolDownExit();
    }
}