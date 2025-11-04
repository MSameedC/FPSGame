public class ChaseState : EnemyState
{
    public ChaseState(EnemyBase enemy) : base(enemy) { }

    public override void Update(float delta)
    {
        // if (!enemy.IsMoveComplete()) return;

        if (enemy.IsInAttackRange())
        {
            enemy.SetState(new AttackState(enemy));
            return;
        }

        if (enemy.IsTooFar())
        {
            enemy.Move(delta, enemy.GetDirectionTowardsPlayer());
            enemy.LookAt(delta, enemy.GetDirectionTowardsPlayer());
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
}
