public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerController player) : base(player) { }
    
    private PlayerController player => (PlayerController)entity;
    private float jumpStateTimer = 0.1f; // Brief state for jump animation

    public override void Update(float delta)
    {
        jumpStateTimer -= delta;

        if (jumpStateTimer <= 0f || player.IsGrounded)
        {
            player.SetState(new PlayerAirborneState(player));
        }
    }

    public override void OnSlamInput()
    {
        if (player.CanSlam())
            player.SetState(new PlayerSlamState(player));
    }
}