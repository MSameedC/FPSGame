public class EnemyAttackState : BaseState
{
    public EnemyAttackState(EnemyBase enemy) : base(enemy) { }
    
    private EnemyBase enemy => (EnemyBase)entity;
    
    public override void Enter()
    {
        enemy.ResetAttackData();
        enemy.Attack(); // ⭐ CALL ATTACK HERE, not in Update!
    }
    
    public override void Update(float delta)
    {
        // Wait for attack to complete
        if (!enemy.IsAttackComplete()) return;
        
        // Update movement i.e rotation after attack
        enemy.UpdateAttackPosition(delta);
        
        // Attack finished - check what to do next
        if (!enemy.IsInAttackRange())
        {
            enemy.SetState(new EnemyGroundedState(enemy));
        }
        else
        {
            // Player still in range - start new attack after cooldown
            if (enemy.CanAttack())
            {
                enemy.SetState(new EnemyAttackState(enemy));
            }
            // Otherwise wait in this state until cooldown finishes
        }
    }
}