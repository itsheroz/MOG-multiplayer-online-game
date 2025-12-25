using Photon.Pun;
using UnityEngine;

/// <summary>
/// This is the PUN2 NETWORK ADAPTER (Composition Pattern).
/// It disables the core PlayerStamina logic on remote clients
/// and handles all network serialization.
/// </summary>
[RequireComponent(typeof(PlayerStamina))]
[RequireComponent(typeof(PhotonView))]
public class PUN_PlayerStamina : MonoBehaviourPun, IPunObservable
{
    // The "Engine" we are controlling
    private PlayerStamina _playerStamina;

    private void Awake()
    {
        _playerStamina = GetComponent<PlayerStamina>();

        // This is the key:
        // If we are NOT the owner, disable the core logic component.
        // The core Update() logic will only run on the owner's machine.
        if (!photonView.IsMine)
        {
            _playerStamina.enabled = false;
        }
    }

    // Debug display for the local owner
    public void OnGUI()
    {
        if (photonView.IsMine)
        {
            // Read the value from the core component for display
            GUI.Label(new Rect(0, 20, 300, 50), "Player Stamina: " + (int)_playerStamina.CurrentStamina);
        }
    }

    /// <summary>
    /// This is PUN2's serialization method.
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We are the owner (IsMine). Read the stamina from our core engine and send it.
            stream.SendNext(_playerStamina.CurrentStamina);
        }
        else // stream.IsReading
        {
            // We are a remote client. Receive the stamina data.
            float receivedStamina = (float)stream.ReceiveNext();

            // Call the public setter on our core engine.
            // This will update the value AND apply the sprint speed effects on remote clients.
            _playerStamina.SetStamina(receivedStamina);
        }
    }
}