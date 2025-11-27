using Photon.Pun;
using StarterAssets;
using UnityEngine;
using static Unity.Collections.Unicode;
/// <summary>
/// An ONLINE shooter implementation for PUN2. It handles firing via RPCs.
/// </summary>
[RequireComponent(typeof(FirstPersonController))]
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Shooter))]
[RequireComponent(typeof(PUN_ShooterSelector))]
public class PUN_Shooter : PlayerAction
{
    [SerializeField] private GameObject bulletPrefab;
    protected override void HandleFire(Vector3 position, Quaternion rotation)
    {
        Vector3 spawnPosition = position + (rotation * Vector3.forward * 1.5f);
        // 1. Instantiate the projectile using PhotonNetwork.
        // This client becomes the owner, and PUN automatically tells
        // other players to instantiate their own copies.
        PhotonView netObject = PhotonNetwork.Instantiate(bulletPrefab.name,
        spawnPosition
        , rotation).GetPhotonView();
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
