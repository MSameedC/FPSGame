public class JumpState : PlayerState
{
    public JumpState(PlayerController player) : base(player) { }

    private float jumpStateTimer = 0.1f; // Brief state for jump animation

    public override void Update(float delta)
    {
        jumpStateTimer -= delta;

        if (jumpStateTimer <= 0f || player.IsGrounded)
        {
            player.SetState(new AirborneState(player));
        }
    }

    public override void OnSlamInput()
    {
        if (player.CanSlam())
            player.SetState(new SlamState(player));
    }
}