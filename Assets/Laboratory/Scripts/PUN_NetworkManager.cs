using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

public class PUN_NetworkManager : ConnectAndJoinRandom 
{
    public static PUN_NetworkManager singleton;

    [Header("Spawn Info")]
    [Tooltip("The prefab to use for representing the player")]
    public GameObject GamePlayerPrefab;

    // Add Awake to assign the Singleton instance.
    public void Awake()
    {
        singleton = this;
    }

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
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            PhotonNetwork.Instantiate(GamePlayerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        }
        else {
            Debug.Log("Ignoring scene load for " + SceneManagerHelper.ActiveSceneName);
        }
    }

    // --- Added Section: Simple GUI for connection ---
    private void OnGUI()
    {
        // Set basic GUI styles for better readability.
        GUI.skin.label.fontSize = 20;
        GUI.skin.button.fontSize = 20;

        // Display the current network connection state at the top-left corner.
        GUILayout.Label("Status: " + PhotonNetwork.NetworkClientState);

        // Show the Connect button only when not connected and not ready.
        if (!PhotonNetwork.IsConnected && !PhotonNetwork.IsConnectedAndReady)
        {
            // Create a button with size 200x50.
            if (GUILayout.Button("Connect Now", GUILayout.Width(200), GUILayout.Height(50)))
            {
                Debug.Log("Connect button clicked.");
                ConnectNow(); // Call the parent class method to initiate connection.
            }
        }
    }
}
