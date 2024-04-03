using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using MFPS.Runtime.AI;

[Serializable]
public class MFPSPlayer
{
    public string Name;
    public Transform Actor;
    public bool isRealPlayer = true;
    public bool isAlive = true;
    public Team Team = Team.None;
    public Transform AimPosition;

    public PhotonView m_actorView;
    /// <summary>
    /// Photon View of this actor
    /// </summary>
    public PhotonView ActorView
    {
        get => m_actorView;
        set
        {
            m_actorView = value;
            if (m_actorView != null) ActorViewID = m_actorView.ViewID;
        }
    }

    private int m_viewID;
    /// <summary>
    /// Photon View ID of this actor
    /// </summary>
    public int ActorViewID
    {
        get => m_viewID;
        set => m_viewID = value;
    }

    /// <summary>
    /// Photon Player Actor Number of this actor
    /// </summary>
    public int ActorNumber
    {
        get
        {
            if (ActorView == null || ActorView.Owner == null) return -1;
            return ActorView.Owner.ActorNumber;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public MFPSBotProperties GetBotStats()
    {
        MFPSBotProperties botProps = bl_AIMananger.Instance.GetBotStatistics(Name);
        if (botProps == null)
        {
            Debug.LogWarning($"Bot {Name} not found in AI Manager, this could be a de-sync issue.");
            //return a empty container to prevent any error
            botProps = new MFPSBotProperties();
            botProps.Name = Name;
        }
        return botProps;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Player GetNetworkPlayer()
    {
        if (ActorView != null) return ActorView.Owner;
        else
        {
            var playerList = bl_PhotonNetwork.PlayerList;
            for (int i = 0; i < playerList.Length; i++)
            {
                if (playerList[i].NickName == Name) return playerList[i];
            }
            return null;
        }      
    }

    /// <summary>
    /// Get a real player or bot property value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public object GetPlayerPropertie(string key, object defaultValue = null)
    {
        switch (key)
        {
            case PropertiesKeys.KillsKey:
                if (isRealPlayer) return GetNetworkPlayer().GetKills();
                else return GetBotStats().Kills;
            case PropertiesKeys.DeathsKey:
                if (isRealPlayer) return GetNetworkPlayer().GetDeaths();
                else return GetBotStats().Deaths;
            case PropertiesKeys.ScoreKey:
                if (isRealPlayer) return GetNetworkPlayer().GetPlayerScore();
                else return GetBotStats().Score;
            default:
                Debug.LogWarning($"Property {key} has not been setup yet.");
                return defaultValue;
        }
    }

    #region Constructors    
    public MFPSPlayer() { }

    public MFPSPlayer(PhotonView view, bool realPlayer = true, bool alive = true)
    {
        BuildFromView(view, realPlayer, alive);
    }

    public MFPSPlayer BuildFromView(PhotonView view, bool realPlayer = true, bool alive = true)
    {
        isRealPlayer = realPlayer;
        isAlive = alive;
        if (view == null) return this;

        Actor = view.transform;
        ActorView = view;
        AimPosition = Actor;
        if (view.InstantiationData != null)
        {
            if (!isRealPlayer)
            {
                Name = (string)view.InstantiationData[0];
                Team = (Team)view.InstantiationData[1];
            }
            else
            {
                Name = view.Owner.NickName;
                Team = (Team)view.InstantiationData[0];
            }
        }
        return this;
    }
    #endregion
}