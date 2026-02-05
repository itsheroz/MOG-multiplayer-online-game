using Photon.Pun;
using UnityEngine;

/// <summary>
/// This class handles the network logic for an object that changes color when triggered.
/// It uses PUN2's RPC (Remote Procedure Call) system to synchronize the state.
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class PUN_RPCsNetworkAction : MonoBehaviourPun, IColorChangeInitiator
{
    
    /// <summary>
    /// This public method is called by an external trigger (e.g., a projectile collision)
    /// to initiate the color change process across the network.
    /// </summary>
    
    [Header("Level Transition Settings")]
    public string TargetSceneName = "GameScene";
    
    public void InitiateLevelTransition(){
        if(string.IsNullOrEmpty(TargetSceneName)){
            Debug.LogError("TargetSceneName is not set. Please set the target scene name in the inspector.");
            return;
        }

        photonView.RPC(nameof(RequestLevelLoad), RpcTarget.MasterClient);
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

    [PunRPC]
    private void RequestLevelLoad(PhotonMessageInfo info){
        Debug.Log($"[Server] MasterClient received request to load level:{TargetSceneName}");
        PhotonNetwork.LoadLevel(TargetSceneName);
    }
    public void InitiateColorChange()
    {
        // Send an RPC request to the MasterClient, asking it to orchestrate the color change.
        // Only the MasterClient will execute the 'RequestColorChange' method.
        GetComponent<PhotonView>().RPC(nameof(RequestColorChange), RpcTarget.MasterClient);
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
}