using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [Space]
    [SerializeField] private Vector3 forceRange;
    [SerializeField] private float intensity = 2;
    [SerializeField] private float duration = 1;

    private CinemachineImpulseSource ImpulseSource;

    // ---

    private void Awake()
    {
        ImpulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Start()
    {
        ImpulseSource.ImpulseDefinition.ImpulseDuration = duration;
        ImpulseSource.DefaultVelocity = forceRange;

        player.OnSlamImpact += () => InvokeShake(intensity);
    }

    private void InvokeShake(float intensity)
    {
        ImpulseSource.GenerateImpulse(intensity);
    }
}
