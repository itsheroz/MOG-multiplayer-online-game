using Photon.Pun;
using StarterAssets;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

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

    void Awake()
    {
        // Get references to the components attached to this GameObject
        _controllerLogic = GetComponent<FirstPersonController>();
        _playerInput = GetComponent<PlayerInput>();
        _assetsInput = GetComponent<StarterAssetsInputs>();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log(info.photonView.Owner.ToString());
        Debug.Log(info.photonView.ViewID.ToString());

        // photonView.IsMine is the PUN2 equivalent of isLocalPlayer or IsOwner
        if (photonView.IsMine)
        {
            // Enable the character controller logic for our local player.
            _controllerLogic.SetControl(true);
            _controllerLogic.enabled = true;
            _playerInput.enabled = true;
            _assetsInput.enabled = true;

            Debug.Log("Start LocalPlayer: Local control enabled.");

            // Set the static follow target for the camera to find.
            LocalPlayerFollowTarget = _controllerLogic.CinemachineCameraTarget.transform;
            SetupCamera();

            // we keep track of the localPlayer instance to prevent instanciation
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
            Debug.Log("Proxy character. Local control disabled.");
        }
    }

    private void SetupCamera()
    {
        // --- Camera Setup for the Local Player
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
            Debug.LogError("Failed! CinemachineCamera not found at the moment of spawn.");
        }
    }
}