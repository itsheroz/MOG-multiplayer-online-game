using Photon.Pun;
using Photon.Realtime; // Required for the Player parameter in callbacks
using UnityEngine;

/// <summary>
/// The PUN 2 NETWORK ADAPTER (Driver) for Scene Objects.
/// It listens to network events to correctly enable the "Engine" 
/// when the room is joined or when Master Client changes.
/// </summary>
[RequireComponent(typeof(PingPongMove))]
public class PUN_PingPongDriver : MonoBehaviourPunCallbacks // ✅ Inherit from Callbacks
{
    private PingPongMove _engine;

    private void Awake() {
        _engine = GetComponent<PingPongMove>();

        // Start disabled by default to prevent movement before connection.
        _engine.enabled = false;
    }

    /// <summary>
    /// Check ownership immediately when the script is enabled, 
    /// in case we are already in a room (e.g., Scene reloaded via PhotonNetwork.LoadLevel).
    /// </summary>
    public override void OnEnable() {
        base.OnEnable();
        CheckAuthority();
    }

    /// <summary>
    /// Called when the local player joins a room.
    /// This is the fix for your issue: waiting until connection is established.
    /// </summary>
    public override void OnJoinedRoom() {
        CheckAuthority();
    }

    /// <summary>
    /// Called when the current Master Client leaves and a new one is assigned.
    /// If we become the new Master Client, we must take over control.
    /// </summary>
    public override void OnMasterClientSwitched(Player newMasterClient) {
        CheckAuthority();
    }

    /// <summary>
    /// Centralized logic to determine if we should run the engine.
    /// </summary>
    private void CheckAuthority() {
        // Only run logic if we are actually inside a room
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient) {
            Debug.Log("[PUN_Driver] I am the Master Client. Taking control of Scene Object.");
            _engine.enabled = true;
        } else {
            _engine.enabled = false;
        }
    }
}