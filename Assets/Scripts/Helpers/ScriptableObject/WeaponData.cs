using UnityEngine;

[CreateAssetMenu(fileName = "WeaponProfile", menuName = "Create Data/New Weapon profile")]
public class WeaponData : ScriptableObject
{
    [Header("Stats")]
    public int damage;
    [Min(0.01f)]
    public float fireRate;
    public float range;
    [Space]
    [Min(1)]
    public float heatAddAmount;
    [Min(1)]
    public float heatDecayAmount;
    [Min(10)]
    public float maxHeat;
    [Min(1)]
    public float heatDecayRate;
    [Min(1)]
    public float postOverheatDelay;

    [Header("Recoil")]
    public RecoilSettings recoil;

    [Header("Aim")]
    public AimSettings aim;

    [Header("Weapon")]
    public WeaponSettings weapon;
}

[System.Serializable]
public struct RecoilSettings
{
    public float recoilX;
    public float recoilY;
    public float recoilZ;
}

[System.Serializable]
public struct AimSettings
{
    public float aimSway;
    public float aimRecoil;
    public float aimReturnSpeed;
}

[System.Serializable]
public struct WeaponSettings
{
    public float recoilReturnSpeed;
    public float recoilSnappiness;
    public float swaySpringStrength;
    public float swayDamping;
}
