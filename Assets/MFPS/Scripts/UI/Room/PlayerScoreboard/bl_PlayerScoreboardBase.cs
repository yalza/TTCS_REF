using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class bl_PlayerScoreboardBase : bl_MonoBehaviour
{
    /// <summary>
    /// If you don't want to show the scoreboard, simply set this property to false
    /// </summary>
    private bool blockScoreboards = false;
    public bool BlockScoreboards
    {
        get { return blockScoreboards; }
        set
        {
            if (value == true)
            {
                SetActive(false);
            }
            blockScoreboards = value;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public abstract void ForceUpdateAll();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActive(bool active, bool enableJoinButtons = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enableJoinButtons"></param>
    public abstract void SetActiveByTeamMode(bool enableJoinButtons = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActiveByGameState(bool active);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActiveJoinButtons(bool active);

    /// <summary>
    /// 
    /// </summary>
    public abstract void OnPlayerClicked(Player player);

    /// <summary>
    /// 
    /// </summary>
    public abstract void ResetJoinButtons();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uiBinding"></param>
    /// <returns></returns>
    public abstract bool RemoveUIBinding(bl_PlayerScoreboardUIBase uiBinding);

    private static bl_PlayerScoreboardBase _instance;
    public static bl_PlayerScoreboardBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_PlayerScoreboardBase>(); }
            if (_instance == null) { _instance = bl_UIReferences.Instance.playerScoreboards; }
            return _instance;
        }
    }
}