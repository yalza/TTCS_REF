using MFPS.Runtime.AI;
using Photon.Realtime;
using UnityEngine;

public abstract class bl_PlayerScoreboardTableBase : MonoBehaviour
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="uiPrefab"></param>
    /// <returns></returns>
    public abstract bl_PlayerScoreboardUIBase Instance(Player player, GameObject uiPrefab);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="uiPrefab"></param>
    /// <returns></returns>
    public abstract bl_PlayerScoreboardUIBase InstanceBot(MFPSBotProperties player, GameObject uiPrefab);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActive(bool active);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActiveJoinButton(bool active);

    /// <summary>
    /// 
    /// </summary>
    public abstract void ResetJoinButton();
}