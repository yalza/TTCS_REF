using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using MFPS.GameModes.FreeForAll;

public class bl_FreeForAll : MonoBehaviour, IGameMode
{

    private bool isSub = false;
    [HideInInspector] public List<MFPSPlayer> FFAPlayerSort = new List<MFPSPlayer>();

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (!bl_PhotonNetwork.IsConnected)
            return;

        Initialize();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        if (isSub)
        {
            bl_PhotonCallbacks.PlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey(PropertiesKeys.KillsKey))
        {
            CheckScore();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckScore()
    {
        if (!bl_RoomSettings.Instance.RoomInfoFetched) return;

        FFAPlayerSort.Clear();
        FFAPlayerSort.AddRange(bl_GameManager.Instance.OthersActorsInScene);
        FFAPlayerSort.Add(bl_GameManager.Instance.LocalActor);

        MFPSPlayer player = null;
        if (FFAPlayerSort.Count > 0 && FFAPlayerSort != null)
        {
            FFAPlayerSort.Sort(bl_UtilityHelper.GetSortPlayerByKills);
            player = FFAPlayerSort[0];
        }
        else
        {
            player = bl_GameManager.Instance.LocalActor;
        }
        bl_FreeForAllUI.Instance.SetScores(player);
        //check if the best player reach the max kills
        if((int)player.GetPlayerPropertie(PropertiesKeys.KillsKey) >= bl_RoomSettings.Instance.GameGoal && !bl_PhotonNetwork.OfflineMode)
        {
            bl_MatchTimeManagerBase.Instance.FinishRound();
            return;
        }
        //check if bots have not reach max kills
        if (bl_AIMananger.Instance != null && bl_AIMananger.Instance.BotsActive && bl_AIMananger.Instance.BotsStatistics.Count > 0)
        {
            if (bl_AIMananger.Instance.GetBotWithMoreKills().Kills >= bl_RoomSettings.Instance.GameGoal)
            {
                bl_MatchTimeManagerBase.Instance.FinishRound();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public MFPSPlayer GetBestPlayer()
    {
        if (FFAPlayerSort.Count > 0 && FFAPlayerSort != null)
        {
            return FFAPlayerSort[0];
        }
        else
        {
            return bl_GameManager.Instance.LocalActor;
        }
    }

    #region Interface
    public bool isLocalPlayerWinner
    {
        get
        {
            string winner = GetBestPlayer().Name;
            if (bl_AIMananger.Instance != null && bl_AIMananger.Instance.GetBotWithMoreKills().Kills >= bl_RoomSettings.Instance.GameGoal)
            {
                winner = bl_AIMananger.Instance.GetBotWithMoreKills().Name;
            }
            return winner == bl_PhotonNetwork.LocalPlayer.NickName;

        }
    }

    public void Initialize()
    {
        //check if this is the game mode of this room
        if (bl_GameManager.Instance.IsGameMode(GameMode.FFA, this))
        {
            bl_GameManager.Instance.SetGameState(MatchState.Starting);
            bl_PhotonCallbacks.PlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
            bl_FreeForAllUI.Instance.ShowUp();
            isSub = true;
        }
        else
        {
            bl_FreeForAllUI.Instance.Hide();
        }
    }

    public void OnFinishTime(bool gameOver)
    {
        bl_RoundFinishScreenBase.Instance?.Show(GetBestPlayer().Name);
    }

    public void OnLocalPlayerDeath()
    {

    }

    public void OnLocalPlayerKill()
    {

    }

    public void OnLocalPoint(int points, Team teamToAddPoint)
    {
        CheckScore();
    }

    public void OnOtherPlayerEnter(Player newPlayer)
    {

    }

    public void OnOtherPlayerLeave(Player otherPlayer)
    {

    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {

    } 
    #endregion
}