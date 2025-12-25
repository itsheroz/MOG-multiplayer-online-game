using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
public class Local_PlayerColorController : PlayerColorController
{
    // In offline/local mode, this instance represents the local player.
    public override bool IsMine => true;
    // Assigns a fixed ID since no external network ID is required offline.
    public override int OwnerID => 1;
}