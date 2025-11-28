using System.Collections;
using UnityEngine;

public class Turret : EnemyBase
{
    private Vector3 velocity;
    private Vector3 randomDir;
    
    private Vector3 horizontalVelocity;

    protected override Vector3 moveVelocity { get; set; }

    // ---

    #region Behaviours
    
    #region Attack

    public override void Attack()
    {
        
    }

    #endregion

    #region Movement

    public override void MoveTo(Vector3 direction, float delta)
    {
        base.MoveTo(direction, delta);

        if (!cc) return;

        // Handle horizontal movement with smoothing
        Vector3 targetHorizontalVelocity = direction * enemyData.moveSpeed;
        horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetHorizontalVelocity, delta * moveSmoothness);

        // Combine horizontal and vertical movement
        Vector3 finalMovement = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
        
        moveVelocity = finalMovement;
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