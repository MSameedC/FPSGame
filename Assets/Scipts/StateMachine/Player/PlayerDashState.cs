using UnityEngine;

public class PlayerDashState : PlayerState
{
    public PlayerDashState(PlayerController player) : base(player) { }

    private PlayerController player => (PlayerController)entity;
    private float dashTimer;

    public override void Enter()
    {
        Vector3 dir = player.CaptureDirection();

        player.ApplyDashImpulse(dir);
        dashTimer = player.DashDuration;
    }

    public override void Update(float delta)
    {
        dashTimer -= delta;
        if (!(dashTimer <= 0f)) return;
        if (player.IsGrounded)
            player.SetState(new PlayerGroundedState(player));
        else
            player.SetState(new PlayerAirborneState(player));

    }

    public override void Exit()
    {
        player.ResetDash();
    }
}