using Photon.Realtime;
using UnityEngine;

public abstract class bl_WaitingRoomBase : bl_PhotonHelper
{
    /// <summary>
    /// 
    /// </summary>
    public abstract void StartGame();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="teamID"></param>
    public abstract void JoinToTeam(Team team);

    /// <summary>
    /// 
    /// </summary>
    public abstract void SetLocalPlayerReady();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public abstract bool IsPlayerReady(Player player);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract bool IsLocalReady();

    /// <summary>
    /// 
    /// </summary>
    private static bl_WaitingRoomBase _instance;
    public static bl_WaitingRoomBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<bl_WaitingRoomBase>();
            }
            return _instance;
        }
    }
}