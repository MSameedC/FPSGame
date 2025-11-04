using UnityEngine;

public class WeaponUpgradeManager : MonoBehaviour
{
    private UpgradeableWeaponData weaponData;

    private void Start()
    {
        weaponData = GetComponent<WeaponController>().GetUpgradedData();
    }
    public void ApplyUpgradeToWeapon(WeaponUpgrade upgrade)
    {
        weaponData.ApplyUpgrade(upgrade);
    }

    // public WeaponUpgrade[] GetRandomWeaponUpgrades(int count)
    // {
    //     // Return random upgrades from available pool
    // }
}