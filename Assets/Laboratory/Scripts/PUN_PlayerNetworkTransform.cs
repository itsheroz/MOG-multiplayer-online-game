using Photon.Pun;
using StarterAssets;
using UnityEngine;

/// <summary>
/// Handles the RUNTIME SYNCHRONIZATION of the character's transform using PUN2.
/// Its single responsibility is to serialize/deserialize state via OnPhotonSerializeView.
/// </summary>
[RequireComponent(typeof(FirstPersonController))]
public class PUN_PlayerNetworkTransform : MonoBehaviourPun, IPunObservable
{
    // Cached reference to the character's movement logic to get camera pitch.
    private FirstPersonController _controllerLogic;

    // --- Variables to store network state for proxies ---
    private Vector3 _networkPosition;
    private Quaternion _networkRotation;
    private float _networkCameraPitch;

    private void Awake()
    {
        // This script also needs a reference to the controller to get camera data.
        _controllerLogic = GetComponent<FirstPersonController>();
    }

    /// <summary>
    /// This is the heart of PUN2's state synchronization.
    /// It's called automatically by the PhotonView to send and receive data.
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // --- We are the owner of this object: SEND data ---
            // This code runs on the local player's machine.
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(_controllerLogic.GetCameraPitch());
        }
        else
        {
            // --- We are a proxy: RECEIVE data ---
            // This code runs on remote players' machines.
            // We receive the data in the same order we sent it.
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();
            _networkCameraPitch = (float)stream.ReceiveNext();
        }
    }

    void LateUpdate() {

        // This is where we read the network state for visual representation.
        if (!photonView.IsMine)
        {
            InterpolateMovement();
        }
    }

    /// <summary>
    /// Handles the smooth interpolation for remote player objects.
    /// </summary>
    private void InterpolateMovement()
    {
        if (_controllerLogic == null) return;

        float interpolationSpeed = 20f; // Tweak for smoothness

        // Move the proxy character towards the synced position.
        transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * interpolationSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation, Time.deltaTime * interpolationSpeed);

        // Update the proxy's camera pitch.
        _controllerLogic.SetCameraPitch(_networkCameraPitch);
    }
}
