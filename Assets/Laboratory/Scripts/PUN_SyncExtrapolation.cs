using UnityEngine;
using Photon.Pun;
/// Extrapolation (Prediction). Estimates the future position based on process velocity
[RequireComponent(typeof(PhotonView))]
public class PUN_SyncExtrapolation : MonoBehaviourPun, IPunObservable
{
    [Range(0.0f, 2.0f)]
    public float Factor = 0.98f; // Factor to dampen overshoot.
    private Vector3 latestCorrectPos;
    private Vector3 movementVector;
    private Vector3 errorVector;
    private double lastTime;
    public void Awake()
    {
        latestCorrectPos = transform.position;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Master Client: Send position only (velocity is calculated on client).
            stream.SendNext(transform.position);
        }
        else
        {
            // Remote Client: Calculate velocity and error.
            Vector3 updatedLocalPos = (Vector3)stream.ReceiveNext();
            // Convert to seconds for accurate velocity calculation.
            double currentTime = info.SentServerTimestamp / 1000.0;
            double timeDiff = currentTime - lastTime;
            lastTime = currentTime;
            if (timeDiff <= 0.001) return;
            // Calculate estimated velocity.
            movementVector = (updatedLocalPos - latestCorrectPos) / (float)timeDiff;
            // Calculate error between actual client pos and network pos.
            errorVector = (updatedLocalPos - transform.position) / (float)timeDiff;
            latestCorrectPos = updatedLocalPos;
        }
    }
    public void Update()
    {
        // Master Client controls logic directly.
        if (PhotonNetwork.IsMasterClient) return;
        // Apply extrapolated movement + error correction over time.
        transform.position += (movementVector + errorVector) * Factor * Time.deltaTime;
    }
}
