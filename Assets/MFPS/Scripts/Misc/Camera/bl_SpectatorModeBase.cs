using System;
using UnityEngine;

public abstract class bl_SpectatorModeBase : bl_MonoBehaviour
{
    [Serializable]
    public enum LeaveSpectatorModeAction
    {
        AllowSelectTeam,
        ReturnToLobby,
    }

    /// <summary>
    /// Is the spectator mode active?
    /// </summary>
    public virtual bool isActive 
    { 
        get; 
        set; 
    } = false;

    /// <summary>
    /// Does the local player has enters as spectator to the current match?
    /// </summary>
    private static bool s_enterAsSpectator = false;
    public static bool EnterAsSpectator
    {
        get => s_enterAsSpectator;
        set => s_enterAsSpectator = value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActiveSpectatorMode(bool active);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActiveUI(bool active);

    /// <summary>
    /// Can the player select a team after being in spectator mode?
    /// </summary>
    /// <returns></returns>
    public virtual bool CanSelectTeam()
    {
        if (!isActive) return true;
        return bl_GameData.Instance.onLeaveSpectatorMode == LeaveSpectatorModeAction.AllowSelectTeam;
    }

    private static bl_SpectatorModeBase _instance;
    public static bl_SpectatorModeBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<bl_SpectatorModeBase>();
            }
            return _instance;
        }
    }
}