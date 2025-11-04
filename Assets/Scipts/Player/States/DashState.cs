using UnityEngine;

public class DashState : PlayerState
{
    public DashState(PlayerController player) : base(player) { }

    private float dashTimer;

    public override void Enter()
    {
        player.IsDashing = true;

        Vector3 dir = player.CaptureDirection();

        player.ApplyDashImpulse(dir);
        dashTimer = player.DashDuration;
    }

    public override void Update(float delta)
    {
        dashTimer -= delta;
        if (dashTimer <= 0f)
        {
            if (player.IsGrounded)
                player.SetState(new GroundedState(player));
            else
                player.SetState(new AirborneState(player));
        }

    }

    public override void Exit()
    {
        player.IsDashing = false;
    }
}