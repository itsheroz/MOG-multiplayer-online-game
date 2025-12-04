using Photon.Pun;
using StarterAssets;
using UnityEngine;
/// An ONLINE shooter implementation for PUN2. It handles firing via RPCs.
[RequireComponent(typeof(FirstPersonController))]
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Healer))]
public class PUN_Healer : MonoBehaviourPun, IHealer
{
    [SerializeField] private GameObject healBoxPrefab;
    private Camera _playerCamera;
    private void Awake()
    {
        // This component is self-sufficient and finds its own camera reference.
        _playerCamera = GetComponent<FirstPersonController>().MainCamera;
    }
    public void LaunchHeal()
    {
        if (_playerCamera == null) return;
        LocalLaunch(_playerCamera.transform.position, _playerCamera.transform.rotation);
    }
    private void LocalLaunch(Vector3 position, Quaternion rotation)
    {
        Vector3 spawnPosition = position + (rotation * Vector3.forward * 1.5f);
        // 1. Instantiate the projectile using PhotonNetwork.
        PhotonView netObject = PhotonNetwork.Instantiate(healBoxPrefab.name,
        spawnPosition, rotation).GetPhotonView();
        // 2. Inject the network destruction logic into the core projectile.
        netObject.GetComponent<Projectile>().SetupHitAction(() =>
        {
            // This action is called by Projectile.OnTriggerEnter on all clients.
            // Authority Check: Only the owner (the client who fired)
            // is allowed to destroy the projectile over the network.
            if (netObject.IsMine && PhotonNetwork.IsConnected)
            {
                // Use PhotonNetwork.Destroy to remove the object from all clients.
                // We add extra checks to prevent errors if the object
                // was already destroyed by a race condition.
                if (netObject != null && PhotonNetwork.InRoom)
                {
                    if (PhotonNetwork.GetPhotonView(netObject.ViewID) != null)
                        PhotonNetwork.Destroy(netObject);
                    else Debug.Log("Too fast to Destroy on Network");
                }
            }
        });
    }
}