using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    [SerializeField] private PlayerProfile profile;
    [SerializeField] private WeaponHandler weapon;

    private PlayerController player;
    private PlayerStamina stamina;
    private PlayerHealth health;

    public PlayerData Data { get; private set; }

    // ---

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        stamina = GetComponent<PlayerStamina>();
        health = GetComponent<PlayerHealth>();

        // Create PlayerData for this player
        Data = new PlayerData();
        Data.Initialize(player, weapon, health, stamina, profile);
    }

    private void Start()
    {
        // Register in the global registry
        PlayerRegistry.Instance.RegisterPlayer(profile, Data);
    }

    private void OnDestroy()
    {
        // Clean up when player is destroyed
        if (PlayerRegistry.Instance != null)
            PlayerRegistry.Instance.UnregisterPlayer(profile);
    }
}