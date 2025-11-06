using UnityEngine;

public class AirborneState : PlayerState
{
    public AirborneState(PlayerController player) : base(player) { }
    
    private PlayerController player => (PlayerController)entity;

    public override void Update(float delta)  // Use Update instead of LateUpdate
    {
        if (player.IsGrounded)
        {
            player.SetState(new GroundedState(player));
            return;
        }

        // Handle wall jumps in Update instead of waiting for input
        // Coyotime has levarage before grounded so if we are still
        // Jumping, we would jump
        if (player.CanWallJump())
        {
            player.PerformWallJump();
            player.SetState(new JumpState(player));
        }
    }

    public override void OnDashInput()
    {
        if (player.CanDash())
            player.SetState(new DashState(player));
    }
    
    public override void OnSlamInput()
    {
        if (player.CanSlam())
            player.SetState(new SlamState(player));
    }

    public override void Exit()
    {
        player.OnLand();
    }
}