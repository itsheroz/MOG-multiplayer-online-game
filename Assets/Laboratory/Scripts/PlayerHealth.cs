using System;
using UnityEngine;

/// <summary>
/// This is the PURE OFFLINE core logic for player health.
/// It now provides public methods for an adapter to read and write its state.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    [SerializeField] private int _currentHealth;

    // [NEW] Public getter so the network adapter can read the health for serialization.
    public int CurrentHealth => _currentHealth;

    // The event that the UI (HealthBarUI) will listen to.
    public event Action<int, int> OnHealthChanged;

    private void Awake() // [MODIFIED] No longer 'protected' or 'virtual'
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int amount) // [MODIFIED] No longer 'virtual'
    {
        int newHealth = _currentHealth;

        if (newHealth <= 0) return;

        newHealth += amount;
        if (newHealth < 0) newHealth = 0;

        SetHealth(newHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void ReceiveHeal(int amount)
    {
        int newHealth = _currentHealth;

        if (newHealth <= 0 || newHealth == maxHealth) return;

        newHealth += amount;
        if (newHealth > maxHealth) newHealth = maxHealth;

        SetHealth(newHealth);
    }

    /// <summary>
    /// [NEW] This public method allows the network adapter (PUN_PlayerHealth)
    /// to force-set the health value received from the network stream.
    /// </summary>
    public void SetHealth(int newHealth)
    {
        // Only update if the health has actually changed.
        if (_currentHealth == newHealth) return;

        _currentHealth = newHealth;

        // Announce the health change to the UI on remote clients.
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died! (Offline Logic)");
    }
}