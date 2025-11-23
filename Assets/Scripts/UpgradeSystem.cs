using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    // ---

    [SerializeField] private WeaponUpgrade[] allUpgrades;

    private WeaponUpgradeManager weaponUpgradeManager;
    private PlayerRegistry PlayerRegistry;
    private WaveManager WaveManager;
    private VisualElement root;
    private UIDocument uiDoc;

    // ---

    #region Unity

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;

        uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;
    }

    private void Start()
    {
        // Get Components
        PlayerRegistry = PlayerRegistry.Instance;
        WaveManager = WaveManager.Instance;

        bool playerHasWeapon = PlayerRegistry?.GetLocalPlayer()?.GetWeapon();
        if (!playerHasWeapon)
        {
            Debug.LogWarning("Cannot find player weapon for upgrades!");
            return;
        }

        GameObject playerWeapon = PlayerRegistry?.GetLocalPlayer()?.GetWeapon().gameObject;

        if (weaponUpgradeManager == null)
            weaponUpgradeManager = playerWeapon.GetComponent<WeaponUpgradeManager>();

        if (weaponUpgradeManager == null)
        {
            Debug.LogWarning("Cannot find WeaponUpgradeManager in player weapon object for upgrades!");
            return;
        }
            
        // Subscribe actions
        WaveManager.OnWaveCompleted += ShowUpgradeScreen;        

        // Set UI on GameStart
        SetCursor(false, CursorLockMode.Locked);
        SetUiVisibility(DisplayStyle.None);
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (WaveManager != null)
            WaveManager.OnWaveCompleted -= ShowUpgradeScreen;
    }

    #endregion

    #region Upgrade

    private void ShowUpgradeScreen()
    {
        SetCursor(true, CursorLockMode.None);
        SetUiVisibility(DisplayStyle.Flex);

        Time.timeScale = 0;

        // Show 3 random upgrade buttons
        for (int i = 1; i <= 3; i++)
        {
            // Generate random upgrade
            int r = Random.Range(0, allUpgrades.Length);
            WeaponUpgrade upgrade = allUpgrades[r];

            VisualElement root = this.root.Q<VisualElement>(i.ToString());
            InitializeButton(root, upgrade);
        }
    }

    private void ApplyUpgrade(WeaponUpgrade upgrade)
    {
        SetCursor(false, CursorLockMode.Locked);
        SetUiVisibility(DisplayStyle.None);

        Time.timeScale = 1;
        // Apply effect based on upgradeType
        weaponUpgradeManager.ApplyUpgradeToWeapon(upgrade);
    }

    #endregion

    #region Helpers

    private void SetCursor(bool visibility, CursorLockMode mode)
    {
        Cursor.visible = visibility;
        Cursor.lockState = mode;
    }

    private void SetUiVisibility(DisplayStyle display)
    {
        root.style.display = display;
    }

    private void InitializeButton(VisualElement root, WeaponUpgrade upgrade)
    {
        Button btn = root.Q<Button>("upgrade-btn");

        btn.clicked -= () => ApplyUpgrade(upgrade); // Remove existing first
        btn.clicked += () => ApplyUpgrade(upgrade);

        root.Q<Label>("name").text = upgrade.name;
        root.Q<Label>("positive").text = upgrade.positiveDescription;
        root.Q<Label>("negative").text = upgrade.negativeDescription;
    }

    #endregion
}
