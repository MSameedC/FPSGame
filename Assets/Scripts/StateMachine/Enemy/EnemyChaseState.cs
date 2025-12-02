using UnityEngine;
public class EnemyChaseState : BaseState
{
    private EnemyBase enemy => (EnemyBase)entity;

    private float lostSightTimer;
    private float strafeTimer;
    private float strafeDirection;
    private bool chaseCompleted;

    public EnemyChaseState(EnemyBase enemy) : base(enemy) { }

    public override void Enter()
    {
        lostSightTimer = 5f;
        strafeTimer = Random.Range(1f, 3f);
        strafeDirection = Random.value < 0.5f ? -1 : 1;
        chaseCompleted = false;

        enemy.InvokeOnPlayerSpotted();
    }

    public override void Update(float delta)
    {
        if (!enemy.PlayerSpotted())
        {
            lostSightTimer -= delta;

            if (lostSightTimer > 0 && !chaseCompleted)
            {
                Vector3 dir = (enemy.lastKnownPlayerPosition - enemy.transform.position).normalized;
                enemy.SetDesiredMove(dir);
                chaseCompleted = true;
            }
            else
            {
                enemy.InvokeOnPlayerLost();
                enemy.SetState(new EnemyPatrolState(enemy));
            }

            return;
        }

        chaseCompleted = false;

        Vector3 toPlayer = enemy.GetPredictedDirectionToPlayer();

        // --- PRIORITY 1: ATTACK ---
        if (enemy.HasClearLineOfSight() && enemy.CanAttack())
        {
            enemy.SetState(new EnemyAttackState(enemy));
            return;
        }

        // --- PRIORITY 2: DISTANCE MANAGEMENT ---
        enemy.SetDesiredLook(toPlayer);
        
        if (enemy.IsTooClose())
        {
            enemy.SetDesiredMove(enemy.GetDirectionAwayFromPlayer());
            return;
        }
        if (enemy.IsTooFar())
        {
            enemy.SetDesiredMove(toPlayer);
            return;
        }

        // --- PRIORITY 3: STRAFING ---
        strafeTimer -= delta;
        if (strafeTimer <= 0)
        {
            strafeDirection *= -1;
            strafeTimer = Random.Range(1f, 3f);
        }

        Vector3 strafe = Vector3.Cross(toPlayer, Vector3.up).normalized * strafeDirection;
        Vector3 combined = (toPlayer + strafe * 0.3f).normalized;

        enemy.SetDesiredMove(combined);

        lostSightTimer = 5f;
    }

    public override void LateUpdate(float delta)
    {
        // Movement is fully handled in EnemyBase LateUpdate
    }
}
