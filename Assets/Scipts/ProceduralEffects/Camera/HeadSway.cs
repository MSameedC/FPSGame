using UnityEngine;

public class HeadSway : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    
    [Header("Move Sway")]
    [SerializeField] private float swayAmount = 0.5f;
    [SerializeField] private float swayReturnSpeed = 3f;
    
    [Header("Jump Sway")]
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
        if (player == null) return;
        player.OnJumped -= () => PlayJumpImpact();
        player.OnLanded -= () => PlayLandImpact();
    }

    private float finalSway;
    private float finalImpactOffset;
    
    private void LateUpdate()
    {
        float delta = Time.deltaTime;
        
        UpdateImpactOffset(delta);
        
        float sway = InputManager.MoveInput.x * swayAmount;
        float swayLerpFactor = 1f - Mathf.Exp(-swayReturnSpeed * delta);
        finalSway = Mathf.Lerp(finalSway, -sway, swayLerpFactor);
        Quaternion targetSway = Quaternion.Euler(0f, 0f, finalSway);
        
        float impactLerpFactor = 1f - Mathf.Exp(-impactReturnSpeed * delta);
        finalImpactOffset = Mathf.Lerp(finalImpactOffset, impactPitchOffset, impactLerpFactor);
        Quaternion targetImpact = Quaternion.Euler(finalImpactOffset, 0f, 0f);
        
        transform.localRotation = initialRot * targetImpact * targetSway;
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

    private void PlayJumpImpact(float intensity = 1f)
    {
        impactPitchOffset -= impactAmount * intensity;
    }

    private void PlayLandImpact(float intensity = 1f)
    {
        impactPitchOffset += impactAmount * intensity;
    }
}

