using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD : MonoBehaviour
{
    private UIDocument uiDoc;

    private ProgressBar staminaBar;
    private ProgressBar healthBar;
    private ProgressBar heatBar;

    PlayerData player;

    private void Awake()
    {
        uiDoc = GetComponent<UIDocument>();

        var root = uiDoc.rootVisualElement;

        healthBar = root.Q<ProgressBar>("health-bar");
        heatBar = root.Q<ProgressBar>("heat-bar");
        staminaBar = root.Q<ProgressBar>("stamina-bar");
    }

    private void Update()
    {
        if (player == null)
        {
            player = PlayerRegistry.Instance.GetLocalPlayer();

            if (player != null)
            {
                player.GetStamina().OnStaminaChanged += UpdateStamina;
                player.GetHealth().OnHealthChanged += UpdateHealth;
                player.GetWeapon().OnWeaponHeatChanged += UpdateHeat;

                UpdateHealth(player.CurrentHealth, player.MaxHealth);
                UpdateStamina(player.CurrentStamina, player.MaxStamina);
                UpdateHeat(player.CurrentHeat, player.MaxHeat);
            }
        }
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