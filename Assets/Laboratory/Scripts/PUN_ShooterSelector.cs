using Photon.Pun;
using UnityEngine;
/// <summary>
/// The Photon.Pun-specific implementation of the ShooterSelector.
/// It fills in the "blanks" from the base class with Photon.Pun's API.
/// </summary>
[RequireComponent(typeof(PUN_Shooter))]
public class PUN_ShooterSelector : ShooterSelector
{
    protected override bool IsNetworkActive()
    {
        return PhotonNetwork.IsConnected;
    }
    protected override IShooter GetOnlineShooter()
    {
        return GetComponent<PUN_Shooter>();
    }
}
