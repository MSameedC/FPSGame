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

    private void LateUpdate()
    {
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
        // Hovering
        // Get ground height
        float groundHeight = GetGroundHeight();
        float targetHeight = groundHeight + hoverHeight;
        // Smooth to target height
        float newY = Mathf.SmoothDamp(transform.position.y, targetHeight, ref velocity.y, heightSmoothTime);
        // Move directly
        Vector3 newPosition = transform.position;
        newPosition.y = newY;
        transform.position = newPosition;
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
        // Fire at player
        enemyManager.SpawnBullet(muzzle.position, muzzle.forward, Data.damage, 40, 5);
        // Spawn only once and if looking at player not beforehand
    }

    public override void UpdateAttackPosition(float delta)
    {
        LookAt(delta, GetDirectionTowardsPlayer());
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