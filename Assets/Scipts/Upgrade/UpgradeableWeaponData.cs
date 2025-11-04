using System;
using UnityEngine;

[System.Serializable]
public class UpgradeableWeaponData
{
    public WeaponData baseData;

    // Modified stats (start with base values)
    public float Damage { get; private set; }
    public float FireRate { get; private set; }
    public float MaxHeat { get; private set; }
    public float HeatDecayRate { get; private set; }
    // Add other stats you want to upgrade...

    public event Action OnStatsChanged;

    public void Initialize()
    {
        if (baseData == null) return;

        Damage = baseData.damage;
        FireRate = baseData.fireRate;
        MaxHeat = baseData.maxHeat;
        HeatDecayRate = baseData.heatDecayRate;
    }

    public void ApplyUpgrade(WeaponUpgrade upgrade)
    {
        Damage = ModifyStat(Damage, upgrade.damageAdd, upgrade.damageMultiply);
        FireRate = ModifyStat(FireRate, upgrade.fireRateAdd, upgrade.fireRateMultiply);
        MaxHeat = ModifyStat(MaxHeat, upgrade.maxHeatAdd, upgrade.maxHeatMultiply);
        HeatDecayRate = ModifyStat(HeatDecayRate, upgrade.heatDecayAdd, upgrade.heatDecayMultiply);

        OnStatsChanged?.Invoke();

        Debug.Log($"Applied upgrade: {upgrade.upgradeName}");
    }

    private float ModifyStat(float baseValue, float additive, float multiplicative)
    {
        return (baseValue + additive) * multiplicative;
    }

    // Method to get current stats for UI display
    public WeaponStats GetCurrentStats()
    {
        return new WeaponStats
        {
            damage = Damage,
            fireRate = FireRate,
            maxHeat = MaxHeat,
            heatDecayRate = HeatDecayRate
        };
    }
}

[System.Serializable]
public struct WeaponStats
{
    public float damage;
    public float fireRate;
    public float maxHeat;
    public float heatDecayRate;
}