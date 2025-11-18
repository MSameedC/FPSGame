public class EnemyPatrolState : BaseState
{
    public EnemyPatrolState(EnemyBase enemy) : base(enemy) { }

    private EnemyBase enemy => (EnemyBase)entity;
    
    // ---
    
    public override void LateUpdate(float delta)
    {
        if (!enemy.PlayerSpotted())
        {
            enemy.Patrol(delta);
        }
        else
        {
            enemy.SetState(new EnemyChaseState(enemy));
        }
        
        if (enemy.IsInAttackRange())
        {
            enemy.SetState(new EnemyAttackState(enemy));
        }
    }
    
    public override void Exit()
    {
        enemy.Stop();
    }
}
