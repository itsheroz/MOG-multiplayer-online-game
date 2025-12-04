using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IHealer
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int _currentHealth;
    [SerializeField] private int healAmount = 20;

    public int CurrentHealth => _currentHealth;
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        int newHealth = _currentHealth;
        if (newHealth <= 0) return;

        newHealth -= amount;
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

    public void SetHealth(int newHealth)
    {
        if (_currentHealth == newHealth) return;
        _currentHealth = newHealth;
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    // IHealer implementation (offline)
    public void LaunchHeal()
    {
        ReceiveHeal(healAmount);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died! (Offline Logic)");
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
