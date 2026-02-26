using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

/// <summary>
/// Handles the core network connection, room joining logic, and instantiating essential game objects 
/// (Player Characters, Session Service) when using Photon Unity Networking (PUN).
/// It extends ConnectAndJoinRandom for simple connection management.
/// </summary>
public class PUN_NetworkManager : ConnectAndJoinRandom
{
    public static PUN_NetworkManager singleton; // Singleton instance for easy global access.

    [Header("Spawn Info")]
    [Tooltip("The prefab to use for representing the player")]
    public GameObject GamePlayerPrefab; // The Photon-instantiated character prefab.

    [Header("Session Service")]
    [SerializeField] private GameObject _sessionServicePrefab; // Prefab containing the PUN_GameDataSessionService component.

    [Header("UI References")]
    [Tooltip("Reference to the UI Canvas/GameObject that holds the GameTimer script.")]
    public GameObject GameTimerCanvas; // UI component that manages the game countdown timer.

    // Assigns the Singleton instance and performs initial setup.
    public void Awake()
    {
        singleton = this;

        // Deactivate the Game Timer UI initially to prevent it from running prematurely or causing errors 
        // before the service is connected.
        if (GameTimerCanvas != null)
        {
            GameTimerCanvas.SetActive(false);
        }
    }

    // Photon Callback: Called when a remote player successfully joins the current room.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("New Player. " + newPlayer.ToString());
    }

    // Photon Callback: Called when the local client successfully joins a room.
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log($"Joined room '{PhotonNetwork.CurrentRoom.Name}'. Spawning player...");

        // 1. Player Character Instantiation
        if (PUN_PlayerNetworkController.LocalPlayerInstance == null)
        {
            Debug.Log("We are Instantiating LocalPlayer from " + SceneManagerHelper.ActiveSceneName);
            // Instantiate the player character over the network for the local client.
            PhotonNetwork.Instantiate(GamePlayerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        }
        else
        {
            Debug.Log("Ignoring scene load for " + SceneManagerHelper.ActiveSceneName);
        }

        // 2. Game Data Session Service Instantiation
        // Ensure the session service is created only once upon entering the room.
        if (GameDataSessionManager.Instance.Service == null)
        {
            GameObject serviceObj = Instantiate(_sessionServicePrefab);

            // Important: Keep the service object alive across scene changes.
            DontDestroyOnLoad(serviceObj);

            Debug.Log("[Game Data Session Manager] Created Session Service.");
        }

        // 3. Timer Activation
        // Activate the Game Timer UI. Its Start() method will then initiate the synchronized timer logic.
        if (GameTimerCanvas != null)
        {
            GameTimerCanvas.SetActive(true);
        }
    }

    // --- Added Section: Simple GUI for connection ---
    private void OnGUI()
    {
        // Set basic GUI styles for better readability.
        GUI.skin.label.fontSize = 20;
        GUI.skin.button.fontSize = 20;

        // Display the current network connection state.
        GUILayout.Label("Status: " + PhotonNetwork.NetworkClientState);

        // Show the Connect button only when disconnected.
        if (!PhotonNetwork.IsConnected && !PhotonNetwork.IsConnectedAndReady)
        {
            if (GUILayout.Button("Connect Now", GUILayout.Width(200), GUILayout.Height(50)))
            {
                Debug.Log("Connect button clicked.");
                ConnectNow(); // Calls the base method to start the connection process (Connect and Join Random).
            }
        }
    }
}