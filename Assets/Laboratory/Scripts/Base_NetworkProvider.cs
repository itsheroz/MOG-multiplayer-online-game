using UnityEngine;
using System;

/// <summary>
/// Defines standard network states used across different networking solutions.
/// </summary>
public enum NetworkConnectionState
{
    Offline,
    Connecting,
    ConnectedToMaster, // Specific to PUN (Photon Unity Networking)
    InLobby,
    JoiningRoom,
    InRoom
}

/// <summary>
/// Abstract base class designed for cross-network implementation (e.g., PUN, Mirror, NGO).
/// Serves as the standard interface for connection logic and state management.
/// </summary>
public abstract class Base_NetworkProvider : MonoBehaviour
{
    #region State Management

    // Network-agnostic state representing the current connection status.
    public NetworkConnectionState CurrentState { get; protected set; }

    // Event triggered when the network state changes.
    public Action<NetworkConnectionState> OnStateChanged;

    // Event triggered when the room is fully joined and ready for object spawning.
    public event Action OnReadyToSpawn;

    #endregion

    #region Abstract Methods (Mandatory Implementation)

    // Initiates the connection process specific to the underlying network library.
    public abstract void Connect();

    public abstract void CreateRoom();
    public abstract void JoinLobby();
    public abstract void JoinRandomRoom();
    public abstract string CurrentRoomName { get; }

    // Handles disconnection and cleanup procedures.
    public abstract void Disconnect();

    // Handles the instantiation of gameplay objects (e.g., Player Prefab) after joining a room.
    public abstract void SpawnGameplayObjects();

    #endregion

    #region Protected Helpers

    /// <summary>
    /// Updates the current state, invokes the change event, and logs the transition.
    /// </summary>
    /// <param name="newState">The new state to transition to.</param>
    protected virtual void UpdateState(NetworkConnectionState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
        Debug.Log($"<color=cyan>[Network System] State: {newState}</color>");
    }

    /// <summary>
    /// Invokes the OnReadyToSpawn event safely from derived classes.
    /// </summary>
    protected void NotifyReadyToSpawn()
    {
        OnReadyToSpawn?.Invoke();
    }

    #endregion
}