using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class Drone : EnemyBase
{
    [SerializeField] private float hoverHeight = 3f;
    [SerializeField] private float heightSmoothTime = 0.5f;
    [Space]
    [SerializeField] private GameObject bulletPrefab;
    [Space]
    [SerializeField] private Transform muzzle;
    [SerializeField] private VisualEffect spark;

    private Vector3 velocity;

    protected override Vector3 moveVelocity { get; set; }

    // ---

    private void Awake()
    {
        Bullet bullet = bulletPrefab.GetComponent<Bullet>();
        bullet.Initialize(Data.damage, 50, 5, Vector3.forward);
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
        Vector3 targetVelocity = new Vector3(direction.x, 0, direction.z) * Data.moveSpeed;
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
        CharacterController.Move(new Vector3(0, verticalMovement, 0));
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
        enemyManager.SpawnBullet(muzzle.position, direction, Data.damage, 40, 5);
    }

    #endregion

    #region Death

    protected override IEnumerator WaitForDeathCompletion()
    {
        yield return new WaitForSeconds(1);
    }

    public override void OnDeathEnter()
    {
        spark.Play();
        useGravity = true;
    }

    #endregion
}