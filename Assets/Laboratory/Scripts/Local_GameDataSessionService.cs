using System;
using System.Collections.Generic;
using UnityEngine;
/// This class simulates network data storage and retrieval using in-memory Dictionaries.
public class Local_GameDataSessionService : IGameDataSessionService
{
    // Data is instantly ready since there's no network synchronization delay.
    public bool IsDataReady { get; private set; } = true;
    // In local mode, the running instance is always the master/host.
    public bool IsSessionMaster => true;
    // Events are triggered immediately upon data modification (no network lag).
    public event Action OnDataReady;
    public event Action<int, string, object> OnPlayerSessionDataChanged;
    public event Action<string, object> OnRoomSessionDataChanged;
    // --- Simulated Data Storage ---
    private Dictionary<int, Dictionary<string, object>> _playerDB = new Dictionary<int,Dictionary<string, object>>();
    // Simulates remote room/session properties storage: Key -> Value.
    private Dictionary<string, object> _roomDB = new Dictionary<string, object>();
    public Local_GameDataSessionService()
    {
        // Immediately signals that data is ready to systems.
        OnDataReady?.Invoke();
    }
    // --- Player Data Operations ---
    /// Saves a key-value pair as a property for a specific player in local memory.
    public void SetPlayerProperty(int playerId, string key, object value)
    {
        // Ensure the player entry exists in the database.
        if (!_playerDB.ContainsKey(playerId))
        {
            _playerDB[playerId] = new Dictionary<string, object>();
        }
        // Store the value in memory.
        _playerDB[playerId][key] = value;
        // Notify listeners immediately.
        OnPlayerSessionDataChanged?.Invoke(playerId, key, value);
    }
    // Retrieves a player property from local memory, casting it to the requested type T.
    public T GetPlayerProperty<T>(int playerId, string key, T defaultValue)
    {
        // Check for the existence of the Player ID and the property key.
        if (_playerDB.ContainsKey(playerId) && _playerDB[playerId].ContainsKey(key))
        {
            // Return the value stored in memory.
            return (T)_playerDB[playerId][key];
        }
        // If not found, return the specified default value.
        return defaultValue;
    }
    // --- Room Data Operations ---
    /// Saves a key-value pair as a room property in local memory
    public void SetRoomProperty(string key, object value)
    {
        _roomDB[key] = value;
        OnRoomSessionDataChanged?.Invoke(key, value); // Immediate notification.
    }
    // Retrieves a room property from local memory, or returns the default value
    public T GetRoomProperty<T>(string key, T defaultValue)
    {
        if (_roomDB.ContainsKey(key))
            return (T)_roomDB[key];
        return defaultValue;
    }
    // Returns the standard Unity time (Time.time) as the session time.
    public double GetSessionTime() => Time.time;
}