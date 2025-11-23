using System.Collections;
using UnityEngine;

public class ShootState : WeaponState
{
    private Coroutine burstCoroutine;
    private int burstSize = 3;
    private float burstDelay = 0.1f;
    
    public ShootState(WeaponController weapon) : base(weapon) { }

    public override void Enter()
    {
        if (weapon.CurrentFireMode == FireMode.Burst)
        {
            weapon.StartCoroutine(BurstFireRoutine());
            return;
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

        if (weapon.CurrentFireMode == FireMode.Auto)
        {
            if (InputManager.IsShooting && weapon.CanShoot())
            {
                weapon.OnShoot();
                return;
            }
        }
        
        weapon.ResetShooting();
        
        // For single/burst modes: exit immediately after shooting once
        if (!InputManager.IsShooting)
            weapon.SetState(new IdleState(weapon));
    }

    public override void Exit()
    {
        weapon.ResetShooting();
    }
    
    private IEnumerator BurstFireRoutine()
    {
        int shotsFired = 0;

        while (shotsFired < burstSize && !weapon.OverHeating)
        {
            weapon.OnShoot();
            shotsFired++;
            
            yield return new WaitForSeconds(burstDelay);
        }
        
        yield return new WaitUntil(() => !InputManager.IsShooting);
        weapon.ResetShooting();
        weapon.SetState(new IdleState(weapon));
    }
}