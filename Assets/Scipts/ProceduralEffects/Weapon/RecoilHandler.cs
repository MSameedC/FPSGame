using UnityEngine;

public class RecoilHandler : MonoBehaviour, IProceduralEffect
{
    [SerializeField] private float zLimit = 0.01f;
    [SerializeField] private float recoilIntensity = 6f;
    [SerializeField] private float maxRecoilAngle = 5f;

    private Vector3 originalPos;
    private Quaternion originalRot;

    private Vector3 currentPosOffset;
    private Quaternion currentRotOffset = Quaternion.identity;

    private Vector3 targetPos;
    private Quaternion targetRot = Quaternion.identity;

    private float returnSpeed;
    private float snappiness;

    private float zRotVelocity;
    private float zRotCurrent;

    private float zRotSpringStrength;
    private float zRotDamping;

    private float rangeX;
    private float rangeY;
    private float rangeZ;

    private float aimRecoilIntensity;

    public void Initialize(ProceduralRuntimeContext ctx)
    {
        SetConfig(ctx.weaponData);

        originalPos = transform.localPosition;
        originalRot = transform.localRotation;
    }

    public void Apply(ProceduralRuntimeContext ctx)
    {

        if (ctx.isShooting) ApplyRecoil(ctx.isAiming);

        float delta = ctx.deltaTime;

        float lerpPos = 1f - Mathf.Exp(-returnSpeed * delta);
        float lerpRot = 1f - Mathf.Exp(-returnSpeed * delta);
        float lerpSnapPos = 1f - Mathf.Exp(-snappiness * delta);
        float lerpSnapRot = 1f - Mathf.Exp(-snappiness * delta);

        targetPos = Vector3.Lerp(targetPos, Vector3.zero, lerpSnapPos);
        currentPosOffset = Vector3.Lerp(currentPosOffset, targetPos, lerpPos);

        targetRot = Quaternion.Slerp(targetRot, Quaternion.identity, lerpSnapRot);
        currentRotOffset = Quaternion.Slerp(currentRotOffset, targetRot, lerpRot);

        float zSpringForce = -zRotSpringStrength * zRotCurrent - zRotDamping * zRotVelocity;
        zRotVelocity += zSpringForce * delta;
        zRotCurrent += zRotVelocity * delta;

        Quaternion zRot = Quaternion.Euler(0f, 0f, zRotCurrent);

        transform.localPosition = originalPos + currentPosOffset;
        transform.localRotation = originalRot * currentRotOffset * zRot;
    }

    public void ApplyRecoil(bool aiming)
    {
        float intensity = aiming ? aimRecoilIntensity : recoilIntensity;
        float zClamp = aiming ? zLimit * 0.5f : zLimit;

        float x = rangeX * intensity;
        float y = rangeY * intensity;
        float z = rangeZ * intensity;

        // Backwards kick motion range
        targetPos.z = Mathf.Clamp(targetPos.z - z, -zClamp, 0f);

        // Rotational motion range
        Quaternion newRot = targetRot * Quaternion.Euler(-x, Random.Range(-y, y), 0f);
        float currentRot = Quaternion.Angle(Quaternion.identity, newRot);

        if (currentRot <= maxRecoilAngle)
            targetRot = newRot;
        else
            targetRot = Quaternion.RotateTowards(Quaternion.identity, newRot, maxRecoilAngle);

        // Final z-Rotation
        zRotCurrent += -z * intensity;
    }

    public void SetConfig(WeaponData data)
    {
        // initialize data
        rangeX = data.recoil.recoilX / 10;
        rangeY = data.recoil.recoilY / 10;
        rangeZ = data.recoil.recoilZ / 10;

        zRotDamping = data.weapon.swayDamping;
        zRotSpringStrength = data.weapon.swaySpringStrength;

        snappiness = data.weapon.recoilSnappiness;
        returnSpeed = data.weapon.recoilReturnSpeed;

        aimRecoilIntensity = data.aim.aimRecoil;
    }

    public void ResetEffect()
    {
        transform.localPosition = originalPos;
        transform.localRotation = originalRot;
    }
}
