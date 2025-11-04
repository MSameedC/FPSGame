using UnityEngine;

public class SlamState : PlayerState
{
    public SlamState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        player.ApplySlamStart();
    }
    public override void Update(float delta)
    {
        if (player.IsGrounded)
        {
            player.ApplySlamImpact();          // damage/knockback to enemies
            player.SetState(new GroundedState(player));
        }
    }
}