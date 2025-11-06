public class EnemyGroundedState : BaseState
{
    public EnemyGroundedState(EnemyBase enemy) : base(enemy) { }

    private EnemyBase enemy => (EnemyBase)entity;
    
    public override void Update(float delta)
    {
        if (!enemy.PlayerSpotted())
        {
            enemy.Patrol(delta);
            return;
        }
        
        // if (!enemy.IsMoveComplete()) return;

        if (enemy.IsInAttackRange())
        {
            enemy.SetState(new EnemyAttackState(enemy));
            return;
        }

        if (enemy.IsTooFar())
        {
            ChasePlayer(delta);
        }
        else if (enemy.IsTooClose())
        {
            enemy.Retreat(delta);
        }
    }
    
    public override void Exit()
    {
        enemy.Stop();
    }

    private void ChasePlayer(float delta)
    {
        enemy.MoveTo(enemy.GetDirectionTowardsPlayer(), delta);
        enemy.LookAt(delta, enemy.GetDirectionTowardsPlayer());
    }
}
