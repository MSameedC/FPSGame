public class EnemyGroundedState : BaseState
{
    public EnemyGroundedState(EnemyBase enemy) : base(enemy) { }

    private EnemyBase enemy => (EnemyBase)entity;
    
    public override void Update(float delta)
    {
        if (!enemy.PlayerSpotted())
        {
            enemy.Patrol(delta);
        }
        else
        {
            if (enemy.IsTooClose())
            {
                enemy.Retreat(delta);
            }
            else if (enemy.IsTooFar())
            {
                enemy.Chase(delta);
            }
        
            if (enemy.IsInAttackRange())
            {
                enemy.SetState(new EnemyAttackState(enemy));
            }
        }
    }
    
    public override void Exit()
    {
        enemy.Stop();
    }
}
