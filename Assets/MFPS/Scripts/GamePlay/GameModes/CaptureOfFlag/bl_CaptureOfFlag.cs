using Photon.Realtime;
using UnityEngine;
using MFPS.GameModes.CaptureOfFlag;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class bl_CaptureOfFlag : bl_PhotonHelper, IGameMode
{
    [Header("Settings")]
    public int scorePerCapture = 500;
    public int scorePerRecover = 100;
    public float captureAreaRange = 3;
    [LovattoToogle] public bool moveFlagWithCarrierMotion = true;

    [Header("Events (For Local Only)")]
    public bl_EventHandler.UEvent onPickUp;
    public bl_EventHandler.UEvent onCapture;
    public bl_EventHandler.UEvent onRecover;

    [Header("References")]
    public GameObject Content;
    public bl_FlagPoint Team1Flag;
    public bl_FlagPoint Team2Flag;

    private bool isActive = false;
    public int CapturesToWin { get; set; } = 3;

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
        if (isActive)
        {
            bl_PhotonNetwork.RemoveNetworkCallback(OnNetworkMessage);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void LateUpdate()
    {
        if (Team1Flag) Team1Flag.HandleFlagCapture();
        if (Team2Flag) Team2Flag.HandleFlagCapture();
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckScores()
    {
        if (!bl_RoomSettings.Instance.RoomInfoFetched) return;

        int team1 = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
        int team2 = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);

        if (team1 >= CapturesToWin || team2 >= CapturesToWin)
        {
            bl_MatchTimeManagerBase.Instance.FinishRound();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void OnNetworkMessage(Hashtable data)
    {
        var command = (int)data["cmd"];
        switch (command)
        {
            case 0: // Change Flag State
                OnFlagStateChanged(data);
                break;
            case 1: // Drop a flag
                var flag = (Team)data["team"] == Team.Team1 ? Team1Flag : Team2Flag;
                flag.DropFlag(data);
                break;
            case 2: // Return flag to origin position
                flag = (Team)data["team"] == Team.Team1 ? Team1Flag : Team2Flag;
                flag.SetFlagToOrigin();
                break;
            case 3: // Sync flags states
                SyncFlags(data);
                break;
            default:
                Debug.Log("Not defined command: " + command);
                return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void OnFlagStateChanged(Hashtable data)
    {
        var flagTeam = (Team)data["team"];
        var newState = (bl_FlagPoint.FlagState)data["state"];
        var flag = flagTeam == Team.Team1 ? Team1Flag : Team2Flag;
        var player = (Player)data["player"];

        flag.State = newState;
        switch (newState)
        {
            case bl_FlagPoint.FlagState.Captured:
                flag.OnCapture(player);
                break;
            case bl_FlagPoint.FlagState.PickUp:
                flag.OnPickup(player, (int)data["viewID"]);
                break;
            case bl_FlagPoint.FlagState.InHome:
                flag.Recover(player);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void SyncFlags(Hashtable data)
    {
        Team1Flag.State = (bl_FlagPoint.FlagState)data["f1s"];
        Team2Flag.State = (bl_FlagPoint.FlagState)data["f2s"];

        if (Team1Flag.State == bl_FlagPoint.FlagState.PickUp)
        {
            var player = bl_GameManager.Instance.FindActor((int)data["f1ca"]);
            if (player != null) Team1Flag.SetFlagToCarrier(player.GetComponent<bl_PlayerSettings>());
        }

        if (Team2Flag.State == bl_FlagPoint.FlagState.PickUp)
        {
            var player = bl_GameManager.Instance.FindActor((int)data["f2ca"]);
            if (player != null) Team2Flag.SetFlagToCarrier(player.GetComponent<bl_PlayerSettings>());
        }
    }

    #region GameMode Interface
    public void Initialize()
    {
        //check if this is the game mode of this room
        if (bl_GameManager.Instance.IsGameMode(GameMode.CTF, this))
        {
            Content.SetActive(true);
            bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.CaptureOfFlagMode, OnNetworkMessage);
            bl_GameManager.Instance.WaitForPlayers(GameMode.CTF.GetGameModeInfo().RequiredPlayersToStart);
            bl_CaptureOfFlagUI.Instance.ShowUp();
            CapturesToWin = (int)bl_PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.RoomGoal];
            isActive = true;
        }
        else
        {
            Content.SetActive(false);
            bl_CaptureOfFlagUI.Instance.Hide();
        }
    }

    public void OnFinishTime(bool gameOver)
    {
        //determine the winner
        string finalText = "";
        if (GetWinnerTeam() != Team.None)
        {
            finalText = GetWinnerTeam().GetTeamName();
        }
        else
        {
            finalText = bl_GameTexts.NoOneWonName;
        }
        bl_RoundFinishScreenBase.Instance?.Show(finalText);
    }

    public void OnLocalPlayerDeath()
    {

    }

    public void OnLocalPlayerKill()
    {

    }

    public void OnLocalPoint(int points, Team teamToAddPoint)
    {
        bl_PhotonNetwork.CurrentRoom.SetTeamScore(teamToAddPoint);
    }

    public void OnOtherPlayerEnter(Player newPlayer)
    {
        if (bl_PhotonNetwork.IsMasterClient)
        {
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("cmd", 3);
            data.Add("f1s", Team1Flag.State);
            data.Add("f2s", Team2Flag.State);
            if (Team1Flag.State == bl_FlagPoint.FlagState.PickUp)
            {
                data.Add("f1ca", Team1Flag.carriyingPlayer.View.ViewID);
            }
            if (Team2Flag.State == bl_FlagPoint.FlagState.PickUp)
            {
                data.Add("f2ca", Team1Flag.carriyingPlayer.View.ViewID);
            }
            bl_PhotonNetwork.Instance.SendDataOverNetworkToPlayer(PropertiesKeys.CaptureOfFlagMode, data, newPlayer);
        }
    }

    public void OnOtherPlayerLeave(Player otherPlayer)
    {

    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(PropertiesKeys.Team1Score) || propertiesThatChanged.ContainsKey(PropertiesKeys.Team2Score))
        {
            int team1 = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
            int team2 = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);
            bl_CaptureOfFlagUI.Instance.SetScores(team1, team2);
            CheckScores();
        }
    }

    public bool isLocalPlayerWinner
    {
        get
        {
            return (bl_PhotonNetwork.LocalPlayer.GetPlayerTeam() == GetWinnerTeam());
        }
    }
    #endregion

    public bl_FlagPoint GetFlag(Team team)
    {
        if (team == Team.Team1)
        {
            return Team1Flag;
        }

        return Team2Flag;
    }

    public Team GetWinnerTeam()
    {
        int team1 = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
        int team2 = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);

        Team winner = Team.None;
        if (team1 > team2) { winner = Team.Team1; }
        else if (team1 < team2) { winner = Team.Team2; }
        else { winner = Team.None; }
        return winner;
    }

    public static Team GetOppositeTeam(Team team)
    {
        if (team == Team.Team1)
        {
            return Team.Team2;
        }

        return Team.Team1;
    }

    private static bl_CaptureOfFlag _instance;
    public static bl_CaptureOfFlag Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_CaptureOfFlag>(); }
            return _instance;
        }
    }
}