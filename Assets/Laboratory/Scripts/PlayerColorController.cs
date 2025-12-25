using System;
using UnityEngine;
using Random = UnityEngine.Random;
/// <summary>
/// The abstract base class for controlling a player object's color based on**synchronized session data**.
/// This class handles the reactive logic flow (Initialization, Data Ready, andRealtime Updates)
/// and is **independent of any specific networking solution** (PUN, Mirror, etc.).
/// </summary>
public abstract class PlayerColorController : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected Renderer _targetRenderer; // The Renderer component to visually change the color.
                                                         // Uses a constant key defined centrally in GameDataSessionLogic.
    protected const string COLOR_KEY = GameDataSessionLogic.KEY_PLAYER_COLOR;
    // Abstract properties that must be implemented by network-specific subclasses (e.g., Photon, Local).
    public abstract bool IsMine { get; } // True if this object belongs to thelocal user.
    public abstract int OwnerID { get; } // The unique network/session ID of theplayer who owns this object.
                                         // --------------------------------------------------------------------------
                                         // 1. Lifecycle & Initialization
                                         // --------------------------------------------------------------------------
    protected virtual void Start()
    {
        if (GameDataSessionManager.Instance == null) return;
        // 1. Subscribe: Listen for the service to connect (critical for lateconnections / initial setup).
        GameDataSessionManager.Instance.OnServiceConnected +=
        InitializeWithService;
        // 2. Check: If the Service is already available, start the setup processimmediately.
        if (GameDataSessionManager.Instance.Service != null)
        {
            InitializeWithService(GameDataSessionManager.Instance.Service);
        }
        else Debug.Log("GameDataSessionManager.Instance.Service is NULL.");
    }
    protected virtual void OnDestroy()
    {
        if (GameDataSessionManager.Instance != null)
        {
            // Unsubscribe all events to prevent memory leaks and unexpectedcallbacks.
            GameDataSessionManager.Instance.OnServiceConnected -=
            InitializeWithService;
            var session = GameDataSessionManager.Instance.Service;
            if (session != null)
            {
                session.OnPlayerSessionDataChanged -= HandleDataChange;
                session.OnDataReady -= OnSessionReadyLogic;
            }
        }
    }
    // --------------------------------------------------------------------------
    // 2. Core Logic: Reactive Setup
    // --------------------------------------------------------------------------
    // Called when a session service (Online or Offline) is assigned/connected.
    private void InitializeWithService(IGameDataSessionService session)
    {
        // 1. Subscribe to Realtime Updates (Must be done first to catch immediatechanges).
        session.OnPlayerSessionDataChanged += HandleDataChange;
        // 2. Check the initial state of the session data.
        if (session.IsDataReady)
        {
            // Data is ready -> Proceed immediately.
            OnSessionReadyLogic();
        }
        else
        {
            // Data is not ready -> Wait for the initial synchronization event.
            Debug.Log($"[PlayerColor] Waiting for DataReady... (OwnerID:{OwnerID})");
            session.OnDataReady += OnInitialDataReady;
        }
    }
    // Handler called once the entire session data is initially synchronized.
    private void OnInitialDataReady()
    {
        // Clean up the one-time subscription.
        GameDataSessionManager.Instance.Service.OnDataReady -= OnInitialDataReady;
        OnSessionReadyLogic();
    }
    // The primary function executed when the session data is 100% ready forreading.
    private void OnSessionReadyLogic()
    {
        var session = GameDataSessionManager.Instance.Service;
        // Attempt to retrieve the saved color property (using -1 as the default / not found value).
        int savedColor = session.GetPlayerProperty(OwnerID, COLOR_KEY, -1);
        if (savedColor != -1)
        {
            // CASE 1: Data exists (e.g., player joined late or reconnected).
            // Apply the saved color immediately.
            Debug.Log($"[Player {OwnerID}] Found existing color: {savedColor}");
            UpdateRenderer(savedColor);
        }
        else
        {
            // CASE 2: No data found (First time joining the room).
            if (IsMine)
            {
                // If it's the local player -> Pick a new random color and save it.
                Debug.Log($"[Player {OwnerID}] No color found. Requesting newcolor...");
                int randomColor = Random.Range(1, 8);
                Debug.Log($"[randomColor] is {randomColor}");
                // IMPORTANT: Request the server to save the property.
                // We rely on the network's data change event (HandleDataChange) to officially update the color.
                UpdateRenderer(randomColor); // Optional: Immediate visual feedback
                session.RequestSaveColor(OwnerID, randomColor);
            }
            else
            {
                // If it's a remote player -> Do nothing. Wait for their host to set the property.
                Debug.Log($"[Player {OwnerID}] Waiting for player to set color...");
            }
        }
    }
    // Handler for the Realtime Event (Called when another player or the server changes the property).
    private void HandleDataChange(int playerId, string key, object value)
    {
        Debug.Log("[PlayerColor] Call HandleDataChange");
        // Filter the event to ensure it only applies to this object and thecorrect property key.
        if (playerId == OwnerID && key == COLOR_KEY)
        {
            int colorIndex = Convert.ToInt32(value);
            Debug.Log($"[PlayerColor] Update Received via Network: {colorIndex}");
            UpdateRenderer(colorIndex); // Apply the new synchronized color.
        }
    }
    // Applies the given color index to the target Renderer material.
    protected void UpdateRenderer(int colorIndex)
    {
        // Uses the helper function from GameDataSessionLogic to convert the index to a Color object.
        Color c = GameDataSessionLogic.GetColor(colorIndex);
        Debug.Log("Name : " + this.name + " color : " + c.ToString());
        if (_targetRenderer != null)
            _targetRenderer.material.color = c;
    }
}