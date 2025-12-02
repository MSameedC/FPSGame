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

    private float rangeX;
    private float rangeY;
    private float rangeZ;

    public void Initialize(ProceduralRuntimeContext ctx)
    {
        SetConfig(ctx.weaponData);

        originalPos = transform.localPosition;
        originalRot = transform.localRotation;
    }

    public void Apply(ProceduralRuntimeContext ctx)
    {
        if (ctx.isShooting) ApplyRecoil();

        float delta = ctx.deltaTime;

        float lerpPos = 1f - Mathf.Exp(-returnSpeed * delta);
        float lerpRot = 1f - Mathf.Exp(-returnSpeed * delta);
        float lerpSnapPos = 1f - Mathf.Exp(-snappiness * delta);
        float lerpSnapRot = 1f - Mathf.Exp(-snappiness * delta);

        targetPos = Vector3.Lerp(targetPos, Vector3.zero, lerpSnapPos);
        currentPosOffset = Vector3.Lerp(currentPosOffset, targetPos, lerpPos);

        targetRot = Quaternion.Slerp(targetRot, Quaternion.identity, lerpSnapRot);
        currentRotOffset = Quaternion.Slerp(currentRotOffset, targetRot, lerpRot);

        transform.localPosition = originalPos + currentPosOffset;
        transform.localRotation = originalRot * currentRotOffset;
    }

    private void ApplyRecoil()
    {
        float x = rangeX * recoilIntensity;
        float y = rangeY * recoilIntensity;
        float z = rangeZ * recoilIntensity;

        // Backwards kick motion range
        targetPos.z = Mathf.Clamp(targetPos.z - z, -zLimit, 0f);

        // Rotational motion range
        Quaternion newRot = targetRot * (Quaternion.Euler(-x, Random.Range(-y, y), 0f));
        float currentRot = Quaternion.Angle(Quaternion.identity, newRot);

        if (currentRot <= maxRecoilAngle)
            targetRot = newRot;
        else
            targetRot = Quaternion.RotateTowards(Quaternion.identity, newRot, maxRecoilAngle);
    }

    private void SetConfig(WeaponData data)
    {
        if (data == null)
        {
            Debug.LogError("WeaponData is null in RecoilHandler!");
            return;
        }
        
        // initialize data
        rangeX = data.recoil.recoilX;
        rangeY = data.recoil.recoilY;
        rangeZ = data.recoil.recoilZ;

        snappiness = data.weapon.recoilSnappiness;
        returnSpeed = data.weapon.recoilReturnSpeed;
    }

    public void ResetEffect()
    {
        transform.localPosition = originalPos;
        transform.localRotation = originalRot;
    }
}
