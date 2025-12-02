using UnityEngine;

public class WeaponSway : MonoBehaviour, IProceduralEffect
{
    [Header("Base Settings")]
    [SerializeField] private float maxAngle = 2.5f;
    [SerializeField] private float returnSpeed = 10f;

    private float springStrength;
    private float damping;

    private float aimSwayRate;
    private float aimReturnSpeed;

    private Vector3 rotOffset;
    private Vector3 rotVelocity;
    private Quaternion initialRot;

    public void Initialize(ProceduralRuntimeContext ctx)
    {
        initialRot = transform.localRotation;
        SetConfig(ctx.weaponData);
    }

    public void Apply(ProceduralRuntimeContext ctx)
    {
        float delta = ctx.deltaTime;
        bool isAiming = ctx.isAiming;

        float strength = isAiming ? springStrength * aimSwayRate : springStrength;
        float dampingFactor = isAiming ? damping * aimSwayRate : damping;
        float angleClamp = isAiming ? maxAngle * aimSwayRate : maxAngle;
        float inputScale = (isAiming ? aimSwayRate : 1) * 1.5f;

        Vector2 adjustedMouse = ctx.lookInput * inputScale;
        Vector3 targetRot = new Vector3(-adjustedMouse.y, adjustedMouse.x, 0f);

        Vector3 displacement = rotOffset - targetRot;
        Vector3 springForce = -strength * displacement - dampingFactor * rotVelocity;

        rotVelocity += springForce * delta;
        rotOffset += rotVelocity * delta;

        if (rotOffset.magnitude > angleClamp)
        {
            rotOffset = rotOffset.normalized * angleClamp;
            rotVelocity = Vector3.zero; // prevents jitter at clamp
        }


        float interpSpeed = isAiming ? aimReturnSpeed : returnSpeed;
        float lerpFactor = 1f - Mathf.Exp(-interpSpeed * delta);

        Quaternion swayRotation = Quaternion.Euler(rotOffset);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRot * swayRotation, lerpFactor);
    }

    public void ResetEffect()
    {
        rotVelocity = Vector3.zero;
        rotOffset = Vector3.zero;
        transform.localRotation = initialRot;
    }

    private void SetConfig(WeaponData data)
    {
        damping = data.weapon.swayDamping;
        springStrength = data.weapon.swaySpringStrength;
        aimSwayRate = data.aim.aimSway;
        aimReturnSpeed = data.aim.aimReturnSpeed;
    }
}