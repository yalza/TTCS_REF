using System;
using UnityEngine;

[Serializable]
public class GameModeSettings
{

    [Serializable]
    public enum OnRoundStartedSpawn
    {
        SpawnAfterSelectTeam,
        WaitUntilRoundFinish,
    }
    
    public enum OnPlayerDie
    {
        SpawnAfterDelay,
        SpawnAfterRoundFinish,
    }

    public enum RoundModeAllowedOptions
    {
        AllowBoth,
        RoundModeOnly,
        SingleRoundOnly,
        OneRoundOfMultipleRoundsOnly,
    }

    /// <summary>
    /// Game mode name
    /// This is the name that will show to the player
    /// </summary>
    public string ModeName;

    /// <summary>
    /// Game Mode internal identifier
    /// The unique enum identifier for this mode
    /// </summary>
    public GameMode gameMode;

    /// <summary>
    /// Is this mode enable in your game?
    /// </summary>
    [LovattoToogle] public bool isEnabled = true;

    /// <summary>
    /// Allow play with bots in this mode?
    /// </summary>
    [Header("Settings")] 
    [LovattoToogle] public bool supportBots = false;

    /// <summary>
    /// Force automatically assign the teams for this mode
    /// </summary>
    [LovattoToogle] public bool AutoTeamSelection = false;

    /// <summary>
    /// The minimum number of players that have to be joined in the match
    /// before the game can start
    /// </summary>
    [Range(1, 32)] public int RequiredPlayersToStart = 1;

    /// <summary>
    /// What happen when a player joins in the game when the match has started
    /// </summary>
    public OnRoundStartedSpawn onRoundStartedSpawn = OnRoundStartedSpawn.SpawnAfterSelectTeam;

    /// <summary>
    /// What happen when a player die
    /// </summary>
    public OnPlayerDie onPlayerDie = OnPlayerDie.SpawnAfterDelay;

    /// <summary>
    /// Allow players to select the game mode round mode
    /// or force a round mode
    /// </summary>
    public RoundModeAllowedOptions RoundModeAllowed = RoundModeAllowedOptions.AllowBoth;

    /// <summary>
    /// The score/goal name of this game mode
    /// </summary>
    public string GoalName = "Kills";

    /// <summary>
    /// Can players pick up weapons in this game mode?
    /// </summary>
    [LovattoToogle] public bool allowedPickupWeapons = true;

    /// <summary>
    /// Max players options allowed for this game mode
    /// These are the options that appear in the room creator window
    /// Max players are the total player allowed, in two team game modes the max players
    /// per team is equal to 1/2 of the max player
    /// </summary>
    [Header("Options")]
    public int[] maxPlayers = new int[] { 6, 2, 4, 8 };

    /// <summary>
    /// The max score, kills or wherever the goal is in this mode options
    /// These are the options that appear in the room creator window
    /// </summary>
    public int[] GameGoalsOptions = new int[] { 50, 100, 150, 200 };

    /// <summary>
    /// The round time limits options for this game mode
    /// These are the options that appear in the room creator window
    /// The time is in seconds 60 = 1 minute
    /// </summary>
    public int[] timeLimits = new int[] { 900, 600, 1200, 300 };

    public string GetGoalFullName(int goalID) { return string.Format("{0} {1}", GameGoalsOptions[goalID], GoalName); }

    /// <summary>
    /// Get the game mode goal name
    /// e.g: kills, captures, score, points, etc...
    /// </summary>
    public string GetGoalName(int goalID)
    {
        if (GameGoalsOptions.Length <= 0) return GoalName;
        return $"{GameGoalsOptions[goalID]} {GoalName}";
    }

    /// <summary>
    /// Get the goal value by the index in the GameGoalsOptions list
    /// </summary>
    /// <returns></returns>
    public int GetGoalValue(int goalID)
    {
        if (GameGoalsOptions.Length <= 0) return 0;
        if (goalID >= GameGoalsOptions.Length) return GameGoalsOptions[GameGoalsOptions.Length - 1];

        return GameGoalsOptions[goalID];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool HasForcedRoundMode() => RoundModeAllowed != RoundModeAllowedOptions.AllowBoth;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public RoundStyle GetAllowedRoundMode()
    {
        if (RoundModeAllowed == RoundModeAllowedOptions.RoundModeOnly) return RoundStyle.Rounds;
        return RoundStyle.OneMacht;
    }
}