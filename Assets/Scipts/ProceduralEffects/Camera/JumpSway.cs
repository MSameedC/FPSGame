using UnityEngine;

public class JumpSway : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    [Header("Jump / Fall Impact")]
    [SerializeField] private float impactAmount = 3f;
    [SerializeField] private float impactReturnSpeed = 4f;

    private float impactPitchOffset;
    private float impactReturnVelocity;
    private Quaternion initialRot;

    private void Awake()
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerController>();
    }

    private void Start()
    {
        initialRot = transform.localRotation;

        player.OnJumped += () => PlayJumpImpact(1);
        player.OnLanded += () => PlayLandImpact(1.5f);
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnJumped -= () => PlayJumpImpact();
            player.OnLanded -= () => PlayLandImpact();
        }
    }

    private void LateUpdate()
    {
        float delta = Time.deltaTime;
        UpdateImpactOffset(delta);

        Quaternion impactRot = Quaternion.Euler(impactPitchOffset, 0f, 0f);
        float lerpFactor = 1f - Mathf.Exp(-impactReturnSpeed * delta);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, initialRot * impactRot, lerpFactor);
    }

    private void UpdateImpactOffset(float delta)
    {
        impactPitchOffset = Mathf.SmoothDamp(
            impactPitchOffset,
            0f,
            ref impactReturnVelocity,
            1f / impactReturnSpeed,
            Mathf.Infinity,
            delta
        );
    }

    public void PlayJumpImpact(float intensity = 1f)
    {
        impactPitchOffset -= impactAmount * intensity;
    }

    public void PlayLandImpact(float intensity = 1f)
    {
        impactPitchOffset += impactAmount * intensity;
    }
}

