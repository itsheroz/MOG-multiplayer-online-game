using UnityEngine;

public class PunGameSetting
{
    public const float MIN_SPAWN_TIME = 5.0f;
    public const float MAX_SPAWN_TIME = 10.0f;

    public const float PLAYER_RESPAWN_TIME = 4.0f;

    public const string PLAYER_LIVES = "PlayerLives";
    public const int PLAYER_MAX_LIVES = 100;
    
    public const string PLAYER_READY = "IsPlayerReady";
    public const string PLAYER_LOADED_LEVEL = "PlayerLoadedLevel";
    public const string PLAYER_COLOR = "PlayerColor";

    public const string START_GAMETIME = "GameTime";
    public const string GAMESTATE = "GameState";

    public const string TEAMMODE = "TeamMode";
    public static Color GetColor(int colorChoice)
    {
        switch (colorChoice)
        {

            case 0: return Color.green;
            /// Change Color Index to support team
            case 1: return Color.blue;
            case 2: return Color.red;
            ///
            case 3: return Color.yellow;
            case 4: return Color.cyan;
            case 5: return Color.grey;
            case 6: return Color.magenta;
            case 7: return Color.white;
        }

        return Color.black;
    }
}
