using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class bl_WaitingRoomUI : bl_WaitingRoomUIBase
{
    [Header("References")]
    public GameObject Content;
    public GameObject WaitingPlayerPrefab;
    public GameObject LoadingMapUI;
    public GameObject StartScreen;
    public GameObject waitingRequiredPlayersUI;
    public RectTransform PlayerListPanel;
    public TextMeshProUGUI RoomNameText;
    public TextMeshProUGUI MapNameText;
    public TextMeshProUGUI GameModeText;
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI GoalText;
    public TextMeshProUGUI BotsText;
    public TextMeshProUGUI FriendlyFireText;
    public TextMeshProUGUI PlayerCountText;
    public Image MapPreview;
    public List<RectTransform> PlayerListHeaders = new List<RectTransform>();
    public Button[] readyButtons;

    private List<bl_WaitingPlayerUIBase> playerListCache = new List<bl_WaitingPlayerUIBase>();

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        Content.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public override void ShowLoadingScreen(bool show)
    {
        LoadingMapUI.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetActive(bool active)
    {
        if (active)
        {
            UpdateRoomInfoUI();
            InstancePlayerList();
            Content.SetActive(true);
            StartScreen.SetActive(true);
        }
        else
        {
            StartScreen.SetActive(false);
            Content.SetActive(false);
            bl_LobbyUI.Instance.blackScreenFader.FadeOut(0.5f);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void InstancePlayerList()
    {
        playerListCache.ForEach(x => { if (x != null) { Destroy(x.gameObject); } });
        playerListCache.Clear();

        Player[] list = PhotonNetwork.PlayerList;
        List<Player> secondTeam = new List<Player>();
        bool otm = isOneTeamModeUpdate;
        PlayerListHeaders.ForEach(x => x.gameObject.SetActive(!otm));
        for (int i = 0; i < list.Length; i++)
        {
            if (otm)
            {
                if (bl_PhotonNetwork.IsMasterClient)
                {
                    if(list[i].GetPlayerTeam() != Team.All)
                    {
                        list[i].SetPlayerTeam(Team.All);
                    }
                }
                SetPlayerToList(list[i]);
            }
            else
            {
                if (list[i].GetPlayerTeam() == Team.Team1)
                {
                    SetPlayerToList(list[i]);   
                }
                else if (list[i].GetPlayerTeam() == Team.Team2)
                {
                    secondTeam.Add(list[i]);
                }
            }
        }
        if (!otm) { PlayerListHeaders[1].SetAsLastSibling(); }
        if (secondTeam.Count > 0)
        {          
            for (int i = 0; i < secondTeam.Count; i++)
            {
                SetPlayerToList(secondTeam[i]);
            }
        }
        UpdatePlayerCount();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetPlayerToList(Player player)
    {
        GameObject g = Instantiate(WaitingPlayerPrefab) as GameObject;
        var wp = g.GetComponent<bl_WaitingPlayerUIBase>();
        wp.SetInfo(player);
        g.transform.SetParent(PlayerListPanel, false);
        playerListCache.Add(wp);
    }
   
    /// <summary>
    /// 
    /// </summary>
    public override void UpdateRoomInfoUI()
    {
        GameMode mode = GetGameModeUpdated;
        Room room = PhotonNetwork.CurrentRoom;
        RoomNameText.text = room.Name.ToUpper();
        string mapName = (string)room.CustomProperties[PropertiesKeys.SceneNameKey];    
        var si = bl_GameData.Instance.AllScenes.Find(x => x.RealSceneName == mapName);
        MapPreview.sprite = si.Preview;
        MapNameText.text = si.ShowName.ToUpper();
        GameModeText.text = mode.GetName().ToUpper();
        int t = (int)room.CustomProperties[PropertiesKeys.TimeRoomKey];
        TimeText.text = (t / 60).ToString().ToUpper() + ":00";
        BotsText.text = string.Format("BOTS: {0}", (bool)room.CustomProperties[PropertiesKeys.WithBotsKey] ? "ON" : "OFF");
        FriendlyFireText.text = string.Format("FRIENDLY FIRE: {0}", (bool)room.CustomProperties[PropertiesKeys.RoomFriendlyFire] ? "ON" : "OFF");       
        UpdatePlayerCount();
        readyButtons[0].gameObject.SetActive(bl_PhotonNetwork.IsMasterClient);
        readyButtons[1].gameObject.SetActive(!bl_PhotonNetwork.IsMasterClient);
        readyButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = bl_WaitingRoomBase.Instance.IsLocalReady() ? "CANCEL".Localized(67).ToUpper() : "READY".Localized(184).ToUpper();

        string goal = room.CustomProperties[PropertiesKeys.RoomGoal].ToString();
        if (goal == "0" || string.IsNullOrEmpty(goal))
        {
            GoalText.text = GetGameModeUpdated.GetModeInfo().GoalName.ToUpper();
        }
        else
        {
            GoalText.text = $"{goal} {GetGameModeUpdated.GetModeInfo().GoalName.ToUpper()}";
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public override void UpdatePlayerCount()
    {
        int required = GetGameModeUpdated.GetGameModeInfo().RequiredPlayersToStart;
        if(required > 1)
        {
            bool allRequired = (PhotonNetwork.PlayerList.Length >= required);
            readyButtons[0].interactable = (bl_PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length >= required);
            PlayerCountText.text = string.Format("{0} OF {2} PLAYERS ({1} MAX)", PhotonNetwork.PlayerList.Length, PhotonNetwork.CurrentRoom.MaxPlayers, required);
            waitingRequiredPlayersUI?.SetActive(!allRequired);
        }
        else
        {
            readyButtons[0].interactable = true;
            waitingRequiredPlayersUI?.SetActive(false);
            PlayerCountText.text = string.Format("{0} PLAYERS ({1} MAX)", PhotonNetwork.PlayerList.Length, PhotonNetwork.CurrentRoom.MaxPlayers);
        }

        int spectatorsCount = GetSpectatorsCount();
        if(spectatorsCount > 0)
        {
            PlayerCountText.text += $" SPECTATORS {spectatorsCount}";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdateAllPlayersStates()
    {
        playerListCache.ForEach(x => { if(x != null) x.UpdateState(); });
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetLocalReady()
    {
        bl_WaitingRoomBase.Instance.SetLocalPlayerReady();
        readyButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = bl_WaitingRoomBase.Instance.IsLocalReady() ? "CANCEL".Localized(67).ToUpper() : "READY".Localized(184).ToUpper();
    }

    /// <summary>
    /// 
    /// </summary>
    public void MasterStartTheGame()
    {
        bl_WaitingRoomBase.Instance.StartGame();
    }

    /// <summary>
    /// 
    /// </summary>
    public void EnterAsSpectator()
    {
        bl_LobbyUI.ShowConfirmationWindow(bl_GameTexts.EnterAsSpectator.Localized(209), () =>
        {
            bl_WaitingRoomBase.Instance.JoinToTeam(Team.None);
            bl_SpectatorModeBase.EnterAsSpectator = true;
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private int GetSpectatorsCount()
    {
        int count = 0;
        var players = PhotonNetwork.PlayerList;
        foreach (var player in players)
        {
            if(player.GetPlayerTeam() == Team.None)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 
    /// </summary>
    public void LeaveRoom(bool comfirmed)
    {
        if (comfirmed)
        {
            bl_LobbyUI.Instance.blackScreenFader.FadeIn(0.5f);
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            bl_LobbyUI.ShowConfirmationWindow(bl_GameTexts.LeaveRoomConfirmation.Localized(211), () =>
            {
                LeaveRoom(true);
            });
        }      
    }

  
}