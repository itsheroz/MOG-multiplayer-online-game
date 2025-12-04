using Photon.Pun;
using UnityEngine;

/// <summary>
/// This is the NETWORK ADAPTER for the Projectile. It manages the Projectile's
/// behavior in a PUN2 online environment.
/// </summary>
[RequireComponent(typeof(Projectile))] // Ensures we always have the core logic script
[RequireComponent(typeof(PhotonView))]
public class PUN_Projectile : MonoBehaviourPun
{
    // A reference to the core projectile logic script.
    private Projectile _projectileLogic;

    private void Awake()
    {
        _projectileLogic = GetComponent<Projectile>();
    }

    private void Start()
    {
        // --- This is the most important logic ---
        // If this projectile is NOT ours (it's a proxy for another player's projectile),
        // we must disable the core logic script.
        if (!photonView.IsMine)
        {
            // By disabling the Projectile script, we prevent it from applying force
            // or processing collisions on non-owner clients. Its movement will be
            // handled entirely by the PhotonRigidbodyView.
            _projectileLogic.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (!photonView.IsMine)
            return;
        // Start listening for the OnMagicBoxHit event from the core projectile logic.
        _projectileLogic.OnMagicBoxHit += HandleMagicBoxHit;
        _projectileLogic.OnPlayerHit += HandlePlayerHit;
    }
    private void OnDisable()
    {
        if (!photonView.IsMine)
            return;
        // Stop listening for the event to avoid errors and memory leaks.
        _projectileLogic.OnMagicBoxHit -= HandleMagicBoxHit;
        _projectileLogic.OnPlayerHit -= HandlePlayerHit;
    }
    private void HandleMagicBoxHit(GameObject boxObject)
    {
        // Authority Check: When the event is received, only the owner of the projectile
        // should be allowed to initiate a network action.
        if (photonView.IsMine)
        {
            // If we are the owner, find the network action script on the box.
            PUN_RPCsNetworkAction magicBox = boxObject.GetComponent<PUN_RPCsNetworkAction>();
            if (magicBox != null)
            {
                // Tell the box to start its color change process across the network.
                magicBox.InitiateColorChange();
            }
        }
    }
    /// This method is now the "Smart Delivery Driver".
    /// It checks the effectValue to decide which RPC to call.
    private void HandlePlayerHit(GameObject playerObject, int effectValue)
    {
        if (!photonView.IsMine) return;
        PhotonView targetPhotonView = playerObject.GetComponentInParent<PhotonView>();
        if (targetPhotonView == null) return;
        if (effectValue < 0)
        {
            // Negative value means DAMAGE
            targetPhotonView.RPC(nameof(PUN_PlayerHealth.RpcTakeDamage),
            targetPhotonView.Owner, effectValue);
        }
        else if (effectValue > 0)
        {
            // Positive value means HEALING
            targetPhotonView.RPC(nameof(PUN_PlayerHealth.RpcReceiveHeal),
            targetPhotonView.Owner, effectValue);
        }
    }
}