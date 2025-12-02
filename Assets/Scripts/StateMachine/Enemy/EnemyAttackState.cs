using UnityEngine;

public class EnemyAttackState : BaseState
{
    private EnemyBase enemy => (EnemyBase)entity;

    public EnemyAttackState(EnemyBase enemy) : base(enemy) {}

    public override void Enter()
    {
        enemy.Attack();
        enemy.InvokeAttackEvent();
    }

    public override void Update(float delta)
    {
        if (!enemy.IsAttackComplete())
            return;

        // Orient after attack finishes
        Vector3 toPlayer = enemy.GetPredictedDirectionToPlayer();
        enemy.SetDesiredLook(toPlayer);

        // Go back to chase
        enemy.SetState(new EnemyChaseState(enemy));
    }

    public override void Exit()
    {
        enemy.ResetAttackData();
    }
}