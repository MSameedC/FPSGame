using UnityEngine;

public class ProceduralManager : MonoBehaviour
{
    private bool isShooting;
    private WeaponData weaponData;

    private IProceduralEffect[] effects;
    private IMoveable player;

    #region Unity

    private void Awake()
    {
        effects = GetComponentsInChildren<IProceduralEffect>(true); // Creating Cache
    }

    private void Start()
    {
        var baseContext = BuildBaseContext();

        foreach (var effect in effects)
            effect.Initialize(baseContext);
    }

    private void Update()
    {
        var runtimeContext = BuildRuntimeContext();

        foreach (var effect in effects)
            effect.Apply(runtimeContext);
    }

    #endregion

    #region Context

    // Required every frame because data changes
    public ProceduralRuntimeContext BuildRuntimeContext()
    {
        return new ProceduralRuntimeContext
        {
            moveInput = InputManager.MoveInput,
            lookInput = InputManager.LookInput,
            isAiming = InputManager.IsAiming,
            isShooting = isShooting,
            isDashing = InputManager.IsDashing,
            isGrounded = player.IsGrounded,
            moveMagnitude = player.MoveMagnitude,
            deltaTime = Time.deltaTime
        };
    }

    // Only required once
    public ProceduralRuntimeContext BuildBaseContext()
    {
        return new ProceduralRuntimeContext
        {
            weaponData = weaponData
        };
    }

    public void ResetEffects()
    {
        foreach (var effect in effects)
            effect.ResetEffect();
    }

    #endregion

    #region Accessors

    public void SetShooting(bool shooting)
    {
        isShooting = shooting;
    }

    public void SetWeaponData(WeaponData data)
    {
        weaponData = data;
    }

    public void SetPlayer(IMoveable player)
    {
        this.player = player;
    }

    #endregion
}