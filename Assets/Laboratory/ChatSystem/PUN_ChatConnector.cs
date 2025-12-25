using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
public class PUN_ChatConnector : MonoBehaviourPun, IOnEventCallback
{
    // Helper to check network status
    protected bool IsNetworkActive()
    {
        return PhotonNetwork.IsConnected && PhotonNetwork.InRoom;
    }
    // Define a unique Event Code (Must avoid conflict with other events like AirDrop)
    private const byte CHAT_EVENT_CODE = 10;
    private void Start()
    {
        // 1. Connect to UI Manager events
        if (ChatUIManager.Instance != null)
            ChatUIManager.Instance.OnRequestSendMessage += HandleUISendRequest;
    }
    private void OnDestroy()
    {
        // Unsubscribe from UI events to prevent memory leaks
        if (ChatUIManager.Instance != null)
            ChatUIManager.Instance.OnRequestSendMessage -= HandleUISendRequest;
    }
    public void OnEnable()
    {
        // Register this script to listen for Photon Network Events
        PhotonNetwork.AddCallbackTarget(this);
    }
    public void OnDisable()
    {
        // Unregister to stop listening
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    // --- 1. Outgoing: Receive request from UI -> Send via Photon RaiseEvent ---
    private void HandleUISendRequest(string message, int targetDropdownIndex)
    {
        if (!IsNetworkActive())
        {
            Debug.LogWarning("Sending in OFFLINE Mode");
            // Loopback to UI immediately (Simulate local chat)
            ChatUIManager.Instance.ReceiveMessage("Me(Offline)", message, "#FFFF00");
            return;
        }
        // Map Dropdown Index to Photon ReceiverGroup
        ReceiverGroup targetGroup = ReceiverGroup.All;
        switch (targetDropdownIndex)
        {
            case 0: targetGroup = ReceiverGroup.All; break;
            case 1: targetGroup = ReceiverGroup.MasterClient; break;
            case 2: targetGroup = ReceiverGroup.Others; break;
        }
        // Pack data (Sender ID is automatically handled by Photon, so we just send the message)
        object[] content = new object[] { message };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = targetGroup };
        SendOptions sendOptions = SendOptions.SendReliable;
        // Fire the event!
        PhotonNetwork.RaiseEvent(CHAT_EVENT_CODE, content, options, sendOptions);
    }
    // --- 2. Incoming: Receive from Photon -> Send to UI ---
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == CHAT_EVENT_CODE)
        {
            // Unpack data
            object[] data = (object[])photonEvent.CustomData;
            string msg = (string)data[0];
            // Resolve Sender Name
            int senderID = photonEvent.Sender;
            string senderName = "Unknown";
            // Lookup nickname from Room's Player List
            Player senderPlayer = PhotonNetwork.CurrentRoom.GetPlayer(senderID);
            if (senderPlayer != null)
            {
                senderName = string.IsNullOrEmpty(senderPlayer.NickName) ? $"Player {senderID}" :
                senderPlayer.NickName;
            }
            // Determine Text Color: Green for Self, White for Others
            string color = (senderPlayer.IsLocal) ? "#00FF00" : "#FFFFFF";
            // Forward to UI Manager for display
            ChatUIManager.Instance.ReceiveMessage(senderName, msg, color);
        }
    }
}
