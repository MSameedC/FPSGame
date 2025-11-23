using UnityEngine;

public struct ProceduralRuntimeContext
{
    public Vector2 lookInput;
    public Vector2 moveInput;
    public bool isAiming;
    public bool isDashing;
    public bool isShooting;
    public bool isGrounded;
    public float moveMagnitude;
    public float deltaTime;
    public WeaponData weaponData;
}
