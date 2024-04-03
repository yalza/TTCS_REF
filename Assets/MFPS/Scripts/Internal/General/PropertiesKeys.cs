using UnityEngine;

public static class PropertiesKeys
{
    // Room hashtable keys
    public const string TimeRoomKey = "rTr";
    public const string GameModeKey = "rGm";
    public const string SceneNameKey = "rLn";
    public const string CustomSceneName = "rCsn";
    public const string RoomRoundKey = "rGpr";
    public const string RoomGoal = "rG";
    public const string TeamSelectionKey = "rAt";
    public const string RoomFriendlyFire = "rFf";
    public const string Team1Score = "rTs1";
    public const string Team2Score = "rTs2";
    public const string MaxPing = "rMp";
    public const string RoomPassword = "rPsw";
    public const string WithBotsKey = "rWb";
    public const string TimeState = "rTs";

    // Player in room properties keys
    public const string TeamKey = "Team";
    public const string KillsKey = "Kills";
    public const string DeathsKey = "Deaths";
    public const string ScoreKey = "Score";
    public const string KickKey = "Kick";
    public const string UserRole = "UserRole";
    public const string PlayerTotalScore = "TotalScore";
    public const string PlayerID = "playerID";
    public const string WaitingState = "WaitingState";

    // Custom network event codes
    public const byte KickPlayerEvent = 101;
    public const byte WaitingPlayerReadyEvent = 102;
    public const byte WaitingInitialSyncEvent = 103;
    public const byte WaitingStartGame = 104;
    public const byte ChatEvent = 105;
    public const byte KillFeedEvent = 106;
    public const byte DemolitionEvent = 107;
    public const byte DMBombEvent = 108;
    public const byte KillStreakEvent = 109;
    public const byte BattleRoyalEvent = 110;
    public const byte NetworkItemInstance = 111;
    public const byte NetworkItemChange = 112;
    public const byte WeaponPickUpEvent = 113;
    public const byte VoteEvent = 114;
    public const byte EliminationGameMode = 115;
    public const byte KillConfirmedGameMode = 116;
    public const byte CaptureOfFlagMode = 117;
    public const byte DropManager = 118;
    public const byte DoorSystem = 119;
    public const byte CountdownEvent = 120;

    /// <summary>
    /// Return an per game unique key.
    /// </summary>
    /// <param name="key">key end point</param>
    /// <returns></returns>
    public static string GetUniqueKey(string key)
    {
        return string.Format("{0}.{1}.{2}", Application.companyName, Application.productName, key);
    }

    /// <summary>
    /// Return an per game unique key for an specific player.
    /// </summary>
    /// <param name="key">key end point</param>
    /// <param name="player">player nick</param>
    /// <returns></returns>
    public static string GetUniqueKeyForPlayer(string key, string player)
    {
        return string.Format("{0}.{1}.{2}.{3}", Application.companyName, Application.productName, player, key);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool GetBoolPrefs(this string key, bool defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1 ? true : false;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void SetBoolPrefs(this string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
}