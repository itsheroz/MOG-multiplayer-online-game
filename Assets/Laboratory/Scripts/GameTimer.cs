using UnityEngine;
using TMPro;
using System;

// Synchronizes and displays the countdown timer for the game session.
// It relies on the network/session service to maintain a consistent start time (Server Time).
public class GameTimer : MonoBehaviour
{
    public delegate void CountdownTimerHasExpired();
    public static event CountdownTimerHasExpired OnCountdownTimerHasExpired; // Event fired when the timer hits zero.

    [Header("UI Reference")]
    public TextMeshProUGUI Text; // The UI text component to display the remaining time.

    [Header("Settings")]
    public float Countdown = 120f; // The total duration of the game countdown (e.g., 2 minutes).

    private bool _isTimerRunning; // Flag to control the Update loop logic.
    private double _timerEndTime; // The synchronized server time when the countdown should end.

    // Key used to retrieve the game start time from the session properties.
    private const string START_TIME_KEY = "GameStartTime";

    // --------------------------------------------------------------------------
    // 1. Lifecycle & Initialization
    // --------------------------------------------------------------------------

    private void Start()
    {
        if (GameDataSessionManager.Instance == null) return;

        // A. Subscribe to the event for when the network service connects (in case it connects later).
        GameDataSessionManager.Instance.OnServiceConnected += InitializeWithService;

        // B. If the service is already connected, start the setup immediately.
        if (GameDataSessionManager.Instance.Service != null)
        {
            InitializeWithService(GameDataSessionManager.Instance.Service);
        }
        else
        {
            Text.text = "Connecting...";
        }
    }

    private void OnDestroy()
    {
        if (GameDataSessionManager.Instance != null)
        {
            GameDataSessionManager.Instance.OnServiceConnected -= InitializeWithService;

            var session = GameDataSessionManager.Instance.Service;
            if (session != null)
            {
                // Clean up subscriptions to prevent memory leaks/errors.
                session.OnRoomSessionDataChanged -= HandleRoomPropertyChange;
                session.OnDataReady -= OnSessionReadyLogic;
            }
        }
    }

    // --------------------------------------------------------------------------
    // 2. Setup Logic
    // --------------------------------------------------------------------------

    // Called when the session service is ready to use (either immediately or after connection).
    private void InitializeWithService(IGameDataSessionService session)
    {
        // 1. Listen for real-time changes to room properties (crucial for joining clients).
        session.OnRoomSessionDataChanged += HandleRoomPropertyChange;

        // 2. Check the initial status of the data.
        if (session.IsDataReady)
        {
            OnSessionReadyLogic();
        }
        else
        {
            Text.text = "Syncing Time...";
            // Wait until all initial room data is synchronized.
            session.OnDataReady += OnSessionReadyLogic;
        }
    }

    // Executes the core logic once session data is confirmed 100% ready.
    private void OnSessionReadyLogic()
    {
        // Unsubscribe the event to prevent multiple calls.
        if (GameDataSessionManager.Instance.Service != null)
            GameDataSessionManager.Instance.Service.OnDataReady -= OnSessionReadyLogic;

        var session = GameDataSessionManager.Instance.Service;

        // 1. Attempt to retrieve the existing start time from the session properties.
        double startTime = session.GetRoomProperty<double>(START_TIME_KEY, 0.0);

        if (startTime > 0.001f) // Checks if a start time has been successfully set.
        {
            // Case A: Game has already started -> Synchronize the timer (Join in progress).
            Debug.Log($"[GameTimer] Found existing game time: {startTime}");
            SetupTimer(startTime);
        }
        else
        {
            // Case B: Game has not started yet.
            if (session.IsSessionMaster)
            {
                // If Host/Master -> Initiate the game start time immediately.
                Debug.Log("[GameTimer] I am Master. Starting new game clock.");
                StartGame();
            }
            else
            {
                // If Client -> Wait for the Master to set the start time property.
                Text.text = "Waiting for Host...";
            }
        }
    }

    // --------------------------------------------------------------------------
    // 3. Core Logic
    // --------------------------------------------------------------------------

    // Called by the Session Master to officially begin the game countdown.
    public void StartGame()
    {
        var session = GameDataSessionManager.Instance.Service;
        if (session == null) return;

        // Use the current synchronized server time.
        double now = session.GetSessionTime();

        // Broadcast the start time by saving it to the Room Property.
        session.SetRoomProperty(START_TIME_KEY, now);

        // Start the timer locally for immediate UI response.
        SetupTimer(now);
    }

    // Handler for real-time room property updates from the network.
    private void HandleRoomPropertyChange(string key, object value)
    {
        if (key == START_TIME_KEY)
        {
            Debug.Log($"[GameTimer] Received new start time from Network: {value}");
            // Start the countdown based on the received server time.
            SetupTimer((double)value);
        }
    }

    // Calculates the timer end time and activates the countdown loop.
    private void SetupTimer(double startTime)
    {
        _timerEndTime = startTime + Countdown;
        _isTimerRunning = true;
    }

    private void Update()
    {
        if (!_isTimerRunning) return;

        if (GameDataSessionManager.Instance.Service == null) return; // Guard against service disconnection.

        // Get the current synchronized server time for accurate countdown.
        double currentTime = GameDataSessionManager.Instance.Service.GetSessionTime();
        double remainingTime = _timerEndTime - currentTime;

        if (remainingTime > 0)
        {
            Text.text = FormatTime(remainingTime);
        }
        else
        {
            _isTimerRunning = false;
            Text.text = "Time's Up!";
            Debug.Log("[GameTimer] Time has expired!");

            // Trigger the static event for other systems to react.
            OnCountdownTimerHasExpired?.Invoke();
        }
    }

    // Converts the remaining time (double seconds) into a Minutes:Seconds string format.
    string FormatTime(double timeInSeconds)
    {
        if (timeInSeconds < 0) timeInSeconds = 0; // Prevent negative time display.

        TimeSpan ts = TimeSpan.FromSeconds(timeInSeconds);
        return string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
    }
}