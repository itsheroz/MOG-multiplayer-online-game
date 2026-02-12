using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using static LobbyPanelManager;

public class LoginPanel : MonoBehaviour
{
    [Header("Login Panel")]
    public InputField _playerNameInput;
    public Button _loginButton;
    public Button _quickLoginButton;



    public void Awake()
    {
        if (_playerNameInput != null)
            _playerNameInput.text = "Player " + Random.Range(1000, 10000);
        else
            Debug.LogError("Missing player name Input.");

        if (_loginButton != null)
            _loginButton.onClick.AddListener(OnLoginButtonClicked);
        else
            Debug.LogError("Missing Login Button.");

        if (_quickLoginButton != null)
            _quickLoginButton.onClick.AddListener(OnQuickLoginButtonClicked);
        else
            Debug.LogError("Missing Quick Login Button.");
    }

    public void SettingInteractive(bool _isActive)
    {
        _loginButton.interactable = _isActive;
        _quickLoginButton.interactable = _isActive;
    }

    public void OnLoginButtonClicked()
    {
        LobbyPanelManager.instance.SetActivePanel(panelName.SelectionPanel);
        string playerName = _playerNameInput.text;

        if (!playerName.Equals(""))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PlayerListEntry.SettingPlayerProperties();
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }

    public void OnQuickLoginButtonClicked()
    {
        
        LobbyPanelManager.instance.AutoConnect = true;

        string playerName = _playerNameInput.text;

        if (!playerName.Equals(""))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PlayerListEntry.SettingPlayerProperties();
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }
}
