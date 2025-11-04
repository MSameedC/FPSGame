using System;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    public event Action<float, float> OnStaminaChanged;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float recoverSpeed = 25f;
    [SerializeField] private float recoveryDelay = 1f;

    public float CurrentStamina { get; private set; }
    public float MaxStamina => maxStamina;

    private float recoveryTimer;
    private bool isRecovering;

    private void Start()
    {
        CurrentStamina = MaxStamina;
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        // ONLY check if we're not already at max stamina
        if (CurrentStamina < MaxStamina)
        {
            if (!isRecovering)
            {
                recoveryTimer += delta;
                if (recoveryTimer >= recoveryDelay)
                {
                    isRecovering = true;
                    recoveryTimer = 0f;
                }
            }
            else
            {
                // Smooth recovery
                CurrentStamina += delta * recoverSpeed;
                CurrentStamina = Mathf.Min(CurrentStamina, MaxStamina);
                OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
            }
        }
        else if (isRecovering)
        {
            // Just reached max stamina
            isRecovering = false;
            recoveryTimer = 0f;
        }
    }

    public void TakeStamina(int amount)
    {
        float oldStamina = CurrentStamina;
        CurrentStamina = Mathf.Max(CurrentStamina - amount, 0);

        recoveryTimer = 0f;
        isRecovering = false;

        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }
}