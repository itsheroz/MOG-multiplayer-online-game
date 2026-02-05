using UnityEngine;
using Photon.Pun;

public class Local_Launch : PUN_ConnectionBase
{
    [Header("Leavel Transition Settings")]
    public string TargetSceneName = "GameScene";
    public override void SpawnGameplayObjects(){

    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        
        if(PhotonNetwork.CurrentRoom.PlayerCount >= 1){
            Debug.Log("We Load the Room 1");
        }
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LoadLevel(TargetSceneName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log("OnJoinRoomFailed: " + message);

        CreateRoom();
    }
}
