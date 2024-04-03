using MFPS.Runtime.AI;
using Photon.Realtime;
using UnityEngine;

public abstract class bl_PlayerScoreboardUIBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public MFPSBotProperties Bot 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// 
    /// </summary>
    public bool isBotBinding 
    { 
        get; 
        set; 
    } = false;

    /// <summary>
    /// 
    /// </summary>
    public Player RealPlayer 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="bot"></param>
    public abstract void Init(Player player, MFPSBotProperties bot = null);

    /// <summary>
    /// Called each time the scoreboard is update (when the scoreboard is open)
    /// </summary>
    /// <returns></returns>
    public abstract bool Refresh();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract bool UpdateBot();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract int GetScore();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract Team GetTeam();
}