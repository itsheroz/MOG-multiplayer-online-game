using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Implements the IGameDataSessionService interface using **Photon's Custom Properties** /// (Room and Player Properties) for data synchronization across the network.
/// This service acts as the central network communication layer for session data.
/// </summary>
public class PUN_GameDataSessionService : MonoBehaviourPunCallbacks, IGameDataSessionService
{
    // --- Events & State ---
    // Event fired when another player (or the local player) changes a property value.
    public event Action<int, string, object> OnPlayerSessionDataChanged;

    // Event fired when the Master Client changes a shared room property.
    public event Action<string, object> OnRoomSessionDataChanged;

    // Event fired once the service is connected to a room and ready to access data.
    public event Action OnDataReady;

    public bool IsDataReady { get; private set; } = false; // Flag indicating if the client is in a room and data can be accessed.
    public bool IsSessionMaster => PhotonNetwork.IsMasterClient; // Check if the local client is the Master Client (Host).
    public double GetSessionTime() => PhotonNetwork.Time; // Returns the synchronized network time, useful for timers.

    // -----------------------------------------------------------------------
    // ✅ 1. Lifecycle & Registration
    // -----------------------------------------------------------------------
    private void Awake()
    {
        // Registers this service implementation with the central manager.
        if (GameDataSessionManager.Instance != null)
            GameDataSessionManager.Instance.RegisterService(this);
    }

    public override void OnEnable() => CheckReady(); // Check readiness status upon enabling.

    // -----------------------------------------------------------------------
    // ✅ 2. Network Lifecycle (Host & Client Handling)
    // -----------------------------------------------------------------------
    // Called when the local client successfully joins a Photon Room.
    public override void OnJoinedRoom() => CheckReady();

    // Sets the service ready state and fires the OnDataReady event for listeners.
    private void CheckReady()
    {
        if (PhotonNetwork.InRoom && !IsDataReady)
        {
            IsDataReady = true;
            OnDataReady?.Invoke();
        }
    }

    // -----------------------------------------------------------------------
    // ✅ 3. Getters (Reading Data)
    // -----------------------------------------------------------------------
    // Retrieves a shared room property from Photon's room cache.
    public T GetRoomProperty<T>(string key, T defaultValue)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out object val))
            return (T)val;
        return defaultValue;
    }

    // Retrieves a specific player's custom property using their ActorID.
    public T GetPlayerProperty<T>(int playerId, string key, T defaultValue)
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            // Find the player object in the room using their ActorNumber (ID).
            Player p = PhotonNetwork.CurrentRoom.GetPlayer(playerId);
            if (p != null && p.CustomProperties.TryGetValue(key, out object val)) return (T)val;
        }
        return defaultValue;
    }

    // -----------------------------------------------------------------------
    // ✅ 4. Setters (Writing Data)
    // -----------------------------------------------------------------------
    // Attempts to set a property visible to all clients in the room. 
    // Generally, only the Master Client should call this to ensure consistency.
    public void SetRoomProperty(string key, object value)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Create a Hashtable and apply changes to the room properties.
            Hashtable props = new Hashtable { { key, value } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            Debug.Log("[GDSS] call SetRoomProperty | key " + key + " value " + value);
        }
    }

    // Sets a property specifically for the local player. This is automatically synchronized to others.
    public void SetPlayerProperty(int playerId, string key, object value)
    {
        // Safety check: Only allow the local client to set its own properties.
        if (playerId == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            // Create a Hashtable and apply changes to the local player's properties.
            Hashtable props = new Hashtable { { key, value } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            Debug.Log("[GDSS] call SetPlayerProperty | key " + key + " value " + value);
        }
    }

    // -----------------------------------------------------------------------
    // ✅ 5. MonoBehaviourPun Callbacks (Data Change Events)
    // -----------------------------------------------------------------------
    // Photon callback: Fired when any player's Custom Properties are updated.
    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(target, changedProps);

        Debug.Log("[GDSS] call OnPlayerPropertiesUpdate.");
        foreach (var key in changedProps.Keys)
        {
            if (key is string stringKey) // Ensures the key is a string before using it.
            {
                // Relay the change event to other game systems.
                OnPlayerSessionDataChanged?.Invoke(target.ActorNumber, stringKey, changedProps[key]);
            }
        }
    }

    // Photon callback: Fired when the Room Custom Properties are updated (usually by the Master Client).
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        foreach (var key in propertiesThatChanged.Keys)
        {
            // Relay the change event to other game systems.
            OnRoomSessionDataChanged?.Invoke((string)key, propertiesThatChanged[key]);
        }
    }
}