using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using UnityEngine.UI;

public class InsideRoomPanel : MonoBehaviourPunCallbacks
{
    [Header("Inside Room Panel")]
    public Button LeaveGameButton;
    public Button StartGameButton;
    public GameObject PlayerListEntryPrefab;


    private Dictionary<int, GameObject> playerListEntries;

    public override void OnEnable()
    {
        base.OnEnable();
        AwakeJoinedRoom();
    }

    public void AwakeJoinedRoom()
    {
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(PlayerListEntryPrefab);
            entry.transform.SetParent(this.transform);
            entry.transform.localScale = Vector3.one;
            SetupPlayerList(p, entry);

            object isPlayerReady;
            if (p.CustomProperties.TryGetValue(PunGameSetting.PLAYER_READY, out isPlayerReady))
            {
                entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }

            playerListEntries.Add(p.ActorNumber, entry);
        }

        StartGameButton.onClick.RemoveAllListeners();
        StartGameButton.onClick.AddListener(OnStartGameButtonClicked);
        StartGameButton.gameObject.SetActive(CheckPlayersReady());

        LeaveGameButton.onClick.RemoveAllListeners();
        LeaveGameButton.onClick.AddListener(OnLeaveGameButtonClicked);

        Hashtable props = new Hashtable
            {
                {PunGameSetting.PLAYER_LOADED_LEVEL, false}
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    protected virtual void SetupPlayerList(Player p, GameObject instanceEntry)
    {
        instanceEntry.GetComponent<PlayerListEntry>().Initialize(p.ActorNumber, p.NickName);
    }

    #region PUN Callback

    public override void OnLeftRoom()
    {
        LobbyPanelManager.instance.SetActivePanel(LobbyPanelManager.panelName.SelectionPanel);


        foreach (GameObject entry in playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        playerListEntries.Clear();
        playerListEntries = null;

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {

        GameObject entry = Instantiate(PlayerListEntryPrefab);
        entry.transform.SetParent(this.transform);
        entry.transform.localScale = Vector3.one;

        SetupPlayerEnteredRoom(newPlayer, entry);

        playerListEntries.Add(newPlayer.ActorNumber, entry);

        StartGameButton.gameObject.SetActive(CheckPlayersReady());

    }

    public virtual void SetupPlayerEnteredRoom(Player newPlayer, GameObject instanceEntry)
    {
        instanceEntry.GetComponent<PlayerListEntry>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
        playerListEntries.Remove(otherPlayer.ActorNumber);

        StartGameButton.gameObject.SetActive(CheckPlayersReady());

    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        GameObject entry;
        if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
        {
            object isPlayerReady;
            if (changedProps.TryGetValue(PunGameSetting.PLAYER_READY, out isPlayerReady))
            {
                entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }
        }

        StartGameButton.gameObject.SetActive(CheckPlayersReady());

    }

    #endregion

    #region UI CALLBACKS

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnStartGameButtonClicked()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel(LobbyPanelManager.instance.GamePlayScene);
    }

    // Call form PlayerListEntry.cs
    public void LocalPlayerPropertiesUpdated()
    {
        StartGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    #endregion

    private bool CheckPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object isPlayerReady;
            if (p.CustomProperties.TryGetValue(PunGameSetting.PLAYER_READY, out isPlayerReady))
            {
                if (!(bool)isPlayerReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }
}
