using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using HashTable = ExitGames.Client.Photon.Hashtable;
using System.Linq;
using MFPS.Internal.Structures;

public class bl_WaitingRoom : bl_WaitingRoomBase, IMatchmakingCallbacks, IInRoomCallbacks
{
    public List<int> readyPlayers = new List<int>();
    static readonly RaiseEventOptions EventsAll = new RaiseEventOptions();

    private bool isLocalReady = false;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        readyPlayers.Clear();
        PhotonNetwork.AddCallbackTarget(this);
        EventsAll.Receivers = ReceiverGroup.All;
        PhotonNetwork.NetworkingClient.EventReceived += OnEventCustom;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        PhotonNetwork.NetworkingClient.EventReceived -= OnEventCustom;
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoJoinToTeam()
    {
        if (isOneTeamModeUpdate)
        {
            JoinToTeam(Team.All);
        }
        else
        {
            int team1Count = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length;
            int team2Count = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length;
            if(team1Count > team2Count) { JoinToTeam(Team.Team2); }
            else if(team1Count < team2Count) { JoinToTeam(Team.Team1); }
            else { JoinToTeam(Team.Team1); }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void JoinToTeam(Team team)
    {
        bl_PhotonNetwork.LocalPlayer.SetPlayerTeam(team);
        bl_SpectatorModeBase.EnterAsSpectator = false;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalEnterInWaitingRoom()
    {
        var table = bl_UtilityHelper.CreatePhotonHashTable();
        table.Add(PropertiesKeys.UserRole, bl_GameData.Instance.RolePrefix);
        table.Add(PropertiesKeys.PlayerTotalScore, bl_MFPS.LocalPlayer.Stats.GetAllTimeScore());
        bl_PhotonNetwork.LocalPlayer.SetCustomProperties(table);
    }

    /// <summary>
    /// Local
    /// </summary>
    public override void SetLocalPlayerReady()
    {
        isLocalReady = !isLocalReady;
        HashTable table = new HashTable();
        table.Add(PropertiesKeys.WaitingState, isLocalReady ? (int)PlayerState.Ready : (int)PlayerState.NoReady);
        table.Add(PropertiesKeys.PlayerID, bl_PhotonNetwork.LocalPlayer.ActorNumber);
        PhotonNetwork.RaiseEvent(PropertiesKeys.WaitingPlayerReadyEvent, table, EventsAll, SendOptions.SendReliable);
    }

    /// <summary>
    /// From Server
    /// </summary>
    void OnReceivePlayerState(HashTable data)
    {
        if (!data.ContainsKey(PropertiesKeys.WaitingState)) return;

        PlayerState state =  (PlayerState)(int)data[PropertiesKeys.WaitingState];
        int playerID = (int)data[PropertiesKeys.PlayerID];
        if(state == PlayerState.Ready)
        {
            if (!readyPlayers.Contains(playerID)) { readyPlayers.Add(playerID); }
        }
        else
        {
            if (readyPlayers.Contains(playerID)) { readyPlayers.Remove(playerID); }
        }
        OnUpdateStates();
    }

    /// <summary>
    /// Called by Master Client after click on the start button
    /// </summary>
    public override void StartGame()
    {
        if (!bl_PhotonNetwork.IsMasterClient) return;

        SetWaitingState(WaitingState.Started);
    }

    /// <summary>
    /// 
    /// </summary>
    void SetWaitingState(WaitingState state)
    {
        HashTable table = new HashTable();
        table.Add(PropertiesKeys.WaitingState, state);

        HashTable expectTable = new HashTable(1);
        object temp;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PropertiesKeys.WaitingState, out temp))
        {
            expectTable[PropertiesKeys.WaitingState] = temp;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(table, expectTable);
        //We don't need use a RPC cuz all player will receive a callback on OnRoomPropertiesUpdate and we can work with that
    }

    /// <summary>
    /// 
    /// </summary>
    void OnUpdateStates()
    {
        bl_WaitingRoomUIBase.Instance.UpdateAllPlayersStates();
        if (bl_PhotonNetwork.IsMasterClient)
        {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnReceiveInitialInfo(HashTable data)
    {
        string source = (string)data[PropertiesKeys.PlayerID];
        int[] split = source.Split(","[0]).Select(x => int.Parse(x)).ToArray();
        readyPlayers.AddRange(split);
        OnUpdateStates();
    }

    /// <summary>
    /// RaiseEvent = RPC, I just used this cuz I like it more :)
    /// </summary>
    public void OnEventCustom(EventData data)
    {
        if (data.CustomData == null) return;
        
        HashTable t = (HashTable)data.CustomData;
        switch (data.Code)
        {
            case PropertiesKeys.WaitingPlayerReadyEvent:
                OnReceivePlayerState(t);
                break;
            case PropertiesKeys.WaitingInitialSyncEvent:
                OnReceiveInitialInfo(t);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadMap()
    {      
        bl_UtilityHelper.LoadLevel((string)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.SceneNameKey]);
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator WaitUntilFullyJoin()
    {
        //CHECK IF THE ROOM IS ALREADY STARTED
        while (!PhotonNetwork.InRoom)
        {
            yield return null;
        }
        //if this is the master client is means that the room is not initialized
        if (bl_PhotonNetwork.IsMasterClient)
        {
            //so let set the state as waiting
            SetWaitingState(WaitingState.Waiting);
        }
        else//if is a normal client
        {
            //check the state the Waiting room
            if ((WaitingState)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.WaitingState] == WaitingState.Started)//if is started
            {
                //go directly to the game map and skip the waiting room menu
                PhotonNetwork.IsMessageQueueRunning = false;
                LoadMap();
                yield break;
            }
        }

        //if is not started yet -> show the UI and hide the Lobby menu
        bl_LobbyUI.Instance.HideAll();
        OnLocalEnterInWaitingRoom();
        AutoJoinToTeam();
        yield return new WaitForSeconds(0.5f);
        bl_WaitingRoomUIBase.Instance.SetActive(true);
        bl_LobbyUI.Instance.blackScreenFader.FadeOut(0.4f);
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator MoveToMap()
    {
        bl_WaitingRoomUIBase.Instance.ShowLoadingScreen(true);
        yield return new WaitForSeconds(2);
        yield return bl_LobbyUI.Instance.blackScreenFader.FadeIn(0.3f);
        LoadMap();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public override bool IsPlayerReady(Player player)
    {
        return readyPlayers.Contains(player.ActorNumber);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsLocalReady()
    {
        return isLocalReady;
    }

    #region Photon Callbacks
    public void OnCreatedRoom()
    {
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
    }

    public void OnJoinedRoom()
    {
        if (bl_MFPS.GameData.UsingWaitingRoom())
        {
            StartCoroutine(WaitUntilFullyJoin());
        }
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
    }

    public void OnLeftRoom()
    {
        bl_WaitingRoomUIBase.Instance.SetActive(false);
        if (PhotonNetwork.IsConnected)
        {
            if (!PhotonNetwork.InLobby) { PhotonNetwork.JoinLobby(); }
            else { Debug.Log("Leave Pre Match but already connected to lobby."); bl_LobbyUI.Instance.blackScreenFader.FadeOut(0.5f); }
        }
        else
        {
            Debug.Log("Disconnected from Pre-Match");
        }
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        bl_WaitingRoomUIBase.Instance.UpdatePlayerCount();
        if (bl_PhotonNetwork.IsMasterClient)
        {
            HashTable table = new HashTable();
            string rp = string.Join(",", readyPlayers.Select(x => x.ToString()).ToArray());
            table.Add(PropertiesKeys.PlayerID, rp);

            PhotonNetwork.RaiseEvent(PropertiesKeys.WaitingPlayerReadyEvent, table, new RaiseEventOptions() { TargetActors = new int[] { newPlayer.ActorNumber } }, SendOptions.SendReliable);
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        bl_WaitingRoomUIBase.Instance.InstancePlayerList();
        if (readyPlayers.Contains(otherPlayer.ActorNumber)) { readyPlayers.Remove(otherPlayer.ActorNumber); }
    }

    public void OnRoomPropertiesUpdate(HashTable propertiesThatChanged)
    {
        //if the wait state has change
        if (propertiesThatChanged.ContainsKey(PropertiesKeys.WaitingState))
        {
            WaitingState state = (WaitingState)propertiesThatChanged[PropertiesKeys.WaitingState];
            if(state == WaitingState.Started)//if the master client start the game
            {
                PhotonNetwork.IsMessageQueueRunning = false;
                //load the map scene
                StartCoroutine(MoveToMap());
            }
        }
        else
        {
            bl_WaitingRoomUIBase.Instance.UpdateRoomInfoUI();
            if (propertiesThatChanged.ContainsKey(PropertiesKeys.GameModeKey) || propertiesThatChanged.ContainsKey(PropertiesKeys.TeamKey))
            {
                if (!isOneTeamModeUpdate && bl_PhotonNetwork.LocalPlayer.GetPlayerTeam() == Team.All)
                {
                    AutoJoinToTeam();
                }
                bl_WaitingRoomUIBase.Instance.InstancePlayerList();
            }

            if (propertiesThatChanged.ContainsKey(PropertiesKeys.PlayerTotalScore) || propertiesThatChanged.ContainsKey(PropertiesKeys.UserRole))
            {
                bl_WaitingRoomUIBase.Instance.InstancePlayerList();
            }
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        bl_WaitingRoomUIBase.Instance.UpdatePlayerCount();
        bl_WaitingRoomUIBase.Instance.InstancePlayerList();
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        bl_WaitingRoomUIBase.Instance.UpdateRoomInfoUI();
    }
    #endregion

    [System.Serializable]
    public enum PlayerState
    {
        NoReady = 0,
        Ready = 1,
    }

    [System.Serializable]
    public enum WaitingState
    {
        Waiting = 0,
        Started = 1,
    }
}