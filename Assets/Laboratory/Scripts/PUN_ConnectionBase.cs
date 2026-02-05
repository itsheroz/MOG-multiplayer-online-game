using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the target connection state for the auto-connect feature.
/// </summary>
public enum AutoConnectMode
{
    None,               // Do not connect automatically.
    ConnectToMaster,    // Connect and wait at Master Server (for Matchmaking).
    ConnectToLobby,     // Connect and join the Lobby (to see room lists).
    ConnectToRoom       // Connect and join a random room immediately.
}

/// <summary>
/// Base implementation for PUN2 connection logic, mimicking ConnectAndJoinRandom but adapting to the generalized architecture.
/// </summary>
public abstract class PUN_ConnectionBase : Base_NetworkProvider, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks
{
    [Header("PUN Connection Settings")]
    [Tooltip("Determines how far the auto-connection process should proceed.")]
    public AutoConnectMode AutoConnect = AutoConnectMode.ConnectToRoom; // Default is full connection
    public byte Version = 1;
    public byte MaxPlayers = 4;
    public int playerTTL = -1;

    // Log prefix for easy filtering in the console
    private const string LOG_PREFIX = "<color=cyan>[PUN Base]</color> ";

    // --- Setup Callbacks ---

    protected virtual void Awake()
    {
        // Register this class to listen for Photon callbacks
        PhotonNetwork.AddCallbackTarget(this);
        Debug.Log($"{LOG_PREFIX} Awake: Registered Callback Target.");
    }

    protected virtual void OnDestroy()
    {
        // Unregister callbacks to prevent errors when destroyed
        PhotonNetwork.RemoveCallbackTarget(this);
        Debug.Log($"{LOG_PREFIX} OnDestroy: Removed Callback Target.");
    }

    protected virtual void Start()
    {
        // Check if any auto-connect mode is enabled (not None)
        if (AutoConnect != AutoConnectMode.None)
        {
            Debug.Log($"{LOG_PREFIX} Start: AutoConnect set to {AutoConnect}. Calling Connect()...");
            Connect();
        }
    }

    // --- Implementation of Abstract Base ---

    public override void Connect()
    {
        UpdateState(NetworkConnectionState.Connecting);
        Debug.Log($"{LOG_PREFIX} <color=yellow>Connect() Called</color>: Connecting to Photon Cloud...");

        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = this.Version + "." + "1.0";
    }

    public override void CreateRoom()
    {
        UpdateState(NetworkConnectionState.JoiningRoom);
        Debug.Log("<color=yellow>[PUN Base] CreateRoom Called: Creating a new room...</color>");

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = this.MaxPlayers;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        PhotonNetwork.CreateRoom(null, roomOptions, null);
    }

    public override void JoinLobby()
    {
        Debug.Log("<color=cyan>[PUN Base]</color> JoinLobby Called.");
        PhotonNetwork.JoinLobby();
    }

    public override void JoinRandomRoom()
    {
        Debug.Log("<color=cyan>[PUN Base]</color> JoinRandomRoom Called.");
        PhotonNetwork.JoinRandomRoom();
    }

    // Return ชื่อห้อง หรือ "Unknown" ถ้าเป็น Null
    public override string CurrentRoomName
    {
        get
        {
            return PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.Name : "Unknown";
        }
    }

    public override void Disconnect()
    {
        Debug.Log($"{LOG_PREFIX} <color=orange>Disconnect() Called</color>: Disconnecting...");
        PhotonNetwork.Disconnect();
    }

    
    // --- IConnectionCallbacks ---

    public virtual void OnConnected()
    {
        // Called when low-level connection is established
        Debug.Log($"{LOG_PREFIX} OnConnected: Established low-level connection to Photon Server.");
    }

    public virtual void OnConnectedToMaster()
    {
        UpdateState(NetworkConnectionState.ConnectedToMaster);
        Debug.Log($"{LOG_PREFIX} <color=green>OnConnectedToMaster</color>: Ready! Region: {PhotonNetwork.CloudRegion}.");

        // --- Auto Connect Logic Branching ---
        switch (AutoConnect)
        {
            case AutoConnectMode.ConnectToMaster:
                Debug.Log($"{LOG_PREFIX} AutoConnect: Target reached (Master Server). Waiting for user input.");
                break;

            case AutoConnectMode.ConnectToLobby:
                Debug.Log($"{LOG_PREFIX} AutoConnect: Proceeding to Lobby...");
                PhotonNetwork.JoinLobby();
                break;

            case AutoConnectMode.ConnectToRoom:
                Debug.Log($"{LOG_PREFIX} AutoConnect: Proceeding to Random Room...");
                PhotonNetwork.JoinRandomRoom();
                break;
        }
    }

    public virtual void OnDisconnected(DisconnectCause cause)
    {
        UpdateState(NetworkConnectionState.Offline);

        string color = (cause == DisconnectCause.DisconnectByClientLogic) ? "orange" : "red";
        Debug.Log($"{LOG_PREFIX} <color={color}>OnDisconnected</color>: Reason = {cause}");
    }

    public virtual void OnRegionListReceived(RegionHandler regionHandler)
    {
        Debug.Log($"{LOG_PREFIX} OnRegionListReceived: Found {regionHandler.EnabledRegions.Count} regions.");
    }

    public virtual void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        Debug.Log($"{LOG_PREFIX} OnCustomAuthenticationResponse: {data.ToString()}");
    }

    public virtual void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.LogError($"{LOG_PREFIX} <color=red>OnCustomAuthenticationFailed</color>: {debugMessage}");
    }

    // --- IMatchmakingCallbacks ---

    public virtual void OnJoinedLobby()
    {
        UpdateState(NetworkConnectionState.InLobby);
        Debug.Log($"{LOG_PREFIX} OnJoinedLobby: Entered the Lobby.");

        // Check if we need to proceed further from Lobby to Room
        if (AutoConnect == AutoConnectMode.ConnectToRoom)
        {
            Debug.Log($"{LOG_PREFIX} AutoConnect: Proceeding from Lobby to Random Room...");
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            Debug.Log($"{LOG_PREFIX} AutoConnect: Target reached (Lobby). Waiting for user input.");
        }
    }

    public virtual void OnLeftLobby()
    {
        Debug.Log($"{LOG_PREFIX} OnLeftLobby: Exited the lobby.");
    }

    public virtual void OnCreatedRoom()
    {
        Debug.Log($"{LOG_PREFIX} <color=green>OnCreatedRoom</color>: Room created successfully. Name: {PhotonNetwork.CurrentRoom.Name}");
    }

    public virtual void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"{LOG_PREFIX} <color=red>OnCreateRoomFailed</color>: Code={returnCode}, Msg={message}");
    }

    public virtual void OnJoinRandomFailed(short returnCode, string message)
    {
        // Only attempt to create a room if the intention was to join a room
        if (AutoConnect == AutoConnectMode.ConnectToRoom)
        {
            CreateRoom();
        }
        else
        {
            Debug.LogWarning($"{LOG_PREFIX} OnJoinRandomFailed: {message}");
        }
    }

    public virtual void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"{LOG_PREFIX} <color=red>OnJoinRoomFailed</color>: Code={returnCode}, Msg={message}");
    }

    public virtual void OnJoinedRoom()
    {
        UpdateState(NetworkConnectionState.InRoom);
        Debug.Log($"{LOG_PREFIX} <color=green>OnJoinedRoom</color>: Success! Room: {PhotonNetwork.CurrentRoom.Name}");

        // Call the abstract method to let the child class handle gameplay spawning
        SpawnGameplayObjects();
    }

    public virtual void OnLeftRoom()
    {
        UpdateState(NetworkConnectionState.ConnectedToMaster);
        Debug.Log($"{LOG_PREFIX} OnLeftRoom: Local player left the room.");
    }

    public virtual void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        Debug.Log($"{LOG_PREFIX} OnFriendListUpdate: Updated {friendList.Count} friends.");
    }

    // --- IInRoomCallbacks ---

    public virtual void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{LOG_PREFIX} <color=yellow>OnPlayerEnteredRoom</color>: {newPlayer.NickName} joined.");
    }

    public virtual void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{LOG_PREFIX} <color=orange>OnPlayerLeftRoom</color>: {otherPlayer.NickName} left.");
    }

    public virtual void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log($"{LOG_PREFIX} OnRoomPropertiesUpdate: Props changed.");
    }

    public virtual void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log($"{LOG_PREFIX} OnPlayerPropertiesUpdate: Player {targetPlayer.NickName} props changed.");
    }

    public virtual void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.LogWarning($"{LOG_PREFIX} <color=yellow>OnMasterClientSwitched</color>: New Master is {newMasterClient.NickName}");
    }
}