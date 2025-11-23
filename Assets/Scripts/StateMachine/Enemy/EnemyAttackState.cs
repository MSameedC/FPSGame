public class EnemyAttackState : BaseState
{
    public EnemyAttackState(EnemyBase enemy) : base(enemy) { }
    
    private EnemyBase enemy => (EnemyBase)entity;
    
    public override void Enter()
    {
        enemy.ResetAttackData();
        enemy.Attack(); // ⭐ CALL ATTACK HERE, not in Update!
        enemy.InvokeAttackEvent();
    }
    
    public override void Update(float delta)
    {
        // Wait for attack to complete
        if (!enemy.IsAttackComplete()) return;
        
        // Update movement i.e. rotation after attack
        enemy.LookAt(enemy.GetPredictedDirectionToPlayer(), delta);
        
        // Attack finished - check what to do next
        if (!enemy.IsInAttackRange() || enemy.CanAttack())
        {
            enemy.SetState(new EnemyChaseState(enemy));
        }

    }
}