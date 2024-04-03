using System;
using Photon.Realtime;
using UnityEngine;
using MFPS.Internal.Structures;

/// <summary>
/// use to get all room properties easily
/// usage:  RoomProperties props = PhotonNetwork.CurrentRoom.GetRoomInfo();
/// </summary>
public struct MFPSRoomInfo 
{
    public string roomName { get; set; }
    public string mapName { get; set; }
    public string sceneName { get; set; }
    public string password { get; set; }
    public GameMode gameMode { get; set; }
    public int goal { get; set; }
    public int time { get; set; }
    public int maxPing { get; set; }
    public Room room { get; set; }
    public int maxPlayers { get; set; }
    public bool friendlyFire { get; set; }
    public bool withBots { get; set; }
    public bool autoTeamSelection { get; set; }
    public RoundStyle roundStyle { get; set; }

    public bool isPrivate { get { return !string.IsNullOrEmpty(password); } }

    public MFPSRoomInfo(Room roomTarget)
    {
        room = roomTarget;
        roomName = room.Name;
        mapName = (string)roomTarget.CustomProperties[PropertiesKeys.CustomSceneName];
        sceneName = (string)roomTarget.CustomProperties[PropertiesKeys.SceneNameKey];
        password = (string)roomTarget.CustomProperties[PropertiesKeys.RoomPassword];
        string gm = (string)room.CustomProperties[PropertiesKeys.GameModeKey];
        gameMode = (GameMode)Enum.Parse(typeof(GameMode), gm);
        time = (int)room.CustomProperties[PropertiesKeys.TimeRoomKey];
        goal = (int)room.CustomProperties[PropertiesKeys.RoomGoal];
        maxPing = (int)room.CustomProperties[PropertiesKeys.MaxPing];
        maxPlayers = room.MaxPlayers;
        friendlyFire = (bool)room.CustomProperties[PropertiesKeys.RoomFriendlyFire];
        withBots = (bool)room.CustomProperties[PropertiesKeys.WithBotsKey];
        autoTeamSelection = (bool)room.CustomProperties[PropertiesKeys.TeamSelectionKey];
        roundStyle = (RoundStyle)room.CustomProperties[PropertiesKeys.RoomRoundKey];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public MapInfo GetMapInfo()
    {
        var mapName = sceneName;
        var map = bl_GameData.Instance.AllScenes.Find(x => x.RealSceneName == mapName);
        return map;
    }
}