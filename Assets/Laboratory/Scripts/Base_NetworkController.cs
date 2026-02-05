using TMPro;
using UnityEngine;

public class Base_NetworkController : MonoBehaviour
{
    public Base_NetworkProvider _networkProvider;

    [Header("UI Debug")]
    public TextMeshProUGUI StatusText; // UI Text reference to display connection status

    private void Start()
    {
        // Automatically find the Network Provider in the scene if not assigned
        if (_networkProvider == null)
            _networkProvider = FindFirstObjectByType<Base_NetworkProvider>();

        if (_networkProvider != null)
        {
            _networkProvider.OnStateChanged += HandleStateChanged;
            UpdateUI(_networkProvider.CurrentState);
        }
        else
        {
            Debug.LogError("Network Provider not found in scene!");
        }
    }

    private void HandleStateChanged(NetworkConnectionState newState)
    {
        UpdateUI(newState);
    }

    private void UpdateUI(NetworkConnectionState state)
    {
        if (StatusText != null)
        {
            StatusText.text = $"System State: {state}";

            switch (state)
            {
                case NetworkConnectionState.Offline: StatusText.color = Color.red; break;
                case NetworkConnectionState.Connecting: StatusText.color = Color.yellow; break;
                case NetworkConnectionState.JoiningRoom: StatusText.color = new Color(1f, 0.64f, 0f); break; // Orange
                case NetworkConnectionState.InRoom: StatusText.color = Color.green; break;
                default: StatusText.color = Color.white; break;
            }
        }
    }

    private void OnGUI()
    {
        if (_networkProvider == null) return;

        GUI.skin.button.fontSize = 14;
        float height = 40;
        float width = 220;

        switch (_networkProvider.CurrentState)
        {
            case NetworkConnectionState.Offline:
                if (GUI.Button(new Rect(10, 10, width, height), "Connect to Server"))
                {
                    _networkProvider.Connect();
                }
                break;

            case NetworkConnectionState.Connecting:
                GUI.Label(new Rect(10, 10, width, height), "Status: Connecting...");
                break;

            case NetworkConnectionState.ConnectedToMaster:
                GUI.Label(new Rect(10, 10, 300, 30), "Status: Connected to Master");

                // Option 1: Enter the Lobby
                // Now calling abstract method directly, no need to check for PUN type
                if (GUI.Button(new Rect(10, 40, width, height), "Join Lobby"))
                {
                    _networkProvider.JoinLobby();
                }

                // Option 2: Attempt to join a random room (Auto-create on fail)
                if (GUI.Button(new Rect(10, 90, width, height), "Join Random / Create"))
                {
                    _networkProvider.JoinRandomRoom();
                }

                // Option 3: Explicitly create a new room
                if (GUI.Button(new Rect(10, 140, width, height), "Create New Room"))
                {
                    _networkProvider.CreateRoom();
                }
                break;

            case NetworkConnectionState.InLobby:
                GUI.Label(new Rect(10, 10, 300, 30), "Status: In Lobby");

                // UI options available inside the Lobby
                if (GUI.Button(new Rect(10, 40, width, height), "Create New Room"))
                {
                    _networkProvider.CreateRoom();
                }

                if (GUI.Button(new Rect(10, 90, width, height), "Join Random Room"))
                {
                    _networkProvider.JoinRandomRoom();
                }
                break;

            case NetworkConnectionState.JoiningRoom:
                // Display this state to inform the user that a room is being found or created
                GUI.Label(new Rect(10, 10, width, height), "Status: Finding or Creating Room...");
                break;

            case NetworkConnectionState.InRoom:
                // Accessing abstract property 'CurrentRoomName' instead of specific library code
                GUI.Label(new Rect(10, 10, 300, 30), $"Playing in: {_networkProvider.CurrentRoomName}");

                if (GUI.Button(new Rect(10, 50, width, height), "Leave Game"))
                {
                    _networkProvider.Disconnect();
                }
                break;
        }
    }
}