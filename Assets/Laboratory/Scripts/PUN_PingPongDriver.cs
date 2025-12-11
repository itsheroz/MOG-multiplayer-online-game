using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
/// The PUN 2 NETWORK ADAPTER (Driver) for Scene Objects.
[RequireComponent(typeof(PingPongMove))]
public class PUN_PingPongDriver : MonoBehaviourPunCallbacks
{
    private PingPongMove _engine;
    private void Awake()
    {
        _engine = GetComponent<PingPongMove>();
        // Start disabled by default to prevent movement before connection.
        _engine.enabled = false;
    }
    /// Check ownership immediately when the script is enabled
    public override void OnEnable()
    {
        base.OnEnable();
        CheckAuthority();
    }
    /// Called when the local player joins a room.
    public override void OnJoinedRoom()
    {
        CheckAuthority();
    }
    /// Called when the current Master Client leaves and a new one is assigned.
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        CheckAuthority();
    }
    /// Centralized logic to determine if we should run the engine.
    private void CheckAuthority()
    {
        // Only run logic if we are actually inside a room
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[PUN_Driver] I am the Master Client. Taking control of Scene Object.");
            _engine.enabled = true;
        }
        else
        {
            _engine.enabled = false;
        }
    }
}