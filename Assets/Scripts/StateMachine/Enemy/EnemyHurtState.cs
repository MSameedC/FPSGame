public class EnemyHurtState : BaseState
{
    public EnemyHurtState(EnemyBase enemy) : base(enemy) { }

    private EnemyBase enemy => (EnemyBase)entity;

    public override void Enter()
    {
        enemy.InvokeHurtEvent();
    }

    public override void Update(float delta)
    {
        if (enemy.CurrentHealth <= 0)
        {
            enemy.SetState(new EnemyDeadState(enemy));
            return;
        }
        
        if (enemy.useGravity && !enemy.IsGrounded) return;
        enemy.SetState(new EnemyChaseState(enemy));
    }
}
