using UnityEngine;
using HashTable = ExitGames.Client.Photon.Hashtable;
using MFPS.Internal.Structures;
using Photon.Realtime;

public class bl_KillFeed : bl_KillFeedBase
{    
    public Color SelfColor = Color.green;

#if LOCALIZATION
    private int[] LocaleTextIDs = new int[] { 28,17, };
    private string[] LocaleStrings;
#endif
    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (bl_PhotonNetwork.InRoom)
        {
#if LOCALIZATION
            LocaleStrings = bl_Localization.Instance.GetTextArray(LocaleTextIDs);
#endif
            OnJoined();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_PhotonCallbacks.PlayerLeftRoom += OnPhotonPlayerDisconnected;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        bl_PhotonCallbacks.PlayerLeftRoom -= OnPhotonPlayerDisconnected;
    }

    /// <summary>
    /// Player Joined? sync
    /// </summary>
    void OnJoined()
    {
#if LOCALIZATION
        string joinCmd = bl_Localization.AsCommand("joinmatch");
        SendMessageEvent(string.Format("{0} {1}", bl_PhotonNetwork.LocalPlayer.NickName, joinCmd));
#else
        SendMessageEvent(string.Format("{0} {1}", bl_PhotonNetwork.LocalPlayer.NickName, bl_GameTexts.JoinedInMatch));
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SendKillMessageEvent(FeedData feedData)
    {
        HashTable data = new HashTable();
        data.Add("killer", feedData.LeftText);
        data.Add("killed", feedData.RightText);
        data.Add("gunid", (int)feedData.Data["gunid"]);
        data.Add("team", feedData.Team);
        data.Add("headshot", (bool)feedData.Data["headshot"]);
        data.Add("mt", KillFeedMessageType.WeaponKillEvent);
        SendMessageOverNetwork(data);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SendMessageEvent(string message, bool localOnly = false)
    {
        HashTable data = new HashTable();
        data.Add("message", message);
        data.Add("mt", KillFeedMessageType.Message);
        if (localOnly) { ReceiveMessage(data); return; }

        SendMessageOverNetwork(data);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SendTeamHighlightMessage(string teamHighlightMessage, string normalMessage, Team playerTeam)
    {
        HashTable data = new HashTable();
        data.Add("killer", teamHighlightMessage);
        data.Add("message", normalMessage);
        data.Add("team", playerTeam);
        data.Add("mt", KillFeedMessageType.TeamHighlightMessage);
        SendMessageOverNetwork(data);
    }

    /// <summary>
    /// 
    /// </summary>
    void SendMessageOverNetwork(HashTable data)
    {
        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.KillFeedEvent, data);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnMessageReceive(HashTable data)
    {
        KillFeedMessageType mtype = (KillFeedMessageType)data["mt"];
        switch (mtype)
        {
            case KillFeedMessageType.WeaponKillEvent:
                ReceiveWeaponKillEvent(data);
                break;
            case KillFeedMessageType.Message:
                ReceiveMessage(data);
                break;
            case KillFeedMessageType.TeamHighlightMessage:
                ReceiveOnePlayerMessage(data);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void ReceiveWeaponKillEvent(HashTable data)
    {
        KillFeed kf = new KillFeed();
        kf.Killer = (string)data["killer"];
        kf.Killed = (string)data["killed"];
        kf.GunID = (int)data["gunid"];
        kf.HeadShot = (bool)data["headshot"];
        kf.KillerTeam = (Team)data["team"];
        kf.messageType = KillFeedMessageType.WeaponKillEvent;

        bl_KillFeedUIBase.Instance.SetKillFeed(kf);
    }

    /// <summary>
    /// 
    /// </summary>
    void ReceiveMessage(HashTable data)
    {
        KillFeed kf = new KillFeed();
        kf.Message = (string)data["message"];
        kf.messageType = KillFeedMessageType.Message;

#if LOCALIZATION
        bl_Localization.Instance.ParseCommad(ref kf.Message);
#endif

        bl_KillFeedUIBase.Instance.SetKillFeed(kf);
    }

    /// <summary>
    /// 
    /// </summary>
    void ReceiveOnePlayerMessage(HashTable data)
    {
        KillFeed kf = new KillFeed();
        kf.Killer = (string)data["killer"];
        kf.Message = (string)data["message"];
        kf.KillerTeam = (Team)data["team"];
        kf.messageType = KillFeedMessageType.TeamHighlightMessage;

        bl_KillFeedUIBase.Instance.SetKillFeed(kf);
    }

    public void OnPhotonPlayerDisconnected(Player otherPlayer)
    {
#if LOCALIZATION
        SendMessageEvent(string.Format("{0} {1}", otherPlayer.NickName, bl_Localization.Instance.GetText(18)), true);
#else
        SendMessageEvent(string.Format("{0} {1}", otherPlayer.NickName, bl_GameTexts.LeftOfMatch), true);
#endif
    }
  
    /// <summary>
    /// 
    /// </summary>
    public override void AddCustomIcon(string keyName, Sprite icon)
    {
        if (customIcons.Exists(x => x.Name.Equals(keyName))) return;

        customIcons.Add(new CustomIcons()
        {
            Name = keyName,
            Icon = icon
        });
    }

    public override int GetCustomIconIndex(string keyName) => customIcons.FindIndex(x => x.Name == keyName);
    public override CustomIcons GetCustomIconByIndex(int index)
    {
        if (index > customIcons.Count - 1) return null;
        return customIcons[index];
    }

    /// <summary>
    /// 
    /// </summary>
    public override Sprite GetCustomIcon(string keyName)
    {
        Sprite spr = null;
        if (customIcons.Exists(x => x.Name == keyName))
        {
            spr = customIcons.Find(x => x.Name == keyName).Icon;
        }
        else { Debug.LogWarning($"Custom icon {keyName} has not been register in the custom icons list"); }

        return spr;
    }
}