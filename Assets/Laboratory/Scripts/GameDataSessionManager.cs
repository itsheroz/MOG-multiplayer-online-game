using System;
using UnityEngine;

// Central manager for all game session and data interactions.
// It acts as a bridge, providing a consistent IGameDataSessionService instance 
// regardless of whether the game is running online or offline.
public class GameDataSessionManager : MonoBehaviour
{
    // Singleton Instance
    public static GameDataSessionManager Instance; // Provides global access to this single manager object.

    // The active service used for reading and writing session data (e.g., player props, room props).
    public IGameDataSessionService Service { get; private set; }

    // Event fired when a new Service (Online/Offline) successfully connects and is ready for use.
    public event Action<IGameDataSessionService> OnServiceConnected;

    [Header("Settings")]
    public bool UseOfflineMode = false; // If true, the manager creates a LocalDataSessionService on Start.

    private void Start()
    {
        // Singleton Setup: Ensures only one instance of the manager exists.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);

        // If OfflineMode is set, initialize with a local, non-networked data service.
        if (UseOfflineMode)
        {
            Service = new Local_GameDataSessionService(); // Placeholder for the local data implementation.
        }
    }

    // Allows an external network layer (e.g., Photon, Mirror) to register itself as the official Service.
    // This updates the central 'Service' interface and notifies all subscribers.
    public void RegisterService(IGameDataSessionService service)
    {
        Service = service;

        if (Service != null)
        {
            Debug.Log("[Manager] Network Session Service Connected.");
            OnServiceConnected?.Invoke(Service); // Notify listeners that the Service is ready.
        }
        else
        {
            Debug.Log("[Manager] Network Session Service Disconnected.");
            // Optional: Logic to handle disconnection (e.g., fallback to offline or display an error).
        }
    }
}