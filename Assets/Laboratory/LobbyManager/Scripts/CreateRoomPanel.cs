using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CreateRoomPanel : MonoBehaviour
{
    [Header("Create Room Panel")]
    public InputField _roomNameInputField;
    public InputField _maxPlayersInputField;
    public Toggle _teamModeToggle;
    public Button _cancelButton;
    public Button _createRoomButton;

    private void Awake()
    {
        if (_roomNameInputField != null)
            _roomNameInputField.text = "";
        else
            Debug.LogError("Missing room name Input.");

        if (_maxPlayersInputField != null)
            _maxPlayersInputField.text = "";
        else
            Debug.LogError("Missing  max player Input.");

        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(OnBackButtonClicked);
        else
            Debug.LogError("Missing join Random Room Button.");

        if (_createRoomButton != null)
            _createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        else
            Debug.LogError("Missing Room List Room Button.");
    }

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        LobbyPanelManager.instance.SetActivePanel(LobbyPanelManager.panelName.SelectionPanel);
    }

    public void OnCreateRoomButtonClicked()
    {
        string roomName = _roomNameInputField.text;
        roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

        byte maxPlayers;
        byte.TryParse(_maxPlayersInputField.text, out maxPlayers);
        maxPlayers = (byte)Mathf.Clamp(maxPlayers, 2, 8);

        RoomOptions options = new RoomOptions {
            MaxPlayers = maxPlayers ,
            CustomRoomProperties = new Hashtable() {
                { PunGameSetting.TEAMMODE , _teamModeToggle.isOn }
            }
        };

        //options.CustomRoomPropertiesForLobby = { "map", "ai" };

        PhotonNetwork.CreateRoom(roomName, options, null);
    }
}
