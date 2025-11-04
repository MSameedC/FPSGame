public class AttackState : EnemyState
{
    public AttackState(EnemyBase enemy) : base(enemy) { }
    
    public override void Enter()
    {
        enemy.ResetAttackData();
        enemy.Attack(); // ⭐ CALL ATTACK HERE, not in Update!
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public override void Update(float delta)
    {
        // Wait for attack to complete
        if (!enemy.IsAttackComplete()) return;
        
        // Update movement i.e rotation after attack
        enemy.UpdateAttackPosition(delta);
        
        // Attack finished - check what to do next
        if (!enemy.IsInAttackRange())
        {
            enemy.SetState(new ChaseState(enemy));
        }
        else
        {
            // Player still in range - start new attack after cooldown
            if (enemy.CanAttack())
            {
                enemy.SetState(new AttackState(enemy));
            }
            // Otherwise wait in this state until cooldown finishes
        }
    }
}