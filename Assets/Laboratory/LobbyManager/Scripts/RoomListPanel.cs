using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListPanel : MonoBehaviourPunCallbacks
{
    [Header("Room List Panel")]
    public RectTransform _roomListContent;
    public GameObject _roomListEntryPrefab;
    public Button _cancelButton;

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;

    private void Awake()
    {
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();

        if (_roomListContent == null)
            Debug.LogError("Missing room list Content.");

        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(OnBackButtonClicked);
        else
            Debug.LogError("Missing Cancel Button.");
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // Add new room info to cache
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    private void UpdateRoomListView()
    {
        print(cachedRoomList.Count);
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = Instantiate(_roomListEntryPrefab);
            entry.transform.SetParent(_roomListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<RoomListEntry>().Initialize(info.Name, (byte)info.PlayerCount, (byte)info.MaxPlayers);

            roomListEntries.Add(info.Name, entry);
        }
    }

    private void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        roomListEntries.Clear();
    }

    public void OnBackButtonClicked()
    {
        LobbyPanelManager.instance.SetActivePanel(LobbyPanelManager.panelName.SelectionPanel);
    }

    #region PUN Callback

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }

    public override void OnLeftLobby()
    {
        cachedRoomList.Clear();

        ClearRoomListView();
    }

    #endregion
}
