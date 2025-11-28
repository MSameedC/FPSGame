using System.Collections;
using UnityEngine;

public class Enemy : EnemyBase
{
    [SerializeField] private Animator animator;

    private bool attackAnimationFinished = true;
    
    private Vector3 horizontalVelocity;

    protected override Vector3 moveVelocity { get; set; }

    // ---

    #region Behaviours
    
    #region Attack

    public override void Attack()
    {
        animator.CrossFade(AnimationName.Attack, 0.05f);
        StartCoroutine(WaitForAttackAnimation());
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

    private IEnumerator WaitForAttackAnimation()
    {
        yield return null;
        var state = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(state.length);
        attackAnimationFinished = true;
    }

    #endregion

    #region Movement

    public override void MoveTo(Vector3 direction, float delta)
    {
        base.MoveTo(direction, delta);

        // Handle horizontal movement with smoothing
        Vector3 targetHorizontalVelocity = direction * enemyData.moveSpeed;
        horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetHorizontalVelocity, delta * moveSmoothness);

        // Combine horizontal and vertical movement
        moveVelocity = Vector3.forward * horizontalVelocity.z + Vector3.right * horizontalVelocity.x;

        // Effects
        animator.Play(AnimationName.Run);
    }

    public override void Stop()
    {
        animator.CrossFade(AnimationName.Idle, 0.1f);
    }

    #endregion
    
    #region Death

    protected override IEnumerator WaitForDeathCompletion()
    {
        yield return null;
        var state = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(state.length);
    }

    public override void OnDeathEnter()
    {
        animator.Play(AnimationName.Die);
    }

    #endregion

    #endregion
}