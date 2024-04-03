using System;
using TMPro;

public abstract class bl_MatchTimeManagerBase : bl_MonoBehaviour
{

    /// <summary>
    ///  
    /// </summary>
    public enum RoundFinishCause
    {
        TimeUp,
        GameModeLogic,
        GameFinish,
    }

    /// <summary>
    /// Option to pass when the <see cref="StartNewRound(NewRoundOptions)"/> function is called
    /// </summary>
    public struct NewRoundOptions
    {
        /// <summary>
        /// The current room data?
        /// (Team score, player scores, etc...)
        /// </summary>
        public bool KeepData
        {
            get;
            set;
        }

        /// <summary>
        /// Automatically respawn the local player?
        /// </summary>
        public bool RespawnPlayer
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keepData"></param>
        public NewRoundOptions(bool keepData)
        {
            KeepData = keepData;
            RespawnPlayer = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract RoomTimeState TimeState
    {
        get;
        set;
    }

    /// <summary>
    /// Current time in seconds
    /// </summary>
    public abstract float CurrentTime
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract int RoundDuration
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract bool IsInitialized
    {
        get;
        set;
    }

    /// <summary>
    /// Initialize the time manager
    /// </summary>
    public abstract void Init();

    /// <summary>
    /// Initialize the time manager after been waiting for the players.
    /// </summary>
    public abstract void InitAfterWaiting();

    /// <summary>
    /// Initialize the time manager after a count down
    /// </summary>
    public abstract void InitAfterCountdown();

    /// <summary>
    /// Active/Disable the script not the game object
    /// You can use for example when you want to handle the match time in a different script
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActive(bool active);

    /// <summary>
    /// Change the time state
    /// </summary>
    /// <param name="state"></param>
    /// <param name="syncOnAllClients">Replicate the state on all clients</param>
    public abstract void SetTimeState(RoomTimeState state, bool syncOnAllClients = false);

    /// <summary>
    /// Use to use an custom Finish Round function instead of the default generic one
    /// </summary>
    /// <param name="finishRoundHandler"></param>
    public abstract void SetFinishRoundHandler(Action<RoundFinishCause> finishRoundHandler);

    /// <summary>
    /// Finish the current round
    /// if the room is one round only this will finish the match.
    /// </summary>
    public abstract void FinishRound(RoundFinishCause cause = RoundFinishCause.GameModeLogic);

    /// <summary>
    /// Restart the round time
    /// </summary>
    public abstract void RestartTime();

    /// <summary>
    /// Pause the time
    /// </summary>
    /// <param name="doPause"></param>
    public abstract void Pause(bool doPause);

    /// <summary>
    /// Restart the time and respawn the player
    /// </summary>
    /// <param name="keepData">Reset the player properties or keep them?</param>
    public abstract void StartNewRound(NewRoundOptions options);

    /// <summary>
    /// Override the default text UI
    /// </summary>
    /// <param name="timeText"></param>
    public abstract void SetTimeTextRender(TextMeshProUGUI timeText, bool disableCurrent = true);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract bool IsTimeUp();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool HaveTimeStarted() => Instance.TimeState == RoomTimeState.Started;

    /// <summary>
    /// 
    /// </summary>
    private static bl_MatchTimeManagerBase _instance;
    public static bl_MatchTimeManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_MatchTimeManagerBase>(); }
            return _instance;
        }
    }
}