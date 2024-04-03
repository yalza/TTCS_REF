using UnityEngine;

public enum RoundStyle
{
    Rounds,
    OneMacht,
    RoundsOneMatch,
}

public enum RoomTimeState
{
    None = 0,
    Started = 1,
    Countdown = 2,
    Waiting = 3,
    StartedAfterCountdown = 4,
}

[System.Serializable, System.Flags]
public enum RoomUILayers
{
    TopScoreBoard = 1,
    WeaponData = 2,
    PlayerStats = 4,
    KillFeed = 8,
    Time = 16,
    Loadout = 32,
    Scoreboards = 64,
    Misc = 128,
}