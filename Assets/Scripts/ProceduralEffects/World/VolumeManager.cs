using UnityEngine;
using UnityEngine.Rendering;

public class VolumeManager : MonoBehaviour
{
    [SerializeField] private Volume playerVolume;
    [SerializeField] private Volume PostProcessing;
    [SerializeField] private float playerVolumeSnappiness = 10;

    private PlayerData playerData;
    // ---

    private void LateUpdate()
    {
        float delta = Time.deltaTime;
        playerData = PlayerRegistry.Instance.GetLocalPlayer();

        float currentHealth = 1 - playerData.HealthRatio; // '1 - value' coverts it to the opposite
        playerVolume.weight = Mathf.Lerp(playerVolume.weight, currentHealth, delta * playerVolumeSnappiness);
    }
}
