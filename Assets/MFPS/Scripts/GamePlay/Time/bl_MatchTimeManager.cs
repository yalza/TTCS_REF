using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;

/// <summary>
/// Default Time Management script, handled the match time
/// For modifications inherited a new script from <see cref="bl_MatchTimeManagerBase"/>
/// </summary>
public class bl_MatchTimeManager : bl_MatchTimeManagerBase
{
    #region Public properties

    [SerializeField] private RoomTimeState m_timeState = RoomTimeState.None;
    public override RoomTimeState TimeState
    {
        get => m_timeState;
        set
        {
            m_timeState = value;
        }
    }

    public override int RoundDuration { get; set; }
    public override float CurrentTime { get; set; }

    public override bool IsInitialized 
    { 
        get; 
        set; 
    }

    public bool IsPaused
    {
        get;
        set;
    } = false;
    #endregion

    #region Private members
    private const string StartTimeKey = "RoomTime";
    private float m_serverTimeReference;
    private const int SECOND = 60;
    private bool roomClose = false;
    private bool isThisActive = true;
    private TextMeshProUGUI TimeText;
    private bool m_timeIsUp = false;
    private int finalCountDown = 10;
    private Action<RoundFinishCause> finishRoundCallback = null;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!bl_PhotonNetwork.IsConnected && !bl_GameData.Instance.offlineMode && !bl_OfflineRoom.Instance.forceOffline)
        {
            bl_UtilityHelper.LoadLevel(bl_GameData.Instance.MainMenuScene);
            return;
        }
        base.Awake();
        if(TimeText == null) TimeText = bl_UIReferences.Instance.PlayerUI.TimeText;
        IsPaused = false;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        if (!bl_PhotonNetwork.IsConnected)
            return;

        //only master client initialized by default, other players will wait until master sync match information after they join.
        if (bl_PhotonNetwork.IsMasterClient)
        {
            Init();
        }
        else
        {
            RemoteInit();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_PhotonCallbacks.MasterClientSwitched += OnMasterClientSwitch;
        bl_PhotonCallbacks.RoomPropertiesUpdate += OnRoomPropertiesChange;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_PhotonCallbacks.MasterClientSwitched -= OnMasterClientSwitch;
        bl_PhotonCallbacks.RoomPropertiesUpdate -= OnRoomPropertiesChange;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public override void SetActive(bool active)
    {
        isThisActive = active;
    }

    /// <summary>
    /// Master Client Initialize the room time
    /// </summary>
    public override void Init()
    {
        //if the match is waiting for a minimum amount of players to start
        if (bl_GameManager.Instance.GameMatchState == MatchState.Waiting && !isThisActive)
        {
            bl_UIReferences.Instance.SetWaitingPlayersText(string.Format(bl_GameTexts.WaitingPlayers, bl_PhotonNetwork.PlayerList.Length, 2), true);
            return;
        }
        if (bl_GameData.Instance.useCountDownOnStart)
        {
            bl_CountDownBase.Instance.StartCountDown();
            return;
        }
        else
        {
            SetTimeState(RoomTimeState.Started, true);
        }
#if LMS
        if (GetGameMode == GameMode.BR) return;
#endif
        GetTime(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void InitAfterWaiting()
    {
        GetTime(true);
        photonView.RPC(nameof(RpcStartTime), RpcTarget.AllBuffered, 3);
    }

    /// <summary>
    /// 
    /// </summary>
    void RemoteInit()
    {
        if (TimeState == RoomTimeState.StartedAfterCountdown)
        {
            SetTimeState(RoomTimeState.Started);
            GetTime(false);
            return;
        }

        if (bl_PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(PropertiesKeys.TimeState))
        {
            TimeState = (RoomTimeState)(int)bl_PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.TimeState];
            if (TimeState == RoomTimeState.Started)
            {
                GetTime(false);
            }
            SetTimeState(TimeState);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void InitAfterCountdown()
    {
        RoundDuration = (int)bl_PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey];
        m_serverTimeReference = (float)bl_PhotonNetwork.Time;
        if (bl_PhotonNetwork.IsMasterClient)
        {
            Hashtable startTimeProp = new Hashtable();
            startTimeProp.Add(StartTimeKey, m_serverTimeReference);
            bl_PhotonNetwork.CurrentRoom.SetCustomProperties(startTimeProp);
            SetTimeState(RoomTimeState.Started, true);
        }
        else
        {
            SetTimeState(RoomTimeState.StartedAfterCountdown);
            GetTime(false);
        }
        if (!IsInitialized) { bl_GameManager.Instance.SetGameState(MatchState.Playing); }
        IsInitialized = true;
        IsPaused = false;
    }

    /// <summary>
    /// get the current time and verify if it is correct
    /// </summary>
    public void GetTime(bool ResetReference)
    {
        if (bl_PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey] != null)
        {
            //get the time duration from the room properties 
            RoundDuration = (int)bl_PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey];
        }

        if (bl_PhotonNetwork.IsMasterClient)
        {
            if (ResetReference)//get the server time again?
            {
                m_serverTimeReference = (float)bl_PhotonNetwork.Time;//get a reference time from the server

                Hashtable startTimeProp = new Hashtable();//create a property to store the reference time
                startTimeProp.Add(StartTimeKey, m_serverTimeReference);
                bl_PhotonNetwork.CurrentRoom.SetCustomProperties(startTimeProp);//send to the room hash tables so other clients can access to it
            }
        }
        else//Non master clients
        {
            if (bl_PhotonNetwork.CurrentRoom.CustomProperties[StartTimeKey] != null)//if there's a reference time available
            {
                m_serverTimeReference = (float)bl_PhotonNetwork.CurrentRoom.CustomProperties[StartTimeKey];//get it from the room hash tables
            }
        }
        if (!IsInitialized) { bl_GameManager.Instance.SetGameState(MatchState.Playing); }//is this the first round?
        IsInitialized = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetTimeState(RoomTimeState state, bool sync = false)
    {
        // Debug.Log($"time state changed from {TimeState.ToString()} to {state.ToString()}");
        if (sync && bl_PhotonNetwork.IsMasterClient)
        {
            Hashtable table = new Hashtable();
            table.Add(PropertiesKeys.TimeState, (int)state);
            bl_PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
        TimeState = state;
        if (TimeState == RoomTimeState.Started)
        {
            bl_EventHandler.CallOnMatchStart();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        UpdateTime();
        DisplayTime();
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateTime()
    {
        if (!IsInitialized || IsPaused || m_timeIsUp || !isThisActive)
            return;

        //calculate seconds from the reference time and the current server time
        float seconds = RoundDuration - ((float)bl_PhotonNetwork.Time - m_serverTimeReference);
        if (seconds > 0.0001f)
        {
            CurrentTime = seconds;
            //if the game is about to finish, close the room so it will not be listed in the lobby anymore
            if (CurrentTime <= 30 && !roomClose && bl_PhotonNetwork.IsMasterClient)
            {
                roomClose = true;
                if (bl_RoomSettings.Instance.CurrentRoomInfo.roundStyle == RoundStyle.OneMacht)
                {
                   // PhotonNetwork.CurrentRoom.IsOpen = false;
                   // PhotonNetwork.CurrentRoom.IsVisible = false;
                }
            }
        }
        else if (seconds <= 0.001 && GetTimeServed == true)//Round Finished
        {
            CurrentTime = 0;
            FinishRound(RoundFinishCause.TimeUp);
        }
        else//if we cant get the server time yet
        {
            Refresh();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="doPause"></param>
    public override void Pause(bool doPause)
    {
        IsPaused = doPause;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void RestartTime()
    {
        //get the room time duration from hash tables
        if (bl_PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey] != null)
        {
            RoundDuration = (int)bl_PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey];
        }
        //cause everyone is already in the room, all will get the reference locally from the server
        m_serverTimeReference = (float)bl_PhotonNetwork.Time;
        //Master Client will take care of store the reference time for future players
        if (bl_PhotonNetwork.IsMasterClient)
        {
            Hashtable startTimeProp = new Hashtable();  // only use ExitGames.Client.Photon.Hashtable for Photon
            startTimeProp.Add(StartTimeKey, m_serverTimeReference);
            bl_PhotonNetwork.CurrentRoom.SetCustomProperties(startTimeProp);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="finishRoundHandler"></param>
    public override void SetFinishRoundHandler(Action<RoundFinishCause> finishRoundHandler)
    {
        finishRoundCallback = finishRoundHandler;
    }

    /// <summary>
    /// Call this in order to finish the match
    /// if the room is setup as one match room -> the game resume screen will appear.
    /// </summary>
    public override void FinishRound(RoundFinishCause cause)
    {
        // if the room time if finished but the countdown is still running, do not finish the match.
        if (TimeState == RoomTimeState.Countdown) return;
        
        if(finishRoundCallback != null && cause != RoundFinishCause.GameFinish)
        {
            // handle the finish round logic in the custom callback
            finishRoundCallback?.Invoke(cause);
            return;
        }

        if (!bl_PhotonNetwork.IsConnected || !isThisActive)
            return;

        bl_EventHandler.DispatchRoundEndEvent();
        Debug.Log($"Round Finish ({cause.ToString()}), mode: {bl_RoomSettings.Instance.CurrentRoomInfo.roundStyle.ToString()}");
        if (!m_timeIsUp)
        {
            m_timeIsUp = true;
            bl_GameManager.Instance.OnGameTimeFinish(bl_RoomSettings.Instance.CurrentRoomInfo.roundStyle == RoundStyle.OneMacht || cause == RoundFinishCause.GameFinish);

            // Save the gained XP in this game
            bl_UIReferences.Instance.ResumeScreen.CollectData();

            finalCountDown = bl_GameData.Instance.afterMatchCountdown;
            bl_RoundFinishScreenBase.Instance?.SetCountdown(finalCountDown);
            InvokeRepeating(nameof(FinalCountdown), 1, 1);
            bl_CrosshairBase.Instance.Show(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DisplayTime()
    {
        if (CurrentTime == 0 || !isThisActive) return;
        if (TimeText == null) return;

        TimeText.text = bl_StringUtility.GetTimeFormat(Mathf.FloorToInt(CurrentTime / SECOND), Mathf.FloorToInt(CurrentTime % SECOND));
    }

    /// <summary>
    /// with this fixed the problem of the time lag in the Photon
    /// </summary>
    void Refresh()
    {
        if (bl_PhotonNetwork.CurrentRoom == null)
            return;

        if (bl_PhotonNetwork.IsMasterClient)
        {
            m_serverTimeReference = (float)bl_PhotonNetwork.Time;

            var startTimeProp = new Hashtable();  // only use ExitGames.Client.Photon.Hashtable for Photon
            startTimeProp.Add(StartTimeKey, m_serverTimeReference);
            bl_PhotonNetwork.CurrentRoom.SetCustomProperties(startTimeProp);
        }
        else
        {
            if (bl_PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(StartTimeKey))
            {
                m_serverTimeReference = (float)bl_PhotonNetwork.CurrentRoom.CustomProperties[StartTimeKey];
            }
        }
    }

    /// <summary>
    /// Count down before leave the room after finish the game
    /// </summary>
    void FinalCountdown()
    {
        finalCountDown--;
        bl_RoundFinishScreenBase.Instance?.SetCountdown(finalCountDown);
        if (finalCountDown <= 0)
        {
            FinishGame();
            CancelInvoke(nameof(FinalCountdown));
            finalCountDown = bl_GameData.Instance.afterMatchCountdown;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void FinishGame()
    {
        bl_UtilityHelper.LockCursor(false);
        var roundStyle = bl_RoomSettings.Instance.CurrentRoomInfo.roundStyle;
        if (roundStyle == RoundStyle.OneMacht || roundStyle == RoundStyle.RoundsOneMatch)
        {
            bl_UIReferences.Instance.ResumeScreen.Show();
        }
        else if (roundStyle == RoundStyle.Rounds)
        {
            StartNewRound(new NewRoundOptions()
            {
                KeepData = false,
                RespawnPlayer = false
            });
        }
    }

    /// <summary>
    /// Start a new round and keep the player and team scores
    /// The time will be reset to the initial value
    /// </summary>
    public override void StartNewRound(NewRoundOptions options)
    {
        if (bl_GameData.Instance.useCountDownOnStart)
        {
            bl_CountDownBase.Instance.StartCountDown();
        }
        else
        {
            GetTime(true);
            IsPaused = false;
        }

        if (!options.KeepData)
        {
            bl_RoomSettings.Instance.ResetRoom();
            bl_GameManager.Instance.DestroyPlayer(true);
        }

        m_timeIsUp = false;
        bl_GameManager.Instance.GameFinish = false;
        bl_GameManager.Instance.IsLocalPlaying = false;
        bl_UtilityHelper.LockCursor(false);
        bl_KillCamBase.Instance.SetActive(false);
        finalCountDown = bl_GameData.Instance.afterMatchCountdown;
        bl_PhotonNetwork.CurrentRoom.IsOpen = true;
        bl_PhotonNetwork.CurrentRoom.IsVisible = true;
        bl_AIMananger.Instance?.RespawnAllBots(true);

        if (options.RespawnPlayer)
        {
            bl_GameManager.Instance.SpawnPlayer(bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());
        }

        if (!options.KeepData)
        {
            bl_UIReferences.Instance.ResetRound();

            if (bl_RoomSettings.Instance.AutoTeamSelection)
                bl_RoomSettings.Instance.CheckAutoSpawn();
        }
    }

    [PunRPC]
    void RpcStartTime(int wait)
    {
        if (bl_MFPS.LocalPlayer.Team == Team.None) return;

        SetTimeState(RoomTimeState.Started, bl_PhotonNetwork.IsMasterClient);
        bl_UIReferences.Instance.SetWaitingPlayersText(bl_GameTexts.StartingMatch, true);
        Invoke(nameof(StartTime), wait);
    }

    /// <summary>
    /// 
    /// </summary>
    void StartTime()
    {
        if (bl_GameData.Instance.useCountDownOnStart)
        {
            bl_CountDownBase.Instance.StartCountDown();
        }
        else
        {
            GetTime(false);
        }
        IsPaused = false;
        bl_UIReferences.Instance.SetWaitingPlayersText();
        bl_GameManager.Instance.SpawnPlayer(bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeText"></param>
    /// <param name="disableCurrent"></param>
    public override void SetTimeTextRender(TextMeshProUGUI timeText, bool disableCurrent = true)
    {
        if (disableCurrent)
        {
            if (TimeText != null) TimeText.gameObject.SetActive(false);
            else bl_UIReferences.Instance.PlayerUI.TimeText.gameObject.SetActive(false);                            
        }
        TimeText = timeText;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsTimeUp() => m_timeIsUp;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newMaster"></param>
    void OnMasterClientSwitch(Player newMaster)
    {
        if (newMaster.ActorNumber == bl_PhotonNetwork.LocalPlayer.ActorNumber)
        {
            if (TimeState == RoomTimeState.Countdown)
            {
                bl_CountDownBase.Instance.StartCountDown();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="props"></param>
    void OnRoomPropertiesChange(Hashtable props)
    {
        if (props.ContainsKey(StartTimeKey) && !bl_PhotonNetwork.IsMasterClient)
        {
            RemoteInit();
        }
    }

    private bool GetTimeServed => Time.timeSinceLevelLoad > 7;
}