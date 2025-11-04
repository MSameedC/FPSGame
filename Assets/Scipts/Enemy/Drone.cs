using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class Drone : EnemyBase
{
    [SerializeField] private float hoverHeight = 3f;
    [SerializeField] private float heightSmoothTime = 0.5f;
    [Space]
    [SerializeField] private VisualEffect spark;
    private CharacterController cc;

    private float bufferTimer = 0;
    private float patrolTimer = 0;
    private float waitTimer = 0;

    private Vector3 velocity;
    private Vector3 randomDir;
    private Vector3 horizontalVelocity;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    #region Movement
    
    public override void Move(float delta, Vector3 direction)
    {
        base.Move(delta, direction);

        if (!cc) return;
        
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
        
        // Horizontal Movement
        Vector3 targetvelocity = new Vector3(direction.x, 0, direction.z) * Data.moveSpeed;
        horizontalVelocity = new Vector3(targetvelocity.x, 0, targetvelocity.z);
        velocity = Vector3.Lerp(velocity, targetvelocity, delta * moveSmoothness);

        cc.Move(velocity * delta);
    }
    
    private float GetGroundHeight()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 50f))
            return hit.point.y;
        return transform.position.y - hoverHeight; // Fallback
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
            Move(delta, randomDir);
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

    public override void Stop()
    {
        // Smoothly stop horizontal movement
        horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.deltaTime * moveSmoothness);
    }
    
    #endregion
    
    #region Attack
    
    public override void Attack()
    {
        // Fire at player
        Fire();
    }

    private void Fire()
    {
        
    }

    public override void UpdateAttackPosition(float delta)
    {
        LookAt(delta, GetDirectionTowardsPlayer());
    }
    
    #endregion

    #region Death

    protected override IEnumerator WaitForDeathCompletion()
    {
        return null;
    }

    protected override void OnDeathEnter()
    {
        spark.Play();
    }

    #endregion
}