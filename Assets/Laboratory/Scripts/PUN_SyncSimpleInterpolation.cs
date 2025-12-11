using UnityEngine;
using Photon.Pun;
/// Basic interpolation. Linearly interpolates towards the latest received net position.
[RequireComponent(typeof(PhotonView))]
public class PUN_SyncSimpleInterpolation : MonoBehaviourPun, IPunObservable
{
    private Vector3 _networkPosition;
    private Quaternion _networkRotation;
    [Tooltip("Teleport immediately if distance exceeds this value.")]
    public float TeleportIfDistanceGreaterThan = 5.0f;
    [Tooltip("How fast to lerp towards the target.")]
    public float LerpSpeed = 10.0f;
    public void Awake()
    {
        _networkPosition = transform.position;
        _networkRotation = transform.rotation;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Master Client: Send current pos/rot.
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // Remote Client: Receive target pos/rot.
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();
            // Snap immediately if the lag/distance is too large.
            if (Vector3.Distance(transform.position, _networkPosition) >
            TeleportIfDistanceGreaterThan)
            {
                transform.position = _networkPosition;
            }
        }
    }
    public void Update()
    {
        // Master Client controls the logic directly; no interpolation needed.
        if (PhotonNetwork.IsMasterClient) return;
        // Smoothly move towards the network position.
        transform.position = Vector3.Lerp(transform.position, _networkPosition,
        Time.deltaTime * LerpSpeed);
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
        _networkRotation, Time.deltaTime * LerpSpeed * 100);
    }
}
