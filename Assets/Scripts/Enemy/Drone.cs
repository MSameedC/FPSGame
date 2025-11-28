using System.Collections;
using UnityEngine;

public class Drone : EnemyBase
{
    [Header("Hover")]
    [SerializeField] private float hoverHeight = 3f;
    [SerializeField] private float heightSmoothTime = 0.5f;
    [Header("Bullet Data")]
    [SerializeField] private BulletData bulletData;
    [Space]
    [SerializeField] private Transform muzzle;

    private Vector3 velocity;

    protected override Vector3 moveVelocity { get; set; }

    // ---

    private void Awake()
    {
        bulletData.damage = enemyData.damage;
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        Hover();
    }

    #region Movement

    public override void MoveTo(Vector3 direction, float delta)
    {
        base.MoveTo(direction, delta);

        // Horizontal Movement
        Vector3 targetVelocity = new Vector3(direction.x, 0, direction.z) * enemyData.moveSpeed;
        Vector3 horizontalVelocity = new Vector3(targetVelocity.x, 0, targetVelocity.z);
        moveVelocity = Vector3.Lerp(moveVelocity, horizontalVelocity, delta * moveSmoothness);
    }

    private void Hover()
    {
        // Get ground height
        float groundHeight = GetGroundHeight();
        float targetHeight = groundHeight + hoverHeight;
        // Smooth to target height
        float newY = Mathf.SmoothDamp(transform.position.y, targetHeight, ref velocity.y, heightSmoothTime);
        // Use CharacterController.Move for the vertical movement to avoid conflicts
        float verticalMovement = newY - transform.position.y;
        cc.Move(Vector3.up * verticalMovement);
    }

    private float GetGroundHeight()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 50f))
            return hit.point.y;
        return transform.position.y - hoverHeight; // Fallback
    }

    #endregion

    #region Attack

    public override void Attack()
    {
        Vector3 direction = GetPredictedDirectionToPlayer();
        projectileManager.SpawnBullet(muzzle.position, direction, bulletData);
    }

    #endregion

    #region Death

    protected override IEnumerator WaitForDeathCompletion()
    {
        // Play explosion effects
        yield return null;
    }

    public override void OnDeathEnter()
    {
        
    }

    #endregion
}