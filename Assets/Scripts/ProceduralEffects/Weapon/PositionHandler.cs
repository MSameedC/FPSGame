using UnityEngine;

public class PositionHandler : MonoBehaviour, IProceduralEffect
{
    [SerializeField] private Transform aimPosition;
    [Space]
    [SerializeField] private float switchRotSpeed = 20;
    [SerializeField] private float switchPosSpeed = 25;

    private Vector3 targetPos;
    private Vector3 initialPos;

    private Quaternion initialRot;
    private Quaternion targetRot;

    // ---

    public void Initialize(ProceduralRuntimeContext ctx)
    {
        initialPos = transform.localPosition;
        initialRot = transform.localRotation;
    }

    public void Apply(ProceduralRuntimeContext ctx)
    {
        //Cache data
        float delta = ctx.deltaTime;

        targetPos = ctx.isAiming ? aimPosition.localPosition : initialPos;
        targetRot = ctx.isAiming ? aimPosition.localRotation : initialRot;

        float positionFactor = 1f - Mathf.Exp(-switchPosSpeed * delta);
        float rotationFactor = 1f - Mathf.Exp(-switchRotSpeed * delta);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, positionFactor);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, rotationFactor);
    }

    // Resets on weapon switch
    public void ResetEffect()
    {
        targetPos = initialPos;
        targetRot = initialRot;
    }
}
