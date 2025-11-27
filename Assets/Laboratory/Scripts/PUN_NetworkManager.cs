using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
public class PUN_NetworkManager : ConnectAndJoinRandom {
    public static PUN_NetworkManager singleton;
    [Header("Spawn Info")]
    [Tooltip("The prefab to use for representing the player")]
    public GameObject GamePlayerPrefab;
    public override void OnPlayerEnteredRoom(Player newPlayer) {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("New Player. " + newPlayer.ToString());
    }
    public override void OnJoinedRoom() {
        base.OnJoinedRoom();
        // When we join a room, we are ready to spawn our character.
        Debug.Log($"Joined room '{PhotonNetwork.CurrentRoom.Name}'. Spawning player...");
        if (PUN_PlayerNetworkController.LocalPlayerInstance == null) {
            Debug.Log("We are Instantiating LocalPlayer from " + SceneManagerHelper.ActiveSceneName);
        // we're in a room. spawn a character for the local player.
        PhotonNetwork.Instantiate(GamePlayerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        }
        else {
            Debug.Log("Ignoring scene load for " + SceneManagerHelper.ActiveSceneName);
        }
    }
}
