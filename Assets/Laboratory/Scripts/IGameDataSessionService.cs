using System;
public interface IGameDataSessionService
{
    // --- 1.1. Player Data ---
    void SetPlayerProperty(int playerId, string key, object value);
    T GetPlayerProperty<T>(int playerId, string key, T defaultValue);
    // --- 1.2. Room Data ---
    void SetRoomProperty(string key, object value);
    T GetRoomProperty<T>(string key, T defaultValue);
    // --- 2. Network State ---
    bool IsDataReady { get; }
    bool IsSessionMaster { get; }
    double GetSessionTime();
    // --- 3. Events ---
    event Action<int, string, object> OnPlayerSessionDataChanged;
    event Action<string, object> OnRoomSessionDataChanged;
    event Action OnDataReady;
}