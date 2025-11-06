public class EnemyDeadState : BaseState
{
    public EnemyDeadState(EnemyBase enemy) : base(enemy) { }
    
    private EnemyBase enemy => (EnemyBase)entity;

    public override void Enter()
    {
        enemy.OnDeathEnter();
    }

    public override void Update(float delta)
    {
        if (enemy.IsGrounded) return;
        enemy.StartCoroutine(enemy.ReturnToPoolAfterDeath());
    }
}
