using UnityEngine;

public class MovementHandler : MonoBehaviour, IProceduralEffect
{
    [Header("Movement")]
    [SerializeField] private float moveAmplitude = 0.2f;
    [SerializeField] private float moveFrequency = 2f;
    [SerializeField] private float snappiness = 10f;
    [SerializeField] private float moveX = 0.25f;
    [SerializeField] private float moveY = 0.5f;

    private float aimRate;

    private Vector3 initialPos;
    private Vector3 targetOffset;

    // ---

    public void Initialize(ProceduralRuntimeContext ctx)
    {
        initialPos = transform.localPosition;

        SetConfig(ctx.weaponData);
    }

    public void Apply(ProceduralRuntimeContext ctx)
    {
        // Cache data
        float delta = ctx.deltaTime;
        float moveFactor = ctx.moveMagnitude;
        bool isAiming = ctx.isAiming;

        // Final Values
        float finalAmp = (isAiming ? aimRate : 1) * moveAmplitude;
        float finalFreq = (isAiming ? aimRate : 1) * moveFrequency;

        bool active = ctx.isGrounded && !ctx.isShooting && moveFactor > 0.01f;

        if (active)
        {
            float step = Time.time * finalFreq;

            float posX = Mathf.Sin(step) * moveX * finalAmp * moveFactor;
            float posY = Mathf.Sin(step * 2f) * moveY * finalAmp * moveFactor;

            targetOffset = new Vector3(posX, posY, 0f);
        }
        else
        {
            targetOffset = Vector3.zero;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPos + targetOffset, snappiness * delta);
    }

    public void SetConfig(WeaponData data)
    {
        aimRate = data.aim.aimRate;
    }

    public void ResetEffect()
    {
        transform.localPosition = initialPos;
    }
}
