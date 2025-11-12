using System.Collections;
using UnityEngine;

public class Turret : EnemyBase
{
    private float bufferTimer = 0;
    private float patrolTimer = 0;
    private float waitTimer = 0;

    private bool attackAnimationFinished = true;

    private Vector3 velocity;
    private Vector3 randomDir;
    
    private Vector3 horizontalVelocity;
    private CharacterController cc;

    protected override Vector3 moveVelocity { get; set; }

    // ---

    #region Unity

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    #endregion

    #region Behaviours
    
    #region Attack

    public override void Attack()
    {
        
    }

    public override bool IsAttackComplete()
    {
        return attackAnimationFinished;
    }

    public override void ResetAttackData()
    {
        base.ResetAttackData();
        attackAnimationFinished = false;
    }

    #endregion

    #region Movement

    public override void MoveTo(Vector3 direction, float delta)
    {
        base.MoveTo(direction, delta);

        if (!cc) return;

        // Handle horizontal movement with smoothing
        Vector3 targetHorizontalVelocity = direction * Data.moveSpeed;
        horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetHorizontalVelocity, delta * moveSmoothness);

        // Combine horizontal and vertical movement
        Vector3 finalMovement = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
        
        moveVelocity = finalMovement;
    }
    
    public override void Patrol(float delta)
    {
        // Handle direction change
        if (bufferTimer <= 0)
        {
            randomDir = GetRandomDirection();
            bufferTimer = Random.Range(2f, 6f);
        }
        else
        {
            bufferTimer -= delta;
        }

        // Patrol state machine
        if (patrolTimer > 0)
        {
            patrolTimer -= delta;
            MoveTo(randomDir, delta);
            LookAt(delta, randomDir);

            if (!(patrolTimer <= 0)) return;
            waitTimer = Random.Range(2f, 6f);
            Stop();
        }
        else if (waitTimer > 0)
        {
            waitTimer -= delta;
            Stop();

            if (waitTimer <= 0)
            {
                patrolTimer = Random.Range(2f, 6f);
            }
        }
        else
        {
            patrolTimer = Random.Range(2f, 6f);
        }
    }

    #endregion
    
    #region Death

    protected override IEnumerator WaitForDeathCompletion()
    {
        yield return null;
    }

    public override void OnDeathEnter()
    {
        
    }

    #endregion

    #endregion
}