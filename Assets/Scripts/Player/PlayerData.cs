using UnityEngine;

public class PlayerData : IPlayerData
{
    private WeaponHandler WeaponParent;
    private PlayerController Player;
    private PlayerStamina Stamina;
    private PlayerHealth Health;

    // ---

    public void Initialize(PlayerController player, WeaponHandler weaponHandler, PlayerHealth health, PlayerStamina stamina)
    {
        Player = player;
        WeaponParent = weaponHandler;
        Health = health;
        Stamina = stamina;
    }

    public GameObject PlayerObj => Player.gameObject;

    // --- Profile ---
    public int PlayerId;

    // --- Health ---
    public int CurrentHealth => Health.CurrentHealth;
    public int MaxHealth => Health.MaxHealth;
    public bool IsAlive => CurrentHealth > 0;

    // --- Stamina ---
    public float CurrentStamina => Stamina.CurrentStamina;
    public float MaxStamina => Stamina.MaxStamina;

    // --- Ammo (from current weapon if any) ---
    public float CurrentHeat => WeaponParent.CurrentWeapon.CurrentHeat;
    public float MaxHeat => WeaponParent.CurrentWeapon.MaxHeat;

    // --- Helpers ---
    public WeaponBase GetWeapon() => WeaponParent.CurrentWeapon;
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
