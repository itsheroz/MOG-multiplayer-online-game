using Photon.Pun;
using UnityEngine;
/// <summary>
/// This is the NETWORK ADAPTER for the Projectile.
/// behavior in a PUN2 online environment.
/// </summary>
[RequireComponent(typeof(Projectile))]
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
        if (!photonView.IsMine)
        {
            // By disabling the Projectile script
            _projectileLogic.enabled = false;
        }
    }
}