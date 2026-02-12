using Photon.Pun;
using UnityEngine;

/// <summary>
/// This class handles the network logic for an object that changes color when triggered.
/// It uses PUN2's RPC (Remote Procedure Call) system to synchronize the state.
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class PUN_RPCsNetworkAction : MonoBehaviourPun, IColorChangeInitiator
{
    [Header("Level Transition Settings")]
    public string TargetSceneName = "GameScene"; // Name of the scene to load

    /// <summary>
    /// This public method is called by an external trigger (e.g., a projectile collision)
    /// to initiate the color change process across the network.
    /// </summary>
    public void InitiateColorChange()
    {
        // Send an RPC request to the MasterClient, asking it to orchestrate the color change.
        // Only the MasterClient will execute the 'RequestColorChange' method.
        GetComponent<PhotonView>().RPC(nameof(RequestColorChange), RpcTarget.MasterClient);
    }

    /// <summary>
    /// This RPC is executed ONLY on the MasterClient.
    /// It receives the request, generates a new color, and broadcasts it to everyone.
    /// </summary>
    [PunRPC]
    private void RequestColorChange(PhotonMessageInfo info)
    {
        // This code executes only on the MasterClient.
        Color newColor = new Color(Random.value, Random.value, Random.value);

        // The MasterClient broadcasts the new color to all clients (including itself).
        GetComponent<PhotonView>().RPC(nameof(RpcSyncColor), RpcTarget.All, newColor.r, newColor.g, newColor.b);
    }

    /// <summary>
    /// This RPC is executed on ALL clients to apply the new color.
    /// </summary>
    [PunRPC]
    private void RpcSyncColor(float r, float g, float b)
    {
        // This code executes on every client in the room.
        Color newColor = new Color(r, g, b);
        GetComponent<Renderer>().material.color = newColor;
    }

    /// <summary>
    /// Call this method (e.g., from a Button or Trigger) to start the scene change.
    /// </summary>
    public void InitiateLevelTransition()
    {
        // Safety check
        if (string.IsNullOrEmpty(TargetSceneName))
        {
            Debug.LogError("Target Scene Name is empty!");
            return;
        }

        // Send RPC to MasterClient because only Master should control scene loading
        photonView.RPC(nameof(RequestLevelLoad), RpcTarget.MasterClient);
    }

    /// <summary>
    /// This RPC executes ONLY on the MasterClient.
    /// </summary>
    [PunRPC]
    private void RequestLevelLoad(PhotonMessageInfo info)
    {
        Debug.Log($"[Server] MasterClient received request to load level: {TargetSceneName}");

        // Critical: In Photon, we use LoadLevel, not SceneManager.LoadScene
        // If 'PhotonNetwork.AutomaticallySyncScene' is true, all clients will follow automatically.
        PhotonNetwork.LoadLevel(TargetSceneName);
    }
}