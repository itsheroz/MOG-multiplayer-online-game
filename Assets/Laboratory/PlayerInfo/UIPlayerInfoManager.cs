using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerInfoManager : MonoBehaviour
{
    public Slider _healthSlider;
    public Slider _sterminaSlider;
    public TextMeshProUGUI _TextNickName;
    private PlayerHealth _targetPlayerHealth;

    public void Awake()
    {
        /// /// Get Component In Parent
        _targetPlayerHealth = GetComponentInParent<PlayerHealth>();
    }
    private void OnEnable()
    {
        /// Register On Change Event
        _targetPlayerHealth.OnHealthChanged += UpdateHealthBar;
    }
    private void OnDisable()
    {
        /// Remove On Change Event
        _targetPlayerHealth.OnHealthChanged -= UpdateHealthBar;
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
