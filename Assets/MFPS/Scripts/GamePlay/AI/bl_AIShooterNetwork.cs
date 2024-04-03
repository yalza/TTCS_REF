using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
using MFPS.Runtime.AI;

public sealed class bl_AIShooterNetwork : bl_MonoBehaviour, IPunObservable
{
    public NavMeshAgent Agent { get; set; }
    public Vector3 Velocity { get; set; }
    public Transform AimTarget { get; set; }

    private Transform m_Transform;
    private int receivePackages = 0;
    private Vector3 correctPlayerPos = Vector3.zero; // We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; // We lerp towards this
    private Vector3 networkLookAtPosition = Vector3.zero;

    private MFPSBotProperties BotStat = null;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_Transform = transform;
        Agent = References.Agent;

        bl_AIMananger.OnMaterStatsReceived += OnMasterStatsReceived;
        bl_AIMananger.OnBotStatUpdate += OnBotStatUpdate;

        GetEssentialData();
        if (photonView.IsMine)
        {
            OnLocalStart();
        }
        else
        {
            OnNetworkStart();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        RegisterPlayerSpawn();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_AIMananger.OnMaterStatsReceived -= OnMasterStatsReceived;
        bl_AIMananger.OnBotStatUpdate -= OnBotStatUpdate;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalStart()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    void OnNetworkStart()
    {
        References.aiShooter.Init();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_Transform.localPosition);
            stream.SendNext(m_Transform.localRotation);
            stream.SendNext(Agent.velocity);
            stream.SendNext(References.aiShooter.LookAtPosition);
        }
        else
        {
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
            Velocity = (Vector3)stream.ReceiveNext();
            networkLookAtPosition = (Vector3)stream.ReceiveNext();

            //Fix the translation effect on remote clients
            if (receivePackages < 5)
            {
                m_Transform.localPosition = correctPlayerPos;
                m_Transform.localRotation = correctPlayerRot;
                receivePackages++;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!bl_PhotonNetwork.IsMasterClient)//if not master client, then get position from server
        {
            m_Transform.localPosition = Vector3.Lerp(m_Transform.localPosition, correctPlayerPos, Time.deltaTime * 7);
            m_Transform.localRotation = Quaternion.Lerp(m_Transform.localRotation, correctPlayerRot, Time.deltaTime * 7);
            References.aiShooter.LookAtPosition = Vector3.Lerp(References.aiShooter.LookAtPosition, networkLookAtPosition, Time.deltaTime * 5);
        }
        else
        {
            Velocity = Agent.velocity;
            if (bl_MatchTimeManagerBase.Instance.IsTimeUp())
            {
                if(Agent.enabled) Agent.isStopped = true;
                return;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GetEssentialData()
    {
        object[] data = photonView.InstantiationData;
        AIName = (string)data[0];
        AITeam = (Team)data[1];
        gameObject.name = AIName;
        CheckNamePlate();
        //since Non master client doesn't update the view ID when bots are created, lets do it on Start
        if (!bl_PhotonNetwork.IsMasterClient)
        {
            bl_AIMananger.UpdateBotView(References.aiShooter, photonView.ViewID);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnMasterStatsReceived(List<MFPSBotProperties> stats)
    {
        ApplyMasterInfo(stats);
    }

    /// <summary>
    /// 
    /// </summary>
    void ApplyMasterInfo(List<MFPSBotProperties> stats)
    {
        int viewID = photonView.ViewID;
        MFPSBotProperties bs = stats.Find(x => x.ViewID == viewID);
        if (bs != null)
        {
            AIName = bs.Name;
            AITeam = bs.Team;
            gameObject.name = AIName;
            BotStat = new MFPSBotProperties();
            BotStat.Name = AIName;
            BotStat.Score = bs.Score;
            BotStat.Kills = bs.Kills;
            BotStat.Deaths = bs.Deaths;
            BotStat.ViewID = bs.ViewID;

            RegisterPlayerSpawn();

            CheckNamePlate();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stat"></param>
    void OnBotStatUpdate(MFPSBotProperties stat)
    {
        if (stat.ViewID != photonView.ViewID) return;

        BotStat = stat;
        AIName = stat.Name;
        AITeam = BotStat.Team;
        gameObject.name = AIName;

        RegisterPlayerSpawn();
    }

    /// <summary>
    /// 
    /// </summary>
    private void RegisterPlayerSpawn()
    {
        string name = string.IsNullOrEmpty(AIName) ? (string)photonView.InstantiationData[0] : AIName;
        bl_EventHandler.DispatchRemoteActorChange(new bl_EventHandler.PlayerChangeData()
        {
            PlayerName = name,
            MFPSActor = BuildPlayer(),
            IsAlive = true,
            NetworkView = photonView
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private MFPSPlayer BuildPlayer(bool isAlive = true)
    {
        MFPSPlayer player = new MFPSPlayer()
        {
            Name = AIName,
            ActorView = photonView,
            isRealPlayer = false,
            Actor = transform,
            AimPosition = AimTarget,
            Team = AITeam,
            isAlive = isAlive,
        };
        return player;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        bl_EventHandler.DispatchRemoteActorChange(new bl_EventHandler.PlayerChangeData()
        {
            PlayerName = AIName,
            MFPSActor = BuildPlayer(false),
            IsAlive = false,
        });
    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckNamePlate()
    {
        References.namePlateDrawer.SetName(AIName);
        if (!isOneTeamMode && bl_GameManager.Instance.LocalPlayer != null && !References.aiShooter.isDeath)
        {
            References.namePlateDrawer.SetActive(bl_MFPS.LocalPlayer.Team == AITeam);
        }
        else
        {
            References.namePlateDrawer.SetActive(false);
        }
    }

    private string AIName { get => References.aiShooter.AIName; set => References.aiShooter.AIName = value; }
    private Team AITeam { get => References.aiShooter.AITeam; set => References.aiShooter.AITeam = value; }

    private bl_AIShooterReferences _References;
    public bl_AIShooterReferences References
    {
        get
        {
            if (_References == null) _References = GetComponent<bl_AIShooterReferences>();
            return _References;
        }
    }
}