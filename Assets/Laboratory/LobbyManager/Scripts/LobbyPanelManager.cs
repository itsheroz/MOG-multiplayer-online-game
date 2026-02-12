using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;

public class LobbyPanelManager : MonoBehaviourPunCallbacks
{
    [Header("Scenes")]
    //public  LobbyScene;
    public string GamePlayScene = "GameScene";
    public bool isTeamMode = false;

    /// <summary>Connect automatically? If false you can set this to true later on or call ConnectUsingSettings in your own scripts.</summary>
    public bool AutoConnect = false;


    public enum panelName
    {
        LoginPanel = 0,
        SelectionPanel = 1,
        CreateRoomPanel = 2,
        JoinRandomRoomPanel = 3,
        RoomLisPanel = 4,
        InsideroomPanel = 5
    }
    
    [Header("Panel UI")]
    public GameObject[] _panelList;
    public static LobbyPanelManager instance;

    private void Awake()
    {
        instance = this;

        PhotonNetwork.AutomaticallySyncScene = true;
        
    }

    public void Start()
    {
        _panelList[(int)panelName.LoginPanel].GetComponent<LoginPanel>().SettingInteractive(false);
        PhotonNetwork.ConnectUsingSettings();
    }

    public void SetActivePanel(panelName EnumActivePanel)
    {
        string activePanel = _panelList[(int)EnumActivePanel].name;

        foreach (GameObject go in _panelList)
            go.SetActive(activePanel.Equals(go.name));
    }

    #region PUN CALLBACKS

    public override void OnConnectedToMaster()
    {
        if(this.AutoConnect)
            PhotonNetwork.JoinRandomRoom();
        else
            _panelList[(int)panelName.LoginPanel].GetComponent<LoginPanel>().SettingInteractive(true);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetActivePanel(panelName.SelectionPanel);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (this.AutoConnect)
            CreateRoom();
        else
            SetActivePanel(panelName.SelectionPanel);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    public override void OnJoinedRoom()
    {
        if (AutoConnect)
        {
            PhotonNetwork.CurrentRoom.IsOpen = AutoConnect;
            PhotonNetwork.CurrentRoom.IsVisible = AutoConnect;

            PlayerListEntry.SettingPlayerProperties();
            PhotonNetwork.LoadLevel(GamePlayScene);
        }
        else
            SetActivePanel(panelName.InsideroomPanel);
    }
    #endregion

    void CreateRoom()
    {
        string roomName = "Room " + Random.Range(1000, 10000);

        RoomOptions options = new RoomOptions { MaxPlayers = 8 };

        PhotonNetwork.CreateRoom(roomName, options, null);
    }
}
