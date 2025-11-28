using UnityEngine;

[CreateAssetMenu(fileName = "WeaponUpgradeProfile", menuName = "Create Data/New Weapon Upgrade profile")]
public class WeaponUpgrade : ScriptableObject
{
    public string upgradeName;
    public string positiveDescription;
    public string negativeDescription;

    [Header("Stat Modifiers")]
    public float damageAdd = 0;
    public float damageMultiply = 1f;

    public float fireRateAdd = 0;
    public float fireRateMultiply = 1f;

    public float maxHeatAdd = 0;
    public float maxHeatMultiply = 1f;

    public float heatDecayAdd = 0;
    public float heatDecayMultiply = 1f;

    [Header("Debuffs (negative values for trade-offs)")]
    public float accuracyPenalty = 0f; // Example trade-off
}