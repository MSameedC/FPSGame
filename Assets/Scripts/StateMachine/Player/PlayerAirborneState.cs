public class PlayerAirborneState : PlayerState
{
    public PlayerAirborneState(PlayerController player) : base(player) { }
    
    private PlayerController player => (PlayerController)entity;

    public override void Update(float delta)  // Use Update instead of LateUpdate
    {
        if (player.IsGrounded)
        {
            player.SetState(new PlayerGroundedState(player));
            return;
        }

        // Handle wall jumps in Update instead of waiting for input
        // Coyotime has levarage before grounded so if we are still
        // Jumping, we would jump
        if (player.CanWallJump())
        {
            player.PerformWallJump();
            player.SetState(new PlayerJumpState(player));
        }
    }

    public override void OnDashInput()
    {
        if (player.CanDash())
            player.SetState(new PlayerDashState(player));
    }
    
    public override void OnSlamInput()
    {
        if (player.CanSlam())
            player.SetState(new PlayerSlamState(player));
    }

    public override void Exit()
    {
        player.OnLand();
    }
}