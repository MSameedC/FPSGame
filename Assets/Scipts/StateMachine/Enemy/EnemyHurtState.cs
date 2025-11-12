public class EnemyHurtState : BaseState
{
    public EnemyHurtState(EnemyBase enemy) : base(enemy) { }

    private EnemyBase enemy => (EnemyBase)entity;

    public override void Enter()
    {
        enemy.OnHurtEnter();
    }

    public override void Update(float delta)
    {
        if (enemy.useGravity)
        {
            if (!enemy.IsGrounded) return;
            enemy.SetState(new EnemyGroundedState(enemy));
        }
        else
        {
            enemy.SetState(new EnemyGroundedState(enemy));
        }
    }
}
