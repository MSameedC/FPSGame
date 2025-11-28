using UnityEngine;

public class EnemyChaseState : BaseState
{
    public EnemyChaseState(EnemyBase enemy) : base(enemy) { }

    private EnemyBase enemy => (EnemyBase)entity;

    private float lostSightTimer;
    private float strafeTimer;
    private float strafeDirection;
    
    // ---
    
    public override void Enter()
    {
        lostSightTimer = 5;
        enemy.InvokeOnPlayerSpotted();
        strafeTimer = Random.Range(1f, 3f);
        strafeDirection = Random.Range(0, 2) * 2 - 1; // -1 or 1
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
            Vector3 toPlayer = enemy.GetPredictedDirectionToPlayer();
            
            if (enemy.IsTooClose())
                enemy.Retreat(delta);
            else if (enemy.IsTooFar())
                enemy.Chase(toPlayer, delta);

            if (enemy.HasClearLineOfSight())
            {
                if (enemy.IsInAttackRange() && enemy.CanAttack() )
                {
                    enemy.SetState(new EnemyAttackState(enemy));
                }
            }
            else
            {
                strafeTimer = 0;    // Immediate Reset
                strafeDirection = Random.Range(0, 2) * 2 - 1; // Generate Direction
            }
            
            // Update Strafe Timer
            strafeTimer -= delta;
            // Update Strafe Direction
            if (strafeTimer <= 0)
            {
                strafeDirection *= -1; // Switch direction
                strafeTimer = Random.Range(1f, 3f); // Set Timer to Strafe for
            }
            // Perform Strafe
            Vector3 strafeDir = Vector3.Cross(toPlayer, Vector3.up).normalized * strafeDirection;
            Vector3 moveDirection = (toPlayer + strafeDir * 0.3f).normalized;
            enemy.Chase(moveDirection, delta);
            
            // Update Data
            lostSightTimer = 5;
        }
    }
    
    public override void Exit()
    {
        enemy.Stop();
    }
}
