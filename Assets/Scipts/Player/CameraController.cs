using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Sensitivity")]
    [Range(0.1f, 1f)][SerializeField] private float ySens = 1f;
    [Range(0.1f, 1f)][SerializeField] private float xSens = 1f;

    [SerializeField] private float smoothness = 30f;
    [SerializeField] private float xRotLimit = 89f;
    [SerializeField] private float maxMouseDelta = 10f;

    private Transform playerBody;

    // Rotation state
    private float mouseX;
    private float mouseY;
    private float targetRotX;
    private float targetRotY;
    private float currentRotX;
    private float currentRotY;
    private const float Sensitivity = 2f;

    private IInputProvider input;

    private void Start()
    {
        playerBody = transform.parent;
        input = playerBody.GetComponent<IInputProvider>();

        // Update Cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        float delta = Time.deltaTime;

        Vector2 rawLookInput = input.LookInput;
        Vector2 clampedLookInput = Vector2.ClampMagnitude(rawLookInput, maxMouseDelta);

        // float adsRate = isAiming ? aimMultiplier : 1f;
        mouseX = clampedLookInput.x * Sensitivity * xSens;
        mouseY = clampedLookInput.y * Sensitivity * ySens;

        targetRotX -= mouseY;
        targetRotY += mouseX;

        targetRotX = Mathf.Clamp(targetRotX, -xRotLimit, xRotLimit);

        float smooth = 1f - Mathf.Exp(-smoothness * delta);

        currentRotX = Mathf.LerpAngle(currentRotX, targetRotX, smooth);
        currentRotY = Mathf.LerpAngle(currentRotY, targetRotY, smooth);

        transform.localRotation = Quaternion.Euler(currentRotX, 0f, 0f);
        playerBody.rotation = Quaternion.Euler(0, currentRotY, 0f);
    }
}

