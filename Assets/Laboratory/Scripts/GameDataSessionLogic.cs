using UnityEngine;
// Static class providing extension methods to manage Game and Player Data.
public static class GameDataSessionLogic
{
    // --- Constants ---
    public const string KEY_PLAYER_COLOR = "PlayerColor";
    public const string KEY_GAME_START_TIME = "GameStartTime";
    // Saves the specified color index (choice) for a player.
    public static void RequestSaveColor(this IGameDataSessionService service, int playerId, int
    colorIndex)
    {
        Debug.Log("[GDS Logic] call RequestSaveColor.");
        service.SetPlayerProperty(playerId, KEY_PLAYER_COLOR, colorIndex);
    }
    // Retrieves the saved color index for a specific player. Returns -1 if not found.
    public static int GetPlayerColor(this IGameDataSessionService service, int playerId)
    {
        Debug.Log("[GDS Logic] call GetPlayerColor.");
        return service.GetPlayerProperty(playerId, KEY_PLAYER_COLOR, -1);
    }
    // Converts a color index (int) into a corresponding UnityEngine.Color object.
    public static Color GetColor(int colorChoice)
    {
        switch (colorChoice)
        {
            case 0: return Color.green; // Default Color
            case 1: return Color.blue;
            case 2: return Color.red;
            case 3: return Color.yellow;
            case 4: return Color.cyan;
            case 5: return Color.grey;
            case 6: return Color.magenta;
            case 7: return Color.white;
        }
        return Color.black; // Fallback
    }
    // --- Game Logic ---
    public static void StartGameTimer(this IGameDataSessionService service)
    {
        if (service.IsSessionMaster)
        {
            double now = service.GetSessionTime(); // Gets the synchronized time.
            service.SetRoomProperty(KEY_GAME_START_TIME, now);
        }
    }
    // Retrieves the synchronized game start time stored in the room properties.
    public static double GetGameStartTime(this IGameDataSessionService service)
    {
        return service.GetRoomProperty<double>(KEY_GAME_START_TIME, 0.0);
    }
}
