public class PlayerSlamState : PlayerState
{
    public PlayerSlamState(PlayerController player) : base(player) { }
    
    private PlayerController player => (PlayerController)entity;
    
    public override void Enter()
    {
        player.ApplySlamStart();
    }
    
    public override void Update(float delta)
    {
        if (!player.IsGrounded) return;
        player.ApplySlamImpact();          // damage/knockback to enemies
        player.SetState(new PlayerGroundedState(player));
    }
}