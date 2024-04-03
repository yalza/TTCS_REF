using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using MFPS.Internal;
using MFPS.Runtime.UI;
using MFPS.Internal.Interfaces;
using MFPS.Internal.Structures;
using TMPro;
using MFPS.InputManager;

public class bl_UIReferences : bl_PhotonHelper, IInRoomCallbacks
{
    [FlagEnum, SerializeField] private RoomUILayers m_uiMask = 0;
    public RoomUILayers UIMask
    {
        get => m_uiMask;
        set => m_uiMask = value;
    }

    public IMFPSResumeScreen ResumeScreen { get; set; }

    [Header("References")]
    public bl_PlayerScoreboardBase playerScoreboards;
    [SerializeField] private GameObject SuicideButton = null;
    [SerializeField] private GameObject AutoTeamUI = null;
    [SerializeField] private GameObject SpectatorButton = null;
    [SerializeField] private GameObject ChangeTeamButton = null;
    [SerializeField] private TextMeshProUGUI SpawnProtectionText = null;
    [SerializeField] private TextMeshProUGUI RoomNameText = null;
    [SerializeField] private TextMeshProUGUI GameModeText = null;
    [SerializeField] private TextMeshProUGUI MaxPlayerText = null;
    [SerializeField] private TextMeshProUGUI MaxKillsText = null;
    [SerializeField] private TextMeshProUGUI WaitingPlayersText = null;
    public bl_CanvasGroupFader blackScreen;
    public bl_ConfirmationWindow leaveRoomConfirmation;
    public GameObject[] addonReferences;

    private bool inTeam = false;
    private int ChangeTeamTimes = 0;
#if LOCALIZATION
    private int[] LocaleTextIDs = new int[] { 126, 22, 38, 32, 33, 34, 127 };
    private string[] LocaleStrings;
#endif

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (!bl_PhotonNetwork.IsConnected || !bl_PhotonNetwork.InRoom)
            return;

        PhotonNetwork.AddCallbackTarget(this);
        blackScreen.FadeOut(1);

        GetRoomInfo();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        SetUpUI();

        bl_EventHandler.onLocalPlayerSpawn += OnPlayerSpawn;
        bl_EventHandler.onPickUpHealth += OnPicUpMedKit;
        bl_EventHandler.onLocalPlayerDeath += OnLocalPlayerDeath;
        bl_EventHandler.onRoundEnd += OnRoundFinish;
        bl_PhotonCallbacks.LeftRoom += OnLeftRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUpJoinButtons(bool forced = false)
    {
        if (!inTeam && (CanSelectTeamToJoin() || forced))
        {
            playerScoreboards.SetActiveByTeamMode(true);
        }
        else { playerScoreboards.SetActiveJoinButtons(false); }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
        bl_EventHandler.onLocalPlayerSpawn -= OnPlayerSpawn;
        bl_EventHandler.onPickUpHealth -= OnPicUpMedKit;
        bl_EventHandler.onLocalPlayerDeath -= OnLocalPlayerDeath;
        bl_EventHandler.onRoundEnd -= OnRoundFinish;
        bl_PhotonCallbacks.LeftRoom -= OnLeftRoom;
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalPlayerDeath()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    void OnRoundFinish()
    {
        playerScoreboards.SetActiveByTeamMode();
        StopAllCoroutines();
        blackScreen.SetAlpha(0);
    }

    /// <summary>
    /// 
    /// </summary>
    void GetRoomInfo()
    {
        GameMode mode = GetGameMode;
        RoomNameText.text = PhotonNetwork.CurrentRoom.Name.ToUpper();
        GameModeText.text = mode.GetName().ToUpper();
        int vs = (!isOneTeamMode) ? PhotonNetwork.CurrentRoom.MaxPlayers / 2 : PhotonNetwork.CurrentRoom.MaxPlayers - 1;
        MaxPlayerText.text = (!isOneTeamMode) ? string.Format("{0} VS {1}", vs, vs) : string.Format("1 VS {0}", vs);       
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUpUI()
    {
#if LOCALIZATION
        if (LocaleStrings == null) LocaleStrings = bl_Localization.Instance.GetTextArray(LocaleTextIDs);
#endif
        if (!bl_RoomSettings.Instance.CurrentRoomInfo.autoTeamSelection)
        {
            bl_PauseMenuBase.Instance.OpenMenu();
        }
       
        playerScoreboards.gameObject.SetActive(true);
        SetActiveChangeTeamButton(false);
        PlayerUI.SpeakerIcon.SetActive(false);
        SetUpJoinButtons();
        UpdateDisplayUI();
        SuicideButton.SetActive(bl_RoomSettings.Instance.canSuicide && bl_GameManager.Instance.IsLocalPlaying);

        if (bl_PhotonNetwork.IsConnected)
        {
            if (MaxKillsText != null)
            {
                int MaxKills = (int)bl_PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.RoomGoal];
#if LOCALIZATION
                MaxKillsText.text = string.Format("{0} {1}", MaxKills, LocaleStrings[0]);
#else
            MaxKillsText.text = string.Format("{0} KILLS", MaxKills);
#endif
            }
        }
#if LMS
        if (GetGameMode == GameMode.BR) ShowMenu(false);
#endif
        if (bl_MFPS.GameData.UsingWaitingRoom())
        {
            if (bl_PhotonNetwork.OfflineMode && !bl_PhotonNetwork.CurrentRoom.GetRoomInfo().autoTeamSelection) return;

            SetUpJoinButtons(true);
            SpectatorButton.SetActive(false);
            ShowMenu(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowMenu(bool active)
    {
        if (active) bl_PauseMenuBase.Instance.OpenMenu();
        else bl_PauseMenuBase.Instance.CloseMenu();

        SetActiveChangeTeamButton(bl_GameData.Instance.CanChangeTeam && !isOneTeamMode && ChangeTeamTimes <= bl_GameData.Instance.MaxChangeTeamTimes 
            && !bl_RoomSettings.Instance.AutoTeamSelection && bl_MatchTimeManagerBase.HaveTimeStarted() && bl_SpectatorModeBase.Instance.CanSelectTeam());

        SuicideButton.SetActive(bl_RoomSettings.Instance.canSuicide && bl_GameManager.Instance.IsLocalPlaying);

        if (!active)
        {
#if PSELECTOR
            if (bl_PlayerSelector.Instance != null)
            {
                bl_PlayerSelector.Instance.isChangeOfTeam = false;
            }
#endif
        }

        if (bl_Input.isGamePad) bl_GamePadPointer.Instance?.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUIMask(RoomUILayers newMask)
    {
        UIMask = newMask;
        UpdateDisplayUI();
        bl_EventHandler.DispatchUIMaskChange(newMask);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Resume()
    {
        if (!bl_GameManager.Instance.FirstSpawnDone) return;

        bl_UtilityHelper.LockCursor(true);
        ShowMenu(false);
        bl_CrosshairBase.Instance.Show(true);
        bl_EventHandler.DispatchGamePauseEvent(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateDisplayUI()
    {
        PlayerUI?.UpdateUIDisplay();
        if (playerScoreboards != null)
        {
            playerScoreboards.SetActive(UIMask.IsEnumFlagPresent(RoomUILayers.Scoreboards));
            playerScoreboards.BlockScoreboards = !UIMask.IsEnumFlagPresent(RoomUILayers.Scoreboards);
            playerScoreboards.SetActive(UIMask.IsEnumFlagPresent(RoomUILayers.Scoreboards), CanSelectTeamToJoin());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoTeam(bool v)
    {
        AutoTeamUI.SetActive(v);
        if (!v)
        {
            bl_PauseMenuBase.Instance.CloseMenu();
            bl_SpectatorModeBase.Instance.SetActiveUI(false);
            SpectatorButton.SetActive(false);
            inTeam = true;
        }
        else
        {
            if (bl_PhotonNetwork.OfflineMode)
            {
                AutoTeamUI.GetComponentInChildren<TextMeshProUGUI>().text = bl_GameTexts.StartingOfflineRoom;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LeftRoom(bool quit)
    {
        if (!quit)
        {
            bl_UtilityHelper.LockCursor(false);
            leaveRoomConfirmation.AskConfirmation(bl_GameTexts.LeaveMatchWarning.Localized("areusulega"), () => { LeftRoom(true); }, () => {  });
        }
        else
        {
            bl_RoomMenu.Instance.LeaveRoom();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Suicide()
    {
        if (!bl_MFPS.LocalPlayer.Suicide())
        {
            bl_EventHandler.DispatchGamePauseEvent(false);
        }
        bl_UtilityHelper.LockCursor(true);
        ShowMenu(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void JoinTeam(int id)
    {
#if ELIM
        if (GetGameMode == GameMode.ELIM)
        {
            if (bl_GameManager.Instance.GameMatchState == MatchState.Playing)
            {
                bl_Elimination.Instance.WaitForRoundFinish();
            }
        }
#endif
        bl_RoomMenu.Instance.JoinTeam(id);
        bl_PauseMenuBase.Instance.CloseMenu();
        inTeam = true;
        AutoTeamUI.SetActive(false);
        bl_SpectatorModeBase.Instance.SetActiveUI(false);
        SpectatorButton.SetActive(false);

        if (bl_GameManager.Joined) { ChangeTeamTimes++; }
        if (bl_GameData.Instance.CanChangeTeam && !isOneTeamMode && ChangeTeamTimes <= bl_GameData.Instance.MaxChangeTeamTimes && bl_MatchTimeManagerBase.HaveTimeStarted())
        {
            SetActiveChangeTeamButton(true);
        }
    }

    /// <summary>
    /// Called from the Change Team button
    /// </summary>
    public void ActiveChangeTeam()
    {
#if PSELECTOR
        if (bl_PlayerSelector.Instance != null)
        {
            bl_PlayerSelector.Instance.isChangeOfTeam = true;
        }
#endif
        playerScoreboards.SetActive(false, true);
        SetActiveChangeTeamButton(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void SetActiveChangeTeamButton(bool active)
    {
        if (ChangeTeamButton == null) return;

        ChangeTeamButton.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void ShowScoreboard(bool active)
    {
        playerScoreboards.SetActive(active);
    }

    public void OnSpawnCount(int count)
    {
#if LOCALIZATION
        SpawnProtectionText.text = string.Format(LocaleStrings[6].ToUpper(), count);
#else
        SpawnProtectionText.text = string.Format("SPAWN PROTECTION DISABLE IN: <b>{0}</b>", count);
#endif
        SpawnProtectionText.gameObject.SetActive(count > 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    void OnPlayerSpawn()
    {
        if (bl_Input.isGamePad)
        {
            bl_GamePadPointer.Instance?.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t_amount"></param>
    void OnPicUpMedKit(int Amount) => new MFPSLocalNotification(string.Format("+{0} Health", Amount));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="show"></param>
    public void SetWaitingPlayersText(string text = "", bool show = false)
    {
        WaitingPlayersText.text = text;
        WaitingPlayersText.transform.parent.gameObject.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetRound()
    {
        bl_RoundFinishScreenBase.Instance?.Hide();
        inTeam = false;
        blackScreen.FadeOut(1);
        SetUpUI();
        ShowMenu(true);
        if (!bl_RoomSettings.Instance.AutoTeamSelection)
        {
            playerScoreboards.ResetJoinButtons();
            SetUpJoinButtons();
        }
    }

    public void SetActiveTimeUI(bool active)
    {
        PlayerUI.TimeUIRoot.SetActive(active);
    }

    public void SetActiveMaxKillsUI(bool active) => PlayerUI.MaxKillsUI.SetActive(active);

    public IEnumerator FinalFade(bool fadein, bool goToLobby = true, float delay = 1)
    {
        if (!goToLobby)
        {
            if (bl_GameManager.Instance.GameFinish) { blackScreen.SetAlpha(0); yield break; }
        }

        if (fadein)
        {
            yield return new WaitForSeconds(delay);
            yield return blackScreen.FadeIn(1);
#if ULSP
            if (bl_DataBase.Instance != null && bl_DataBase.Instance.IsRunningTask)
            {
                while (bl_DataBase.Instance.IsRunningTask) { yield return null; }
            }
#endif
            if (goToLobby)
            {
                bl_UtilityHelper.LoadLevel(bl_GameData.Instance.MainMenuScene);
            }
        }
        else
        {
            yield return new WaitForSeconds(delay);
            blackScreen.FadeOut(1);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool CanSelectTeamToJoin()
    {
        return !inTeam && (!bl_RoomSettings.Instance.AutoTeamSelection || !bl_MFPS.GameData.UsingWaitingRoom());
    }

    #region Photon Callbacks
    public void OnLeftRoom()
    {
        ShowMenu(false);
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
    } 
    #endregion

    #region Getters
    private static bl_UIReferences _instance;
    public static bl_UIReferences Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_UIReferences>(); }
            return _instance;
        }
    }

    private bl_PlayerUIBank _PlayerBank;
    public bl_PlayerUIBank PlayerUI
    {
        get
        {
            if (_PlayerBank == null) { _PlayerBank = transform.GetComponentInChildren<bl_PlayerUIBank>(true); }
            return _PlayerBank;
        }
    }
    #endregion
}