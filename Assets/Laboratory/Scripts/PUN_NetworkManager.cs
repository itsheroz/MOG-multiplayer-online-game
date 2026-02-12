using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the core network connection, room joining logic, and instantiating essential game objects 
/// (Player Characters, Session Service) when using Photon Unity Networking (PUN).
/// It extends ConnectAndJoinRandom for simple connection management.
/// </summary>
public class PUN_NetworkManager : PUN_ConnectionBase
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

    public string TargetSceneName = "GameScene"; // Name of the scene to load


    GameObject serviceObj;
    // Assigns the Singleton instance and performs initial setup.
    protected override void Awake()
    {
        base.Awake();

        singleton = this;

        // Deactivate the Game Timer UI initially to prevent it from running prematurely or causing errors 
        // before the service is connected.
        if (GameTimerCanvas != null)
        {
            GameTimerCanvas.SetActive(false);
        }
    }

    protected override void Start()
    {
        base.Start();

        // Check if: 1. Connected to server (Ready) and 2. In a room (InRoom)
        // To prevent duplicate spawning or spawning while offline.
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            // Explicitly update the internal state to 'InRoom' 
            // because OnJoinedRoom callback won't trigger again after a scene load.
            UpdateState(NetworkConnectionState.InRoom);

            Debug.Log("<color=green>[PUN Manager] Detected active connection in Start. Spawning objects...</color>");
            SpawnGameplayObjects();
        }
        else
        {
            Debug.LogWarning("[PUN Manager] Start called but not in room/connected. Waiting for connection...");
        }
    }

    public override void SpawnGameplayObjects()
    {
        // 1. Instantiate the player character.
        if (GamePlayerPrefab != null && PUN_PlayerNetworkController.LocalPlayerInstance == null)
        {
            float randomX = Random.Range(-2f, 2f);
            PhotonNetwork.Instantiate(GamePlayerPrefab.name, new Vector3(randomX, 5f, 0f), Quaternion.identity, 0);
        }
        else
        {
            Debug.Log("Ignoring scene load for " + SceneManagerHelper.ActiveSceneName);
        }

        // 2. Initialize scene-specific services.
        SetupSceneObjects();

        // 3. Notify listeners that gameplay setup is complete.
        NotifyReadyToSpawn();
    }

    private void SetupSceneObjects()
    {
        // Setup UI or Managers here (e.g., Camera tracking).
        Debug.Log("[PUN Manager] Scene Objects configured.");

        // 2. Game Data Session Service Instantiation
        // Ensure the session service is created only once upon entering the room.
        if (GameDataSessionManager.Instance.Service == null)
        {
            serviceObj = Instantiate(_sessionServicePrefab);

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

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        SceneManager.LoadScene(TargetSceneName);

        Destroy(serviceObj.gameObject);
        Destroy(this.gameObject);
    }
    
}