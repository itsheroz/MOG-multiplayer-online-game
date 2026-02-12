using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerInfoManager : MonoBehaviour
{
    public Slider _healthSlider;
    public Slider _sterminaSlider;
    public TextMeshProUGUI _TextNickName;

    /// <summary>
    /// Add PlayerHealth
    /// </summary>
    PlayerHealth _targetPlayerHealth;
    PlayerStamina _targetPlayerStamina;

    public void Awake()
    {
        /// /// Get Component In Parent 
        _targetPlayerHealth = GetComponentInParent<PlayerHealth>();
        _targetPlayerStamina = GetComponentInParent<PlayerStamina>();

        if (_targetPlayerHealth == null)
        {
            Debug.LogWarning($"{nameof(UIPlayerInfoManager)}: PlayerHealth component not found in parents of {gameObject.name}. UI will not update health.");
        }

        if (_targetPlayerStamina == null)
        {
            Debug.LogWarning($"{nameof(UIPlayerInfoManager)}: PlayerStamina component not found in parents of {gameObject.name}. UI will not update stamina.");
        }
    }

    private void OnEnable()
    {
        /// Register On Change Event
        if (_targetPlayerHealth != null)
        {
            _targetPlayerHealth.OnHealthChanged += UpdateHealthBar;
        }

        if (_targetPlayerStamina != null)
        {
            _targetPlayerStamina.OnStaminaChanged += UpdateSterminaBar;
        }
    }

    private void OnDisable()
    {
        /// Remove On Change Event
        if (_targetPlayerHealth != null)
        {
            _targetPlayerHealth.OnHealthChanged -= UpdateHealthBar;
        }

        if (_targetPlayerStamina != null)
        {
            _targetPlayerStamina.OnStaminaChanged -= UpdateSterminaBar;
        }
    }

    public void SetLocalUI()
    {
        //UI Control
        GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        GetComponent<UIDirectionControl>().enabled = false;
    }

    public void SetNickName(string name)
    {
        if (_TextNickName != null)
            _TextNickName.text = name;
    }

    private void UpdateHealthBar(int current, int max)
    {
        _healthSlider.value = (float)current / max;
    }

    private void UpdateSterminaBar(float current, float max)
    {
        _sterminaSlider.value = (float)current / max;
    }
}
