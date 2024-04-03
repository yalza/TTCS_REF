using UnityEngine;
using System;

[DefaultExecutionOrder(-800)]
public class bl_RoomMenu : bl_MonoBehaviour
{
    public enum ControlFocus
    {
        None,
        Chat,
        InputBinding
    }

    [Range(0.0f, 5)]
    public float DelayLeave = 1.5f;

    /// <summary>
    /// event called when a player enter but have to wait until current round finish.
    /// </summary>
    public Action<Team> onWaitUntilRoundFinish;

    /// <summary>
    /// Is currently the cursor locked?
    /// </summary>
    public bool isCursorLocked
    {
        get;
        private set;
    }

    /// <summary>
    /// Return which part of the game is currently using the Input/Control/Keyboard
    /// </summary>
    public ControlFocus CurrentControlFocus
    {
        get;
        set;
    } = ControlFocus.None;

    /// <summary>
    /// Is currently the game showing the pause menu?
    /// </summary>
    public bool isPaused
    {
        get => bl_PauseMenuBase.IsMenuOpen;
    }

    /// <summary>
    /// Is the game quitting?
    /// </summary>
    public bool isApplicationQuitting
    {
        get;
        private set;
    } = false;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!isConnected)
            return;

        base.Awake();
#if ULSP
        if (bl_DataBase.IsUserLogged) { bl_DataBase.Instance.RecordTime(); }
#endif
        bl_UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = false;
        bl_Input.CheckGamePadRequired();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        bl_EventHandler.onLocalPlayerSpawn += OnPlayerSpawn;
        bl_EventHandler.onLocalPlayerDeath += OnPlayerLocalDeath;
#if MFPSM
        bl_TouchHelper.OnPause += TogglePause;
#endif
        bl_PhotonCallbacks.LeftRoom += OnLeftRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        bl_EventHandler.onLocalPlayerSpawn -= OnPlayerSpawn;
        bl_EventHandler.onLocalPlayerDeath -= OnPlayerLocalDeath;
#if MFPSM
        bl_TouchHelper.OnPause -= TogglePause;
#endif
        bl_PhotonCallbacks.LeftRoom -= OnLeftRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnPlayerSpawn()
    {
        bl_UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = true;
        bl_GameManager.Instance.IsLocalPlaying = true;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnPlayerLocalDeath()
    {
        bl_UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = false;
        bl_GameManager.Instance.IsLocalPlaying = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        isCursorLocked = bl_UtilityHelper.GetCursorState;
        PauseControll();
        ScoreboardInput();
    }

    /// <summary>
    /// Check the input for open/hide the pause menu
    /// </summary>
    void PauseControll()
    {

        // Force lock cursor when press left Alt + left mouse button
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetMouseButtonDown(0))
            {
                bl_UtilityHelper.LockCursor(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bl_UtilityHelper.LockCursor(false);
        }

        if (CurrentControlFocus == ControlFocus.InputBinding) return;

        if (bl_GameInput.Pause()) TogglePause();
    }

    /// <summary>
    /// Toggle the current paused state
    /// </summary>
    public void TogglePause()
    {
        if (!bl_GameManager.Instance.FirstSpawnDone || bl_GameManager.Instance.GameFinish) return;

        bool paused = isPaused;
        paused = !paused;
        bl_UIReferences.Instance.ShowMenu(paused);
        bl_UtilityHelper.LockCursor(!paused);
        bl_CrosshairBase.Instance?.Show(!paused);
        bl_EventHandler.DispatchGamePauseEvent(paused);
    }

    /// <summary>
    /// 
    /// </summary>
    void ScoreboardInput()
    {
        if (bl_GameManager.Instance.GameFinish || bl_PauseMenuBase.IsMenuOpen) return;

        if (bl_GameInput.Scoreboard())
        {
            bl_PauseMenuBase.Instance.SetActiveLayouts(bl_PauseMenuBase.LayoutPart.Body);
            bl_PauseMenuBase.Instance.OpenWindow("scoreboard");
        }
        else if (bl_GameInput.Scoreboard(GameInputType.Up))
        {
            bl_PauseMenuBase.Instance.CloseWindow("scoreboard");
        }
    }

    /// <summary>
    /// Event called when the player will be automatically assigned to a team
    /// </summary>
    public void OnAutoTeam()
    {
        bl_UtilityHelper.LockCursor(true);
        bl_GameManager.Instance.IsLocalPlaying = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void JoinTeam(int id)
    {
        Team team = (Team)id;
        string tn = team.GetTeamName();
        string joinText = isOneTeamMode ? bl_GameTexts.JoinedInMatch : bl_GameTexts.JoinIn;

#if LOCALIZATION
        joinText = isOneTeamMode ? bl_Localization.Instance.GetText(17) : bl_Localization.Instance.GetText(23);
#endif
        if (isOneTeamMode)
        {
            bl_KillFeedBase.Instance.SendMessageEvent(string.Format("{0} {1}", bl_PhotonNetwork.NickName, joinText));
        }
        else
        {
            string jt = string.Format("{0} {1}", joinText, tn);
            bl_KillFeedBase.Instance.SendTeamHighlightMessage(bl_PhotonNetwork.NickName, jt, team);
        }
#if !PSELECTOR
        bl_UtilityHelper.LockCursor(true);
#else
        if (!bl_PlayerSelector.InMatch)
        {
            bl_UtilityHelper.LockCursor(true);
        }
#endif
        //if player only spawn when a new round start
        if (GetGameMode.GetGameModeInfo().onRoundStartedSpawn == GameModeSettings.OnRoundStartedSpawn.WaitUntilRoundFinish && bl_GameManager.Instance.GameMatchState == MatchState.Playing)
        {
            //subscribe to the start round event
            if (onWaitUntilRoundFinish != null) { onWaitUntilRoundFinish.Invoke(team); }
            bl_GameManager.Instance.SetLocalPlayerToTeam(team);//set the player to the selected team but not spawn yet.
            return;
        }
        //set the player to the selected team and spawn the player
        bl_GameManager.Instance.SpawnPlayer(team);
    }

    /// <summary>
    /// Leave the current room (if exist) and return to the lobby
    /// </summary>
    public void LeaveRoom(bool save = true)
    {
#if ULSP
        if (bl_DataBase.IsUserLogged && save)
        {
            var p = bl_PhotonNetwork.LocalPlayer;
            bl_ULoginMFPS.SaveLocalPlayerKDS();
            bl_DataBase.Instance.StopAndSaveTime();
        }
#endif
        //Good place to save info before reset statistics
        if (bl_PhotonNetwork.IsConnected && bl_PhotonNetwork.InRoom)
        {
            bl_PhotonNetwork.CleanBuffer();
            bl_PhotonNetwork.LeaveRoom();
        }
        else
        {
#if UNITY_EDITOR
            if (isApplicationQuitting) { return; }
#endif
            bl_UtilityHelper.LoadLevel(bl_GameData.Instance.MainMenuScene);
        }
    }

    /// <summary>
    /// Called from the server when the left room request was retrieved.
    /// </summary>
    public void OnLeftRoom()
    {
        Debug.Log("Local client left the room");
        bl_RoomCameraBase.Instance?.SetActive(true);
        bl_PhotonNetwork.IsMessageQueueRunning = false;
        bl_MatchTimeManagerBase.Instance.enabled = false;
        if (bl_UIReferences.Instance != null)
            StartCoroutine(bl_UIReferences.Instance.FinalFade(true));
    }

    /// <summary>
    /// 
    /// </summary>
    void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    private static bl_RoomMenu _instance;
    public static bl_RoomMenu Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_RoomMenu>(); }
            return _instance;
        }
    }
}