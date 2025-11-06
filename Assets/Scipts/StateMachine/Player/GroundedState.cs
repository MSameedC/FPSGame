using UnityEngine;

public class GroundedState : PlayerState
{
    public GroundedState(PlayerController player) : base(player) { }
    
    private PlayerController player => (PlayerController)entity;

    public override void Update(float delta)
    {
        // Check if we left the ground
        if (!player.IsGrounded)
        {
            player.SetState(new AirborneState(player));
            return;
        }

        // Check for jump in Update (not LateUpdate)
        if (player.CanJump())
        {
            player.PerformJump();
            player.SetState(new JumpState(player));
            return;
        }
    }

    public override void OnDashInput()
    {
        if (player.CanDash())
        {
            Vector3 dashDir = player.CaptureDirection();
            player.ApplyDashImpulse(dashDir);
            player.SetState(new DashState(player));
        }
    }
}