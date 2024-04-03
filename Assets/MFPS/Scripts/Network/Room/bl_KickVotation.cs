using UnityEngine;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class bl_KickVotation : bl_MonoBehaviour
{
    [SerializeField] private KeyCode YesKey = KeyCode.F1;
    [SerializeField] private KeyCode NoKey = KeyCode.F2;

    private bool IsOpen = false;
    private int YesCount = 0;
    private int NoCount = 0;
    private bl_KickVotationUI UI;
    private Player TargetPlayer;
    private bool isAgainMy = false;
    private bool Voted = false;
    private int AllVoters = 0;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        UI = FindObjectOfType<bl_KickVotationUI>();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_PhotonCallbacks.PlayerLeftRoom += OnPhotonPlayerDisconnected;
        bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.VoteEvent, OnNetworkReceived);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_PhotonCallbacks.PlayerLeftRoom -= OnPhotonPlayerDisconnected;
        bl_PhotonNetwork.Instance?.RemoveCallback(OnNetworkReceived);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnNetworkReceived(Hashtable data)
    {
        CallType ct = (CallType)data["type"];
        switch(ct)
        {
            case CallType.VoteStart:
                VoteStart(data);
                break;
            case CallType.Vote:
                OnVote(data);
                break;
            case CallType.VoteEnd:
                EndVotation(data);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    public void RequestKick(Player player)
    {
        if (IsOpen || player == null)
            return;
        if(bl_PhotonNetwork.PlayerList.Length < 3)
        {
            Debug.Log("there are not enough players.");
            return;
        }
        if (player.ActorNumber == bl_PhotonNetwork.LocalPlayer.ActorNumber)
        {
            Debug.Log("You can not send a vote for yourself.");
            return;
        }

        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("type", CallType.VoteStart);
        data.Add("player", player);
        data.Add("by", bl_PhotonNetwork.LocalPlayer);
        bl_MFPS.Network.SendNetworkCall(PropertiesKeys.VoteEvent, data);
    }

    /// <summary>
    /// 
    /// </summary>
    void ResetVotation()
    {
        YesCount = 0;
        NoCount = 0;
        isAgainMy = false;
        Voted = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!IsOpen || isAgainMy || Voted)
            return;
        if (TargetPlayer == null)
            return;

        if (Input.GetKeyDown(YesKey))
        {
            SendVote(true);
            Voted = true;
        }
        else if (Input.GetKeyDown(NoKey))
        {
            SendVote(false);
            Voted = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="yes"></param>
    void SendVote(bool yes)
    {
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("type", CallType.Vote);
        data.Add("vote", yes);
        bl_MFPS.Network.SendNetworkCall(PropertiesKeys.VoteEvent, data);

        UI.OnSendLocalVote(yes);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnVote(Hashtable data)
    {
        var vote = (bool)data["vote"];
        if (vote)
        {
            YesCount++;
        }
        else
        {
            NoCount++;
        }
        UI.OnReceiveVote(YesCount, NoCount);
        if (bl_PhotonNetwork.IsMasterClient)//master count all votes to determine if kick or not
        {
            CountVotes();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void VoteStart(Hashtable data)
    {
        if (IsOpen)
            return;

        var player = (Player)data["player"];
        var by = (Player)data["by"];

        AllVoters = bl_PhotonNetwork.PlayerListOthers.Length;
        TargetPlayer = player;
        ResetVotation();
        isAgainMy = (player.ActorNumber == bl_PhotonNetwork.LocalPlayer.ActorNumber);
        UI.OpenVotatation(player, by);
        IsOpen = true;
    }

    /// <summary>
    /// 
    /// </summary>
    void EndVotation(Hashtable data)
    {
        var kicked = (bool)data["kick"];

        IsOpen = false;
        Voted = true;
        UI.OnFinish(kicked);
        TargetPlayer = null;
    }

    /// <summary>
    /// 
    /// </summary>
    void CountVotes()
    {
        int half = (AllVoters / 2);
        bool kicked = false;
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("type", CallType.VoteEnd);

        if (YesCount > half)//kick
        {
            bl_PhotonNetwork.Instance.KickPlayer(TargetPlayer);
            kicked = true;
        }

        data.Add("kick", kicked);
        bl_MFPS.Network.SendNetworkCall(PropertiesKeys.VoteEvent, data);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPhotonPlayerDisconnected(Player otherPlayer)
    {
        if (TargetPlayer == null)
            return;

       if(otherPlayer.ActorNumber == TargetPlayer.ActorNumber)
        {
            //cancel voting due player left the room by himself
            UI.OnFinish(true);
        }
    }

    public enum CallType
    {
        VoteStart = 0,
        Vote,
        VoteEnd
    }

    private static bl_KickVotation _instance;
    public static bl_KickVotation Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_KickVotation>(); }
            return _instance;
        }
    }
}