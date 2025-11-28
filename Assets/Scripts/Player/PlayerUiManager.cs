using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUiManager : MonoBehaviour
{
    [SerializeField] private UIDocument HudUi;

    private ProgressBar staminaBar;
    private ProgressBar healthBar;
    private ProgressBar heatBar;

    private PlayerData player;
    private VisualElement root;

    private void Awake()
    {
        root = HudUi.rootVisualElement;

        healthBar = root.Q<ProgressBar>("health-bar");
        heatBar = root.Q<ProgressBar>("heat-bar");
        staminaBar = root.Q<ProgressBar>("stamina-bar");
    }
    
    private void Start()
    {
        PlayerRegistry.Instance.OnPlayerRegistered += OnPlayerRegister;

        var existingPlayer = PlayerRegistry.Instance.GetLocalPlayer();
        if (existingPlayer != null)
            OnPlayerRegister(existingPlayer);
    }

    private void Update()
    {
        // Disable UI on Aim
        root.style.display = InputManager.IsAiming ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void OnPlayerRegister(PlayerData data)
    {
        player ??= PlayerRegistry.Instance.GetLocalPlayer();
        SubscribeToEvents();
        UpdateUi();
    }
    
    private void SubscribeToEvents()
    {
        if (player == null) return;
        player.GetStamina().OnStaminaChanged += UpdateStamina;
        player.GetHealth().OnHealthChanged += UpdateHealth;
        player.GetWeapon().OnWeaponHeatChanged += UpdateHeat;
    }

    private void UpdateUi()
    {
        if (player == null) return;
        UpdateHealth(player.CurrentHealth, player.MaxHealth);
        UpdateStamina(player.CurrentStamina, player.MaxStamina);
        UpdateHeat(player.CurrentHeat, player.MaxHeat);
    }

    private void OnDisable()
    {
        if (player == null) return;
        player.GetStamina().OnStaminaChanged -= UpdateStamina;
        player.GetHealth().OnHealthChanged -= UpdateHealth;
        player.GetWeapon().OnWeaponHeatChanged -= UpdateHeat;
    }


    private void UpdateHealth(int current, int max)
    {
        healthBar.lowValue = 0;
        healthBar.highValue = max;
        healthBar.value = current;
    }

    private void UpdateHeat(float current, float max)
    {
        heatBar.lowValue = 0;
        heatBar.highValue = max;
        heatBar.value = current;
    }

    private void UpdateStamina(float current, float max)
    {
        staminaBar.lowValue = 0;
        staminaBar.highValue = max;
        staminaBar.value = current;
    }
}