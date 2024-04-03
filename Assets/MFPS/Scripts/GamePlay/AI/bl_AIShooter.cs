using UnityEngine;
using MFPS.Runtime.AI;
using UnityEngine.AI;

public abstract class bl_AIShooter : bl_MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public AIAgentState AgentState
    {
        get;
        set;
    } = AIAgentState.Idle;

    /// <summary>
    /// 
    /// </summary>
    public Transform Target
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual Transform AimTarget
    {
        get;
        protected set;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract float CachedTargetDistance
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public AILookAt LookingAt 
    { 
        get; 
        set; 
    } = AILookAt.Path;

    /// <summary>
    /// 
    /// </summary>
    public bool isDeath
    {
        get;
        set;
    } = false;

    /// <summary>
    /// 
    /// </summary>
    public Team AITeam
    {
        get;
        set;
    } = Team.None;

    /// <summary>
    /// 
    /// </summary>
    public bool FocusOnSingleTarget 
    { 
        get;
        set; 
    }

    /// <summary>
    /// 
    /// </summary>
    public bool isTeamMate
    {
        get
        {
            return (AITeam == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam() && !isOneTeamMode);
        }
    }

    public virtual NavMeshAgent Agent
    {
        get;
        set;
    }

    private string _AIName = string.Empty;
    public string AIName
    {
        get
        {
            return _AIName;
        }
        set
        {
            _AIName = value;
            gameObject.name = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract void Init();

    /// <summary>
    /// 
    /// </summary>
    public virtual void OnDeath() { }

    /// <summary>
    /// 
    /// </summary>
    public virtual void CheckTargets() { }

    /// <summary>
    /// 
    /// </summary>
    public abstract void Respawn();

    /// <summary>
    /// 
    /// </summary>
    public abstract void UpdateTargetList();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromPosition"></param>
    public abstract void OnGetHit(Vector3 fromPosition);

    /// <summary>
    /// 
    /// </summary>
    public virtual Vector3 TargetPosition
    {
        get => Target.position;
    }

    /// <summary>
    /// 
    /// </summary>
    private Vector3 m_lookAtDirection = Vector3.forward;
    public virtual Vector3 LookAtDirection
    {
        get => m_lookAtDirection;
        set => m_lookAtDirection = value;
    }

    /// <summary>
    /// 
    /// </summary>
    private Vector3 m_lookAtPosition = Vector3.forward;
    public virtual Vector3 LookAtPosition
    {
        get => m_lookAtPosition;
        set => m_lookAtPosition = value;
    }

    private bl_AIShooterReferences _references;
    public bl_AIShooterReferences References
    {
        get
        {
            if (_references == null) _references = GetComponent<bl_AIShooterReferences>();
            return _references;
        }
    }

}