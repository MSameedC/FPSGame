public class EnemyHurtState : BaseState
{
    public EnemyHurtState(EnemyBase enemy) : base(enemy) { }

    private EnemyBase enemy => (EnemyBase)entity;
    private float hurtTimer = 0.1f;
    
    // ---

    public override void Enter()
    {
        enemy.InvokeHurtEvent();
    }

    public override void Update(float delta)
    {
        hurtTimer -= delta;
        
        if (enemy.CurrentHealth <= 0)
        {
            enemy.SetState(new EnemyDeadState(enemy));
            return;
        }
        
        if (enemy.useGravity && !enemy.IsGrounded) return;
        if (!(hurtTimer <= 0)) return;
        enemy.SetState(new EnemyChaseState(enemy));
    }
}
