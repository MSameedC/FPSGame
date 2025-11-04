public class PatrolState : EnemyState
{
    public PatrolState(EnemyBase enemy) : base(enemy) { }

    public override void Update(float delta)
    {
        if (enemy.IsInSight())
        {
            enemy.SetState(new ChaseState(enemy));
            return;
        }

        enemy.Patrol(delta);
    }
}
