using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public interface IGameMode 
{
    /// <summary>
    /// initialize the game mode
    /// </summary>
    void Initialize();
    /// <summary>
    /// When round time = 0
    /// </summary>
    void OnFinishTime(bool gameOver);
    /// <summary>
    /// Call when LOCAL player score a point in this game mode
    /// </summary>
    /// <param name="points"></param>
    void OnLocalPoint(int points, Team teamToAddPoint);
    /// <summary>
    /// Local player kill someone (only local receive it)
    /// </summary>
    void OnLocalPlayerKill();
    /// <summary>
    /// Local player death (only local receive it)
    /// </summary>
    void OnLocalPlayerDeath();
    /// <summary>
    /// when a new player enter in room
    /// </summary>
    void OnOtherPlayerEnter(Player newPlayer);
    /// <summary>
    /// when a player leave the room
    /// </summary>
    void OnOtherPlayerLeave(Player otherPlayer);
    /// <summary>
    /// Room Properties Update
    /// </summary>
    void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged);
    /// <summary>
    /// use to determinate if the local player win in this game mode
    /// </summary>
    bool isLocalPlayerWinner { get; }
}