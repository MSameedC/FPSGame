using UnityEngine;

public class PlayerData : IPlayerData
{
    [SerializeField] private PlayerProfile Profile;
    private WeaponHandler WeaponParent;
    private PlayerController Player;
    private PlayerStamina Stamina;
    private PlayerHealth Health;

    // ---

    public void Initialize(PlayerController player, WeaponHandler weaponHandler, PlayerHealth health, PlayerStamina stamina, PlayerProfile profile)
    {
        Player = player;
        WeaponParent = weaponHandler;
        Health = health;
        Profile = profile;
        Stamina = stamina;
    }

    public GameObject PlayerObj => Player.gameObject;

    // --- Profile ---
    public string PlayerName => Profile.name;
    public int PlayerId => Profile.playerId;

    // --- Score ----
    public int Score;

    // --- Health ---
    public int CurrentHealth => Health.CurrentHealth;
    public int MaxHealth => Health.MaxHealth;
    public bool IsAlive => CurrentHealth > 0;

    // --- Stamina ---
    public float CurrentStamina => Stamina.CurrentStamina;
    public float MaxStamina => Stamina.MaxStamina;

    // --- Ammo (from current weapon if any) ---
    public float CurrentHeat => WeaponParent.CurrentWeapon.CurrentHeat;
    public float MaxHeat => WeaponData.maxHeat;

    // --- Movement / State ---
    public bool IsDashing => Player.IsDashing;
    public bool IsGrounded => Player.IsGrounded;
    public float MoveMagnitude => Player.MoveMagnitude;

    // --- Combat ---
    public bool IsAiming => WeaponParent.CurrentWeapon != null && WeaponParent.CurrentWeapon.IsAiming();
    public bool IsShooting => WeaponParent.CurrentWeapon != null && WeaponParent.CurrentWeapon.IsShooting;

    // --- Helpers ---
    public UpgradeableWeaponData UpgradedData => WeaponParent.CurrentWeapon?.GetUpgradedData();
    public WeaponData WeaponData => WeaponParent.CurrentWeapon?.GetWeaponData();
    public WeaponController GetWeapon() => WeaponParent.CurrentWeapon;
    public PlayerHealth GetHealth() => Health;
    public PlayerStamina GetStamina() => Stamina;

    public float HealthRatio
    {
        get
        {
            if (MaxHealth <= 0) return 0; // Prevent division by zero
            float max = MaxHealth * 0.5f;
            return Mathf.Clamp01(CurrentHealth / max); // Always returns 0-1
        }
    }
}
