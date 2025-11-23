using UnityEngine;

public class EnemyChaseState : BaseState
{
    public EnemyChaseState(EnemyBase enemy) : base(enemy) { }

    private EnemyBase enemy => (EnemyBase)entity;

    private float lostSightTimer;
    
    // ---
    public override void Enter()
    {
        lostSightTimer = 5;
        enemy.InvokeOnPlayerSpotted();
    }
    
    public override void LateUpdate(float delta)
    {
        if (!enemy.PlayerSpotted())
        {
            lostSightTimer -= delta;
            
            if (lostSightTimer > 0)
            {
                // Chase last known position for 2 seconds
                Vector3 lastKnownDir = (enemy.lastKnownPlayerPosition - enemy.transform.position).normalized;
                enemy.Chase(lastKnownDir, delta);
            }
            else
            {
                // Give up and patrol
                enemy.InvokeOnPlayerLost();
                enemy.SetState(new EnemyPatrolState(enemy));
            }
        }
        else
        {
            lostSightTimer = 5;
            
            if (enemy.IsTooClose())
                enemy.Retreat(delta);
            else if (enemy.IsTooFar())
                enemy.Chase(enemy.GetPredictedDirectionToPlayer(), delta);
        
            if (enemy.IsInAttackRange() && enemy.CanAttack())
                enemy.SetState(new EnemyAttackState(enemy));
        }
    }
    
    public override void Exit()
    {
        enemy.Stop();
    }
}
