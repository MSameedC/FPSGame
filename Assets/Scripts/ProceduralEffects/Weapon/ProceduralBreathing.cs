using UnityEngine;

public class ProceduralBreathing : MonoBehaviour, IProceduralEffect
{
    private const float driftAmount = 0.25f;

    [SerializeField] private float driftSpeed = 2f;
    [SerializeField] private float blendSpeed = 4f;
    [SerializeField] private float moveThreshold = 0.05f;

    private float breathWeight = 0f;
    private Quaternion initialRot;

    private float moveMagnitude;

    public void Initialize(ProceduralRuntimeContext ctx)
    {
        initialRot = transform.localRotation;
    }

    public void Apply(ProceduralRuntimeContext ctx)
    {
        bool isMoving = moveMagnitude > moveThreshold;
        float target = (!ctx.isAiming && !isMoving && !ctx.isShooting) ? 1f : 0f;

        breathWeight = Mathf.MoveTowards(breathWeight, target, Time.deltaTime * blendSpeed);

        if (breathWeight <= 0f) return;

        float t = Time.unscaledTime;
        float x = Mathf.Sin(t * driftSpeed * 0.8f) * driftAmount * breathWeight;
        float y = Mathf.Cos(t * driftSpeed * 1.1f) * driftAmount * breathWeight;

        Quaternion drift = Quaternion.Euler(y, x, 0f);
        transform.localRotation = initialRot * drift;
    }

    public void ResetEffect()
    {
        transform.localRotation = initialRot;
    }
}
