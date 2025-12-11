using Photon.Pun;
using UnityEngine;

/// <summary>
/// This is the PUN2 NETWORK ADAPTER (Composition Pattern).
/// It inherits from MonoBehaviourPun to get network callbacks
/// and CONTROLS the PlayerHealth component sitting next to it.
/// </summary>
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PlayerHealth))]
public class PUN_PlayerHealth : MonoBehaviourPun, IPunObservable {
    private PlayerHealth _playerHealth; 

    private void Awake() {
        // Get the components we need to control
        _playerHealth = GetComponent<PlayerHealth>();
    }

    /// <summary>
    /// This RPC is called by the projectile's owner and executed on
    /// the machine of the player who owns THIS health component.
    /// </summary>
    [PunRPC]
    public void RpcTakeDamage(int amount) {
        if (photonView.IsMine) {
            Debug.Log("Take Damage : " + amount);
            _playerHealth.TakeDamage(amount);
        }
    }

    [PunRPC]
    public void RpcReceiveHeal(int amount) {
        if (photonView.IsMine) {
            Debug.Log("Receive Heal : " + amount);
            _playerHealth.ReceiveHeal(amount);
        }
    }

    /// <summary>
    /// This is PUN2's serialization method. It now works because
    /// this class inherits from MonoBehaviourPun.
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // We are the owner. Read health from our core component and send it.
            stream.SendNext(_playerHealth.CurrentHealth);
        }
        else {
            // We are a remote client. Receive the health data.
            int receivedHealth = (int)stream.ReceiveNext();

            // Set the health on our core component.
            // The core component's SetHealth method will handle the OnHealthChanged event.
            _playerHealth.SetHealth(receivedHealth);
        }
    }
}