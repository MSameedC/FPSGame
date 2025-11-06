public class PatrolState : BaseState
{
    public PatrolState(EnemyBase enemy) : base(enemy) { }

    private EnemyBase enemy => (EnemyBase)entity;
    
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
