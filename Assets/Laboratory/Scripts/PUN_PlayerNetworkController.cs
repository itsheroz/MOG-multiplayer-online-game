using Photon.Pun;
using Photon.Realtime;
using StarterAssets;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the INITIAL SETUP of the player object when it spawns using PUN2.
/// Its single responsibility is to enable/disable the correct components
/// for the owner (IsMine) versus remote proxies.
/// </summary>
[RequireComponent(typeof(FirstPersonController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(StarterAssetsInputs))]
[RequireComponent(typeof(PhotonView))]
public class PUN_PlayerNetworkController : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    // A static reference to the local player's camera follow target. Accessible from anywhere.
    public static Transform LocalPlayerFollowTarget { get; private set; }

    // Cached reference to the character's movement logic.
    private FirstPersonController _controllerLogic;
    private PlayerInput _playerInput;
    private StarterAssetsInputs _assetsInput;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    //team mesh
    public MeshRenderer _teamReander;

    /// <summary>
    /// Add reference UI playerInfo
    /// </summary>
    private UIPlayerInfoManager _playerInfoManager;
    void Awake()
    {
        _controllerLogic = GetComponent<FirstPersonController>();
        _playerInput = GetComponent<PlayerInput>();
        _assetsInput = GetComponent<StarterAssetsInputs>();

        /// Get Component In Children 
        _playerInfoManager = GetComponentInChildren<UIPlayerInfoManager>();

        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Subscribe to the scene loaded event.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent errors or memory leaks.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[System] Moved to new scene: {scene.name}");

        // Perform re-setup here instead of Start().
        SetupCamera();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log(info.photonView.Owner.ToString());
        Debug.Log(info.photonView.ViewID.ToString());
        Debug.Log("PUN instance");
        // photonView.IsMine is the PUN2 equivalent of isLocalPlayer or IsOwner
        if (photonView.IsMine)
        {
            // Enable the character controller logic for our local player.
            _controllerLogic.SetControl(true);
            _controllerLogic.enabled = true;
            _playerInput.enabled = true;
            _assetsInput.enabled = true;
            
            // Set the static follow target for the camera to find.
            LocalPlayerFollowTarget = _controllerLogic.CinemachineCameraTarget.transform;
            SetupCamera();
            Debug.Log("success");

            // Setup LocalUI
            _playerInfoManager.SetLocalUI();

            // : we keep track of the localPlayer instance to prevent instanciation
            // when levels are synchronized
            LocalPlayerInstance = gameObject;
        }
        else
        {
            // This is a remote player (proxy). Disable the local character controller logic.
            _controllerLogic.SetControl(false);
            _controllerLogic.enabled = false;
            _playerInput.enabled = false;
            _assetsInput.enabled = false;

            //setting mesh team
            if (_teamReander != null)
                SettingPlayerTeam(info.Sender);

            Debug.Log("Proxy character. Local control disabled.");
        }
    }


    private void SetupCamera()
    {
        // --- Camera Setup for the Local Player ---
        // Attempt to find the Cinemachine Virtual Camera in the scene.
        var virtualCamera = FindFirstObjectByType<CinemachineCamera>();
        if (virtualCamera != null)
        {
            // If found, assign our follow target to it.
            virtualCamera.Follow = LocalPlayerFollowTarget;
            Debug.Log("Success! Camera found and set immediately in Spawned().");
        }
        else
        {
            // The risk: This will fail if the camera isn't ready when the player spawns.
            Debug.LogError("Failed! CinemachineCamera not found at the moment of spawn. This is a race condition.");
        }
    }
    //lobby team mesh
    private void SettingPlayerTeam(Player Sender)
    {
        Photon.Pun.UtilityScripts.PhotonTeam _currentTeam = Photon.Pun.UtilityScripts.PhotonTeamExtensions.GetPhotonTeam(Sender);
        if(_currentTeam != null)
        {
            int colors = (int)_currentTeam.Code;
            _teamReander.material.color = PunGameSetting.GetColor(colors);
        }
    }
}
