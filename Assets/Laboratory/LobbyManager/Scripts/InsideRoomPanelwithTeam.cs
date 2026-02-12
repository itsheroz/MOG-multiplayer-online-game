using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
public class InsideRoomPanelwithTeam : InsideRoomPanel
{
    public bool isTeamMode = false ;
    public override void OnEnable ()
    {
        SettingRoomTeamMode( PhotonNetwork .CurrentRoom.CustomProperties);
        base .OnEnable();
    }
    private void SettingRoomTeamMode( Hashtable changedProps)
    {
        object isTeamMode;
        if (changedProps.TryGetValue( PunGameSetting .TEAMMODE, out isTeamMode))
        {
            this .isTeamMode = ( bool )isTeamMode;
        }
    }
        protected override void SetupPlayerList( Player p, GameObject instanceEntry)
        {
            base .SetupPlayerList(p, instanceEntry);
            instanceEntry.GetComponent<PlayerListEntrywithTeam>().Initialize(p.ActorNumber,p.NickName, this .isTeamMode);
        }
    public override void SetupPlayerEnteredRoom( Player newPlayer, GameObject instanceEntry)
        {
            base .SetupPlayerEnteredRoom(newPlayer, instanceEntry);
            instanceEntry.GetComponent<PlayerListEntrywithTeam>().Initialize(newPlayer.ActorNumber, newPlayer.NickName,this.isTeamMode);
        }
}