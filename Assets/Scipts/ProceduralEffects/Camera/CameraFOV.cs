using Unity.Cinemachine;
using UnityEngine;

public class CameraFOV : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private CinemachineCamera cam;
    [Space]
    [SerializeField] private float baseFOV = 60;
    [SerializeField] private float aimFOV = 45;
    [SerializeField] private float intensity = 0.8f;
    [SerializeField] private float fovSnappiness = 2;
    [SerializeField] private float aimSnappiness = 8;

    private float currentFOV;

    // ---

    private void Start()
    {
        cam.Lens.FieldOfView = baseFOV;
    }

    private void LateUpdate()
    {
        if (cam == null || player == null) return;

        float delta = Time.deltaTime;
        float movementRate = Mathf.Clamp01(player.MoveMagnitude) * intensity + 1;
        float moveFOV = baseFOV * movementRate;

        float snappiness = InputManager.IsAiming ? aimSnappiness : fovSnappiness;
        float targetFOV = InputManager.IsAiming ? aimFOV : moveFOV;

        currentFOV = Mathf.Lerp(currentFOV, targetFOV, delta * snappiness);
        cam.Lens.FieldOfView = Mathf.Clamp(currentFOV, 30, 120);
    }
}