public class EnemyDeadState : BaseState
{
    public EnemyDeadState(EnemyBase enemy) : base(enemy) { }
    
    private EnemyBase enemy => (EnemyBase)entity;

    public override void Enter()
    {
        enemy.OnDeathEnter();
        enemy.InvokeDeathEvent();
        enemy.StartCoroutine(enemy.ReturnToPoolAfterDeath());
    }
}
