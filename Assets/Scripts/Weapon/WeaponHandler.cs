using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private GameObject player;
    public WeaponBase CurrentWeapon { get; private set; }
    
    // ---

    private void Start()
    {
        CurrentWeapon = transform.GetChild(0).gameObject.GetComponent<WeaponBase>();
        CurrentWeapon.GetComponent<ProceduralManager>().SetPlayer(player.GetComponent<IMoveable>());
    }

    private void OnEnable()
    {
        CurrentWeapon = transform.GetChild(0).gameObject.GetComponent<WeaponBase>();
        CurrentWeapon.GetComponent<ProceduralManager>().SetPlayer(player.GetComponent<IMoveable>());
    }
}
