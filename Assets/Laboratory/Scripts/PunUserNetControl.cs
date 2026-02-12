using UnityEngine;
using StarterAssets;
using Photon.Pun;
using UnityEngine.InputSystem;
using ExitGames.Client.Photon;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PhotonTransformView))]
public class PunUserNetControl : MonoBehaviourPunCallbacks , IPunInstantiateMagicCallback {
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    public Transform CameraRoot;

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        Debug.Log(info.photonView.Owner.ToString());
        Debug.Log(info.photonView.ViewID.ToString());

        // #Important
        // used in PunNetworkManager.cs
        // : we keep track of the localPlayer instance to prevent instanciation
        // when levels are synchronized
        if (photonView.IsMine) {
            LocalPlayerInstance = gameObject;
            GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
            
            // Reference Camera on run-time (with null checks to avoid crashes)
            if (PunNetworkManager.singleton != null && 
                PunNetworkManager.singleton._vCam != null && 
                CameraRoot != null)
            {
                PunNetworkManager.singleton._vCam.Follow = CameraRoot;
            }
            else
            {
                Debug.LogWarning($"{nameof(PunUserNetControl)}: Cannot set camera follow. " +
                                 $"singleton: {PunNetworkManager.singleton != null}, " +
                                 $"vCam: {(PunNetworkManager.singleton != null && PunNetworkManager.singleton._vCam != null)}, " +
                                 $"CameraRoot: {CameraRoot != null}");
            }

            // Reference Input on run-time
            PlayerInput _pInput = GetComponent<PlayerInput>();
            if (PunNetworkManager.singleton != null && PunNetworkManager.singleton._inputActions != null)
            {
                _pInput.actions = PunNetworkManager.singleton._inputActions;
            }
            else
            {
                Debug.LogWarning($"{nameof(PunUserNetControl)}: Cannot assign input actions, PunNetworkManager or _inputActions is null.");
            }
        }
        else {
            GetComponent<FirstPersonController>().enabled = false;
            OnPlayerPropertiesUpdate(photonView.Owner, photonView.Owner.CustomProperties);
        }

        UIPlayerInfoManager uiManager = GetComponentInChildren<UIPlayerInfoManager>();
        if (uiManager != null)
        {
            uiManager.SetNickName(info.Sender.NickName);
        }
        else
        {
            Debug.LogWarning($"{nameof(PunUserNetControl)}: UIPlayerInfoManager not found in children of {gameObject.name}. Nickname will not be shown.");
        }
    }

    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(target, changedProps);
        if (changedProps.ContainsKey(PunGameSetting.PLAYER_COLOR) &&
            target.ActorNumber == photonView.ControllerActorNr)
        {
            object colors;
            if (changedProps.TryGetValue(PunGameSetting.PLAYER_COLOR, out colors))
            {
                GetComponentInChildren<MeshRenderer>().material.color = PunGameSetting.GetColor((int)colors);
            }
            return;
        }
    }

}
