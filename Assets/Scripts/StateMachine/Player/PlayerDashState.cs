using UnityEngine;

public class PlayerDashState : PlayerState
{
    public PlayerDashState(PlayerController player) : base(player) { }

    private PlayerController player => (PlayerController)entity;
    private float dashTimer;
    private float impactTimer;

    public override void Enter()
    {
        Vector3 dir = player.CaptureDirection();

        player.ApplyDashImpulse(dir);
        dashTimer = player.DashDuration;
        impactTimer = 0.2f;
    }

    public override void Update(float delta)
    {
        dashTimer -= delta;
        impactTimer -= delta;
        
        if (impactTimer >= 0f)
        {
            player.ApplyDashImpact();
        }

        if (dashTimer <= 0f || player.DashHit)
        {
            if (player.IsGrounded)
                player.SetState(new PlayerGroundedState(player));
            else
                player.SetState(new PlayerAirborneState(player));
        }
    }

    public override void Exit()
    {
        player.ResetDash();
    }
}