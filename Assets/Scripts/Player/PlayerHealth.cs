using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public event Action<int, int> OnHealthChanged;
    public event Action OnHealed;
    public event Action OnHurt;
    [SerializeField] private int maxHealth = 100;
    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;

    // ---

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnHurt?.Invoke();

        if (CurrentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnHealed?.Invoke();
    }

    private void Die()
    {
        // hook GameManager or Respawn system later
        GameManager.Instance.SetState(GameState.GameOver);
    }
}
