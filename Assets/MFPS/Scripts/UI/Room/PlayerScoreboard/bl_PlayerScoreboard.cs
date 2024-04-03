using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using System.Linq;
using TMPro;
using MFPS.Runtime.AI;

public class bl_PlayerScoreboard : bl_PlayerScoreboardBase
{
    #region Public members
    [Header("Settings")]
    [LovattoToogle] public bool updateOnEnable = true;
    [LovattoToogle] public bool autoUpdate = true;

    [Header("References")]
    public bl_PlayerScoreboardTableBase[] TwoTeamScoreboards;
    public bl_PlayerScoreboardTableBase OneTeamScoreboard;
    public bl_ScoreboardPopUpMenuBase popUpMenu;
    public GameObject playerscoreboardUIBinding;
    public TextMeshProUGUI SpectatorsCountText;
    #endregion

    #region Private members
    Dictionary<int, bl_PlayerScoreboardUIBase> cachedUIBindings = new Dictionary<int, bl_PlayerScoreboardUIBase>();
    Dictionary<MFPSBotProperties, bl_PlayerScoreboardUIBase> cachedBotsUIBindings = new Dictionary<MFPSBotProperties, bl_PlayerScoreboardUIBase>();
    bool botsScoreInstance = false;
    private List<bl_PlayerScoreboardUIBase> cachePlayerScoreboardSorted = new List<bl_PlayerScoreboardUIBase>();
    private List<bl_PlayerScoreboardUIBase> cachePlayerScoreboardSorted2 = new List<bl_PlayerScoreboardUIBase>();
    #endregion

    public bool isShowingTables { get; set; } = false;

    #region Unity Methods
    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        bl_PhotonCallbacks.PlayerLeftRoom += OnPlayerLeftRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_PhotonCallbacks.PlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
        bl_PhotonCallbacks.RoomPropertiesUpdate += OnRoomPropertiesUpdate;
        if (updateOnEnable) ForceUpdateAll();
        if (popUpMenu != null) popUpMenu.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_PhotonCallbacks.PlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
        bl_PhotonCallbacks.RoomPropertiesUpdate -= OnRoomPropertiesUpdate;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        bl_PhotonCallbacks.PlayerLeftRoom -= OnPlayerLeftRoom;
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public void UpdateTables()
    {
        int spectators = 0;
        var players = bl_PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i];
            //check if the player has selected a team
            if (p.GetPlayerTeam() != Team.None)
            {
                //is the ui binding already created for this player?
                if (cachedUIBindings.ContainsKey(p.ActorNumber))
                {
                    //if it's so, then simply refresh his info
                    cachedUIBindings[p.ActorNumber]?.Refresh();
                }
                else
                {
                    bl_PlayerScoreboardUIBase script = null;
                    if (!isOneTeamMode)
                    {
                        if (p.GetPlayerTeam() == Team.Team1)
                            script = TwoTeamScoreboards[0].Instance(p, playerscoreboardUIBinding);
                        else
                            script = TwoTeamScoreboards[1].Instance(p, playerscoreboardUIBinding);
                    }
                    else
                    {
                        script = OneTeamScoreboard.Instance(p, playerscoreboardUIBinding);
                    }
                    cachedUIBindings.Add(p.ActorNumber, script);
                }
            }
            else { spectators++; }//if has not team
        }
        UpdateBotScoreboard();
        if (SpectatorsCountText != null)
        {
            SpectatorsCountText.text = string.Format(bl_GameTexts.Spectators.Localized(36), spectators);
        }
        SortScoreboard();
    }

    /// <summary>
    /// Update the scoreboard players and bots fields
    /// </summary>
    void UpdateBotScoreboard()
    {
        if (bl_AIMananger.Instance == null || !bl_AIMananger.Instance.BotsActive || bl_AIMananger.Instance.BotsStatistics.Count <= 0) return;

        int c = bl_AIMananger.Instance.BotsStatistics.Count;
        for (int i = 0; i < c; i++)
        {
            MFPSBotProperties stat = bl_AIMananger.Instance.BotsStatistics[i];
            if (botsScoreInstance)
            {
                if (cachedBotsUIBindings.ContainsKey(stat))
                {
                    cachedBotsUIBindings[stat]?.UpdateBot();
                }
                else
                {
                    InstanceBotUIBinding(stat);
                }
            }
            else
            {
                InstanceBotUIBinding(stat);
            }
        }
        botsScoreInstance = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    void InstanceBotUIBinding(MFPSBotProperties info)
    {
        bl_PlayerScoreboardUIBase script = null;
        if (!isOneTeamMode)
        {
            if (info.Team == Team.Team1)
                script = TwoTeamScoreboards[0].InstanceBot(info, playerscoreboardUIBinding);
            else
                script = TwoTeamScoreboards[1].InstanceBot(info, playerscoreboardUIBinding);
        }
        else
        {
            script = OneTeamScoreboard.InstanceBot(info, playerscoreboardUIBinding);
        }
        cachedBotsUIBindings.Add(info, script);
    }

    /// <summary>
    /// Force update the scoreboard information
    /// </summary>
    public override void ForceUpdateAll()
    {
        List<bl_PlayerScoreboardUIBase> ailist = cachedBotsUIBindings.Values.ToList();
        foreach (var item in ailist)
        {
            item?.Refresh();
        }
        UpdateTables();
    }

    /// <summary>
    /// Sort the scoreboard players by their score
    /// </summary>
    void SortScoreboard()
    {
        if (isOneTeamMode)
        {
            cachePlayerScoreboardSorted.Clear();
            cachePlayerScoreboardSorted.AddRange(cachedUIBindings.Values.ToArray());
            cachePlayerScoreboardSorted.AddRange(cachedBotsUIBindings.Values.ToArray());
            cachePlayerScoreboardSorted = cachePlayerScoreboardSorted.OrderBy(x => x.GetScore()).ToList();

            for (int i = 0; i < cachePlayerScoreboardSorted.Count; i++)
            {
                if (cachePlayerScoreboardSorted[i] == null) return;
                cachePlayerScoreboardSorted[i].transform.SetSiblingIndex((cachePlayerScoreboardSorted.Count - 1) - i);
            }
        }
        else
        {
            cachePlayerScoreboardSorted.Clear();
            cachePlayerScoreboardSorted2.Clear();
            List<bl_PlayerScoreboardUIBase> all = new List<bl_PlayerScoreboardUIBase>();
            all.AddRange(cachedUIBindings.Values.ToArray());
            all.AddRange(cachedBotsUIBindings.Values.ToArray());

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].GetTeam() == Team.Team1)
                {
                    cachePlayerScoreboardSorted.Add(all[i]);
                }
                else if (all[i].GetTeam() == Team.Team2)
                {
                    cachePlayerScoreboardSorted2.Add(all[i]);
                }
            }
            cachePlayerScoreboardSorted = cachePlayerScoreboardSorted.OrderBy(x => x.GetScore()).ToList();
            cachePlayerScoreboardSorted2 = cachePlayerScoreboardSorted2.OrderBy(x => x.GetScore()).ToList();
            for (int i = 0; i < cachePlayerScoreboardSorted.Count; i++)
            {
                if (cachePlayerScoreboardSorted[i] == null) return;
                cachePlayerScoreboardSorted[i].transform.SetSiblingIndex((cachePlayerScoreboardSorted.Count - 1) - i);
            }
            for (int i = 0; i < cachePlayerScoreboardSorted2.Count; i++)
            {
                if (cachePlayerScoreboardSorted2[i] == null) return;
                cachePlayerScoreboardSorted2[i].transform.SetSiblingIndex((cachePlayerScoreboardSorted2.Count - 1) - i);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetActiveByTeamMode(bool enableJoinButtons = false)
    {
        if (BlockScoreboards) return;

        bool tm = isOneTeamMode;
        OneTeamScoreboard?.SetActive(tm);
        foreach (var table in TwoTeamScoreboards) { table?.SetActive(!tm); }
        SetActiveJoinButtons(enableJoinButtons);
        isShowingTables = true;
        ForceUpdateAll();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetActive(bool active, bool enableJoinButtons = false)
    {
        if (BlockScoreboards) return;

        bool tm = isOneTeamMode;
        OneTeamScoreboard?.SetActive(tm && active);
        foreach (var table in TwoTeamScoreboards) { table?.SetActive(!tm && active); }
        SetActiveJoinButtons(enableJoinButtons);
        isShowingTables = active;
        if (active) { ForceUpdateAll(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetActiveByGameState(bool active)
    {
        SetActive(active, (!bl_GameManager.Instance.FirstSpawnDone && bl_GameManager.Joined));
    }

    /// <summary>
    /// 
    /// </summary>
    public override void ResetJoinButtons()
    {
        OneTeamScoreboard?.ResetJoinButton();
        foreach (var table in TwoTeamScoreboards) { table?.ResetJoinButton(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetActiveJoinButtons(bool active)
    {
        bool tm = isOneTeamMode;
        OneTeamScoreboard?.SetActiveJoinButton(active && tm);
        foreach (var table in TwoTeamScoreboards) { table?.SetActiveJoinButton(active && !tm); }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnPlayerClicked(Player player)
    {
        if (popUpMenu != null)
        {
            bool localPlayer = player != null && player.IsLocal;
            popUpMenu
                .SetTargetPlayer(player)
                .SetActive(true)
               .FilterMenuOptions(new bl_ScoreboardPopUpMenuBase.MenuFilter()
                {
                    IsLocalPlayer = localPlayer,
                    IsBot = player == null,
                });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override bool RemoveUIBinding(bl_PlayerScoreboardUIBase uiBinding)
    {
        if (uiBinding.isBotBinding)
        {
            if (cachedBotsUIBindings.ContainsKey(uiBinding.Bot))
            {
                Destroy(cachedBotsUIBindings[uiBinding.Bot].gameObject);
                cachedBotsUIBindings.Remove(uiBinding.Bot);
                return true;
            }
        }
        else
        {
            if (cachedUIBindings.ContainsKey(uiBinding.RealPlayer.ActorNumber))
            {
                Destroy(cachedUIBindings[uiBinding.RealPlayer.ActorNumber].gameObject);
                cachedUIBindings.Remove(uiBinding.RealPlayer.ActorNumber);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!autoUpdate) return;

        if (cachedUIBindings.ContainsKey(otherPlayer.ActorNumber))
        {
            Destroy(cachedUIBindings[otherPlayer.ActorNumber].gameObject);
            cachedUIBindings.Remove(otherPlayer.ActorNumber);
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!isShowingTables || !autoUpdate) return;
        UpdateTables();
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (!isShowingTables || !autoUpdate) return;
        UpdateTables();
    }
}