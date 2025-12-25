using Photon.Pun;
using UnityEngine;

// Requires a PhotonView for networking context.
[RequireComponent(typeof(PhotonView))]
// Links the abstract PlayerColorController logic to the Photon network environment.
public class PUN_PlayerColorController : PlayerColorController
{
    private PhotonView _pv; // Cached PhotonView component.

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
    }

    // --- PUN Implementation ---

    // True if this object belongs to the local client (linked to _pv.IsMine).
    public override bool IsMine
    {
        get { return _pv != null && _pv.IsMine; }
    }

    // The unique network ID (ActorNumber) of the object's owner.
    public override int OwnerID
    {
        get { return _pv != null && _pv.Owner != null ? _pv.Owner.ActorNumber : -1; }
    }
}