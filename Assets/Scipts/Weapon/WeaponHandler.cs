using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private PlayerController playerManager;
    [SerializeField] private InputHandler input;

    private IPlayerState player;
    private WeaponController lastWeapon;
    public WeaponController CurrentWeapon => lastWeapon;

    private void Start()
    {
        player = playerManager;

        // Bind initial weapon if present
        var initialWeapon = GetComponentInChildren<WeaponController>();
        if (initialWeapon != null)
            EquipWeapon(initialWeapon);
    }

    public void EquipWeapon(WeaponController newWeapon)
    {
        if (newWeapon == lastWeapon) return;

        if (lastWeapon != null)
            UnbindFrom(lastWeapon);

        if (newWeapon != null)
            BindTo(newWeapon);

        lastWeapon = newWeapon;
    }

    private void BindTo(WeaponController weapon)
    {
        if (weapon == null || input == null) return;

        input.OnShootPressed += weapon.OnShootPressed;
        input.OnShootReleased += weapon.OnShootCancelled;

        weapon.SetInput(input);

        ProceduralManager proceduralManager = weapon.GetComponent<ProceduralManager>();
        proceduralManager.SetInput(input);
        proceduralManager.SetPlayer(player);
    }

    private void UnbindFrom(WeaponController weapon)
    {
        if (weapon == null || input == null) return;

        input.OnShootPressed -= weapon.OnShootPressed;
        input.OnShootReleased -= weapon.OnShootCancelled;

        weapon.SetInput(null);

        ProceduralManager proceduralManager = weapon.GetComponent<ProceduralManager>();
        proceduralManager.SetInput(null);
        proceduralManager.SetPlayer(null);
    }

    private void OnDisable()
    {
        if (lastWeapon != null)
            UnbindFrom(lastWeapon);
    }

    private void OnDestroy()
    {
        if (lastWeapon != null)
            UnbindFrom(lastWeapon);
    }
}