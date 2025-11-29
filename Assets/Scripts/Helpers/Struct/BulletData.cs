using UnityEngine;

[System.Serializable]
public struct BulletData
{
    public EnumManager.BulletType bulletType;
    [HideInInspector] public int damage;
    public float speed;
    public float kbForce;
    public float kbRadius;
    public float damageRadius;
    public Material material;
    public LayerMask hitMask;
}
