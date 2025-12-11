using Photon.Pun;
using UnityEngine;
/// This is the PUN2 NETWORK ADAPTER (Composition Pattern).
[RequireComponent(typeof(PlayerStamina))]
[RequireComponent(typeof(PhotonView))]
public class PUN_PlayerStamina : MonoBehaviourPun, IPunObservable
{
    // The "Engine" we are controlling
    private PlayerStamina _playerStamina;
    private void Awake()
    {
        _playerStamina = GetComponent<PlayerStamina>();
        // If we are NOT the owner, disable the core logic component.
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
            GUI.Label(new Rect(0, 20, 300, 50), "Player Stamina: " +
            (int)_playerStamina.CurrentStamina);
        }
    }
    /// This is PUN2's serialization method.
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Read the stamina from our core engine and send it.
            stream.SendNext(_playerStamina.CurrentStamina);
        }
        else
        {
            // We are a remote client. Receive the stamina data.
            float receivedStamina = (float)stream.ReceiveNext();
            // Call the public setter on our core engine.
            _playerStamina.SetStamina(receivedStamina);
        }
    }
}