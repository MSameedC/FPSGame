using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private PlayerController playerManager;

    private IMoveable player;
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
        if (weapon == null) return;

        InputManager.OnShootPressed += weapon.OnShootPressed;
        InputManager.OnShootReleased += weapon.OnShootCancelled;

        ProceduralManager proceduralManager = weapon.GetComponent<ProceduralManager>();
        proceduralManager.SetPlayer(player);
    }

    private void UnbindFrom(WeaponController weapon)
    {
        if (weapon == null) return;

        InputManager.OnShootPressed -= weapon.OnShootPressed;
        InputManager.OnShootReleased -= weapon.OnShootCancelled;

        ProceduralManager proceduralManager = weapon.GetComponent<ProceduralManager>();
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