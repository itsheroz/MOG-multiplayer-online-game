using StarterAssets;
using System;
using UnityEngine;
/// This is the PURE OFFLINE core logic for player stamina.
[RequireComponent(typeof(FirstPersonController))]
[RequireComponent(typeof(StarterAssetsInputs))]
public class PlayerStaminax : MonoBehaviour
{
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float _currentStamina;
    // Dependencies
    private FirstPersonController _fpc;
    private StarterAssetsInputs _input;
    // Public getter for the network adapter to read
    public float CurrentStamina => _currentStamina;
    // The event that the UI (StaminaBarUI) will listen to.
    public event Action<float, float> OnStaminaChanged;
    private void Awake()
    {
        _fpc = GetComponent<FirstPersonController>();
        _input = GetComponent<StarterAssetsInputs>();
        _currentStamina = maxStamina;
    }
    /// The core logic loop. This component will be ENABLED only on the local
    private void Update()
    {
        float newStamina = _currentStamina;
        if (_input != null)
        {
            if (_input.sprint && _input.move != Vector2.zero)
            {
                newStamina -= Time.deltaTime * 30f;
            }
            else
            {
                newStamina += Time.deltaTime * 15f;
            }
            newStamina = Mathf.Clamp(newStamina, 0, maxStamina);
        }
        // Update the stamina value and apply its effects locally.
        SetStamina(newStamina);
    }

    /// Sets the stamina and applies the corresponding effects.
    public void SetStamina(float newStamina)
    {
        _currentStamina = newStamina;
        // Apply sprint speed effects based on the new stamina
        if (_currentStamina > 30)
        {
            _fpc.SprintSpeed = 6f;
        }
        else if (_currentStamina > 15)
        {
            _fpc.SprintSpeed = 3.5f;
        }
        else
        {
            _fpc.SprintSpeed = 2f;
        }
        // Announce the health change to the UI on remote clients.
        OnStaminaChanged?.Invoke(_currentStamina, maxStamina);
    }
}