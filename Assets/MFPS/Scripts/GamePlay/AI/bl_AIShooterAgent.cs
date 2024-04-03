using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;
using NetHashTable = ExitGames.Client.Photon.Hashtable;
using MFPS.Runtime.AI;
using MFPSEditor;

[RequireComponent(typeof(NavMeshAgent))]
public class bl_AIShooterAgent : bl_AIShooter
{
    #region Public Members
    [Space(5)]
    [ScriptableDrawer] public bl_AIBehaviorSettings behaviorSettings;
    [ScriptableDrawer] public bl_AISoldierSettings soldierSettings;

    [Header("Others")]
    public LayerMask ObstaclesLayer;
    public bool DebugStates = false;

    [Header("References")]
    public Transform aimTarget;
    public bl_Footstep footstep;
    #endregion

    #region Public Properties
    public override Transform AimTarget
    {
        get => aimTarget;
    }
    public bool playerInFront { get; set; }
    public bool ObstacleBetweenTarget { get; set; }
    public bl_AIShooterAttackBase AIWeapon { get; set; }
    public bl_PlayerHealthManagerBase AIHealth { get; set; }
    public override float CachedTargetDistance { get; set; } = 0;
    public string DebugLine { get; set; }//last ID 38 
    public bool IsCrouch { get; set; } = false;
    #endregion

    #region Private members
    private Animator Anim;
    private Vector3 finalPosition;
    private float lastPathTime = 0;
    private bl_AICoverPoint CoverPoint = null;
    private bool ForceCoverFire = false;
    private float CoverTime = 0;
    private Vector3 LastHitDirection;
    private int SwitchCoverTimes = 0;
    private float lookTime, lastHoldPositionTime = 0;
    private bool randomOnStartTake = false;
    private bool AllOrNothing = false;
    private GameMode m_GameMode;
    private float time, delta = 0;
    private Transform m_Transform;
    private float nextEnemysCheck = 0;
    private bool isGameStarted = false;
    private Vector3 targetDirection = Vector3.zero;
    private RaycastHit obsRay;
    private float lastDestinationTime = 0;
    private float velocityMagnitud = 0;
    private bool forceUpdateRotation = false;
    private Vector3 localRotation = Vector3.zero;
    private int[] animationHash = new int[] { 0 };
    private bool wasTargetInSight = false;
    public const string RPC_NAME = "RPCShooterBot";
    private int precisionRate = 4;
    private List<Transform> availableTargets = new List<Transform>();
    private Transform headBoneReference;
    public bool isMaster = false;
    private float lastCrouchTime = 0;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_Transform = transform;
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
        Agent = References.Agent;
        AIHealth = References.shooterHealth;
        AIWeapon = References.shooterWeapon;
        Anim = References.PlayerAnimator;
        ObstacleBetweenTarget = false;
        m_GameMode = GetGameMode;
        Agent.updateRotation = false;
        animationHash[0] = Animator.StringToHash("Crouch");
        bl_AIMananger.SetBotGameState(this, BotGameState.Playing);
        headBoneReference = References.aiAnimation.GetHumanBone(HumanBodyBones.Head);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Init()
    {
        isMaster = bl_PhotonNetwork.IsMasterClient;
        isGameStarted = bl_MatchTimeManagerBase.Instance.TimeState == RoomTimeState.Started;
        References.shooterNetwork.CheckNamePlate();
        References.Agent.enabled = isMaster;
    }
    
    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {  
        time = Time.time;
        delta = Time.deltaTime;
        if (isDeath) return;
        isNewDebug = true;

        if (!isGameStarted) return;

        if (Target != null && isMaster)
        {
            if (AgentState != AIAgentState.Covering)
            {
                if (AgentState != AIAgentState.HoldingPosition)
                {
                    TargetControll();
                }
                else
                {
                    OnHoldingPosition();
                }
            }
            else
            {
                OnCovering();
            }
        }
        LookAtControl();
    }

    /// <summary>
    /// this is called one time each second instead of each frame
    /// </summary>
    public override void OnSlowUpdate()
    {
        if (isDeath) return;
        if (bl_MatchTimeManagerBase.Instance.IsTimeUp() || !isGameStarted)
        {
            return;
        }

        velocityMagnitud = Agent.velocity.magnitude;

        if (isMaster)
        {
            if (!HasATarget)
            {
                SetDebugState(-1, true);
                //Get the player nearest player
                SearchPlayers();
                //if target null yet, then patrol         
                RandomPatrol(!isOneTeamMode);
            }
            else
            {
                CheckEnemysDistances();
                CheckVision();
            }
        }

        FootStep();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnAgentStateChanged(AIAgentState from, AIAgentState to)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    private void OnTargetChanged(Transform from, Transform to)
    {

    }

    /// <summary>
    /// Called when the bot not direct vision to the target -> have direct vision to the target and vice versa
    /// </summary>
    /// <param name="from">seeing?</param>
    /// <param name="to">seeing?</param>
    private void OnTargetLineOfSightChanged(bool from, bool to)
    {
        if (from == true)//the player lost the line of vision with the target
        {
            if (HasATarget && TargetDistance > 5)//he lost the target but not because it is death.
                Invoke(nameof(CorrectLookAt), 3);//if after 3 second of loss the target, still now found it -> don't look at it (trough walls for example)
        }
        else//player now has direct vision to the target
        {
            if (AgentState == AIAgentState.Following)
                CancelInvoke(nameof(CorrectLookAt));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CorrectLookAt()
    {
        SetLookAtState(AILookAt.PathToTarget);
    }

    /// <summary>
    /// 
    /// </summary>
    void TargetControll()
    {
        if (Time.frameCount % precisionRate != 0) return;

        CachedTargetDistance = bl_UtilityHelper.Distance(Target.position, m_Transform.localPosition);
        if (CachedTargetDistance >= soldierSettings.limitRange)
        {
            WhenTargetOutOfRange();
        }
        else if (CachedTargetDistance > soldierSettings.closeRange && CachedTargetDistance < soldierSettings.mediumRange)
        {
            WhenTargetOnMediumRange();
        }
        else if (CachedTargetDistance <= soldierSettings.closeRange)
        {
            WhenTargetOnCloseRange();
        }
        else if (CachedTargetDistance < soldierSettings.limitRange)
        {
            WhenTargetOnLimitRange();
        }
        else
        {
            Debug.Log("Unknown state: " + CachedTargetDistance);
            SetDebugState(101, true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void WhenTargetOutOfRange()
    {
        if (behaviorSettings.targetOutRangeBehave == AITargetOutRangeBehave.SearchNewNearestTarget)
        {
            var newTarget = GetNearestPlayer();
            if (newTarget != Target)
            {
                SetTarget(newTarget);
                return;
            }
        }
        precisionRate = 8;
        if (AgentState == AIAgentState.Following || FocusOnSingleTarget)
        {
            if (!isOneTeamMode)
            {
                //update the target position each 300 frames
                if (!Agent.hasPath || (Time.frameCount % 300) == 0)
                {
                    if (bl_UtilityHelper.Distance(TargetPosition, Agent.destination) > 1)//update the path only if the target has moved substantially
                    {
                        SetDestination(TargetPosition, 3);
                    }
                }
            }
            else
            {
                //in one team mode, when the target is in the limit range
                //the bot will start to random patrol instead of following the player.
                RandomPatrol(true);
            }
            SetDebugState(0, true);
        }
        else if (AgentState == AIAgentState.Searching)
        {
            SetDebugState(1, true);
            RandomPatrol(true);
        }
        else
        {
            SetDebugState(2, true);
            SetTarget(null);
            RandomPatrol(false);
            SetState(AIAgentState.Patroling);
        }
        Speed = behaviorSettings.agentBehave == AIAgentBehave.Agressive ? soldierSettings.runSpeed : soldierSettings.walkSpeed;
        if (!AIWeapon.IsFiring)
        {
            Anim.SetInteger("UpperState", 4);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void WhenTargetOnMediumRange()
    {
        SetDebugState(3, true);
        precisionRate = 4;
        OnTargeContest(false);
    }

    /// <summary>
    /// 
    /// </summary>
    void WhenTargetOnCloseRange()
    {
        SetDebugState(4, true);
        precisionRate = 2;
        Follow();
    }

    /// <summary>
    /// 
    /// </summary>
    void WhenTargetOnLimitRange()
    {
        SetDebugState(5, true);
        OnTargeContest(true);
        precisionRate = 1;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnCovering()
    {
        if (Time.frameCount % precisionRate != 0) return;

        if (Target != null)
        {
            CachedTargetDistance = TargetDistance;
            if (CachedTargetDistance <= soldierSettings.mediumRange && playerInFront)//if in look range and in front, start follow him and shot
            {
                if (behaviorSettings.agentBehave == AIAgentBehave.Agressive)
                {
                    if (!Agent.hasPath)
                    {
                        SetDebugState(38, true);
                        SetState(AIAgentState.Following);
                        SetDestination(TargetPosition, 3);
                    }
                    else
                    {
                        if (AgentState == AIAgentState.Covering)
                        {
                            SetDebugState(6, true);

                            if (!ObstacleBetweenTarget)
                            {
                                TriggerFire(playerInFront ? bl_AIShooterAttackBase.FireReason.Forced : bl_AIShooterAttackBase.FireReason.OnMove);
                            }

                            if(CachedTargetDistance < soldierSettings.mediumRange)
                            {
                                SetCrouch(false);
                            }

                            if (AIHealth.GetHealth() <= 30 && CachedTargetDistance < soldierSettings.closeRange)
                            {
                                SetState(AIAgentState.Following);
                                SetDestination(GetPositionAround(TargetPosition, 35), 5);
                            }
                        }
                        else
                        {
                            SetDebugState(39, true);
                        }
                    }
                }
                else //to covert point and looking to it
                {
                    SetDebugState(7, true);
                    SetState(AIAgentState.Covering);
                    if (!Agent.hasPath)
                    {
                        Cover(false);
                    }
                }
                TriggerFire(bl_AIShooterAttackBase.FireReason.OnMove);
            }
            else if (CachedTargetDistance > soldierSettings.limitRange && CanCover(7))// if out of line of sight, start searching him
            {
                SetDebugState(8, true);
                SetState(AIAgentState.Searching);
                SetCrouch(false);
                TriggerFire(bl_AIShooterAttackBase.FireReason.OnMove);
            }
            else if (ForceCoverFire && !ObstacleBetweenTarget)//if bot is cover and still get damage, start shoot at the target (panic)
            {
                SetDebugState(9, true);
                SetLookAtState(AILookAt.Target);
                TriggerFire(bl_AIShooterAttackBase.FireReason.Forced);

                if (behaviorSettings.agentBehave == AIAgentBehave.Protective || CachedTargetDistance > soldierSettings.mediumRange)
                {
                    if (CanCover(behaviorSettings.maxCoverTime)) { SwitchCover(); }
                }
            }
            else if (CanCover(behaviorSettings.maxCoverTime) && CachedTargetDistance >= 7)//if has been a time since cover and nothing happen, try a new spot.
            {
                SetDebugState(10, true);
                SwitchCover();
                TriggerFire(bl_AIShooterAttackBase.FireReason.OnMove);
            }
            else
            {
                if (playerInFront)
                {
                    Speed = soldierSettings.walkSpeed;
                    TriggerFire(bl_AIShooterAttackBase.FireReason.Forced);
                    SetDebugState(11, true);
                }
                else
                {
                    SetDebugState(12, true);
                    Speed = soldierSettings.runSpeed;
                    CheckConfrontation();
                    SetLookAtState(AILookAt.Target);
                    TriggerFire(bl_AIShooterAttackBase.FireReason.OnMove);
                    SetCrouch(false);
                }
            }
        }
        if (Agent.pathStatus == NavMeshPathStatus.PathComplete)//once the bot reach the target cover point
        {
            if (CoverPoint != null && CoverPoint.Crouch) { SetCrouch(true); }//and the point required crouch -> do crouch
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private bool Cover(bool overridePoint, AIAgentCoverArea coverArea = AIAgentCoverArea.ToPoint)
    {
        //if the target if far, there's not point in cover right now
        if (behaviorSettings.agentBehave == AIAgentBehave.Agressive && CachedTargetDistance > 20)
        {
            SetState(AIAgentState.Following);
            return false;
        }
        Transform t = transform;
        switch (coverArea)
        {
            case AIAgentCoverArea.ToTarget:
                t = Target;//find a point near the target
                break;
        }
        if (overridePoint)//override the current cover point
        {
            //if the agent has complete his current destination
            if (Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                //get a random point in 30 metters
                if (coverArea == AIAgentCoverArea.ToRandomPoint)
                {
                    //look for another random cover point 
                    CoverPoint = bl_AICovertPointManager.Instance.GetCoverOnRadius(t, 30);
                }
                else
                {
                    //Get the nearest cover point
                    CoverPoint = bl_AICovertPointManager.Instance.GetCloseCover(t, CoverPoint);
                }
            }
            SetDebugState(13);
        }
        else
        {
            SetDebugState(14);
            //look for a near cover point
            CoverPoint = bl_AICovertPointManager.Instance.GetCloseCover(t);
        }

        if (CoverPoint != null && (time - CoverTime) > behaviorSettings.coverColdDown)//if a point was found
        {
            SetDebugState(15);
            Speed = playerInFront ? soldierSettings.walkSpeed : soldierSettings.runSpeed;
            SetDestination(CoverPoint.transform.position, 0.1f);
            SetState(AIAgentState.Covering);
            CoverTime = time;
            TriggerFire(behaviorSettings.agentBehave == AIAgentBehave.Agressive ? bl_AIShooterAttackBase.FireReason.Normal : bl_AIShooterAttackBase.FireReason.OnMove);
            //LookAtTarget();
            return true;
        }
        else
        {
            //if there are not nears cover points
            if (Target != null)//and have a target
            {
                SetDebugState(16);
                //follow the target
                SetDestination(TargetPosition, 3);
                DetermineSpeedBaseOnRange();
                FocusOnSingleTarget = true;//and follow not matter the distance
                SetState(AIAgentState.Searching);
            }
            else//if don't have a target
            {
                SetDebugState(17);
                //Force to get a covert point
                CoverPoint = bl_AICovertPointManager.Instance.GetCloseCoverForced(m_Transform);
                SetDestination(CoverPoint.transform.position, 0.1f);
                Speed = Probability(0.5f) ? soldierSettings.walkSpeed : soldierSettings.runSpeed;
            }
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnGetHit(Vector3 pos)
    {
        LastHitDirection = pos;
        //if the AI is not covering, will look for a cover point
        if (AgentState != AIAgentState.Covering)
        {
            //if the AI is following and attacking the target he will not look for cover point
            if (AgentState == AIAgentState.Following && TargetDistance <= soldierSettings.mediumRange && !ObstacleBetweenTarget)
            {
                SetLookAtState(AILookAt.Target);
                return;
            }
            Cover(false);
        }
        else
        {
            //if already in a cover and still get shoots from far away will force the AI to fire.
            if (!playerInFront)
            {
                SetLookAtState(AILookAt.Target);
                Cover(true);
            }
            else
            {
                ForceCoverFire = true;
                SetLookAtState(AILookAt.HitDirection);
            }
            //if the AI is cover but still get hit, he will search other cover point 
            if (AIHealth.GetHealth() <= 50 && Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                Cover(true);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnHoldingPosition()
    {
        //if (Time.frameCount % 10 != 0) return;

        SetDebugState(37, true);
        if (time > lastHoldPositionTime)
        {
            SetState(Target != null ? AIAgentState.Following : AIAgentState.Searching);
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SwitchCover()
    {
        if (Agent.pathStatus != NavMeshPathStatus.PathComplete)
            return;

        if (SwitchCoverTimes <= 3)
        {
            Cover(true, AIAgentCoverArea.ToTarget);
            SwitchCoverTimes++;
        }
        else
        {
            SetState(AIAgentState.Following);
            SetDestination(TargetPosition, 3);
            SwitchCoverTimes = 0;
            AllOrNothing = true;//go straight to the target to confront him
        }
    }

    /// <summary>
    /// When the target is at look range
    /// </summary>
    void OnTargeContest(bool overrideCover)
    {
        if (AgentState == AIAgentState.Following || ForceFollowAtHalfHealth)
        {
            if (!Cover(overrideCover) || CanCover(behaviorSettings.maxCoverTime) || AllOrNothing)
            {
                if (CachedTargetDistance <= 3)
                {
                    SetDebugState(35);
                    Cover(true, AIAgentCoverArea.ToRandomPoint);
                }
                else
                {
                    SetDebugState(18);
                    Follow();
                }
            }
            else
            {
                if (Target != null)
                {
                    var fr = TargetDistance < 12 ? bl_AIShooterAttackBase.FireReason.Forced : bl_AIShooterAttackBase.FireReason.OnMove;
                    TriggerFire(fr);
                }
                SetDebugState(19);
                SetCrouch(true);
            }
        }
        else if (AgentState == AIAgentState.Covering)
        {
            if (CanCover(5) && TargetDistance >= 7)
            {
                SetDebugState(21);
                Cover(true);
            }
            else
            {
                SetDebugState(22);
            }
        }
        else
        {

            OnDirectConfrontation();
        }
    }

    /// <summary>
    // When the target is near
    // Determine if can attack him
    /// </summary>
    void OnDirectConfrontation()
    {
        precisionRate = 1;
        if (CachedTargetDistance <= 15)
        {
            SetCrouch(false);
            if (AIHealth.GetHealth() <= 25)
            {
                SetDebugState(36);
                SetState(AIAgentState.Covering);
                Cover(false, AIAgentCoverArea.ToRandomPoint);
                Speed = soldierSettings.runSpeed;
            }
            else
            {
                SetDebugState(23);
                if (time > lastHoldPositionTime)
                {
                    // if (Random.value > 0.8f)
                    HoldPosition();
                    // else lastHoldPositionTime = time + 7;
                }
                DetermineSpeedBaseOnRange();
            }
        }
        else
        {
            SetDebugState(35);
            DetermineSpeedBaseOnRange();
        }

        CheckConfrontation();
    }

    /// <summary>
    /// 
    /// </summary>
    void SearchPlayers()
    {
        SetDebugState(-2);

        Transform target = behaviorSettings.agentBehave == AIAgentBehave.Agressive ? GetNearestPlayer() : GetTargetOnRange();
        if (target != null) SetTarget(target);

        // Take a random target at the beginning
        if (bl_PhotonNetwork.IsMasterClient && !randomOnStartTake && availableTargets.Count > 0)
        {
            if (behaviorSettings.GetRandomTargetOnStart)
            {
                SetTarget(availableTargets[Random.Range(0, availableTargets.Count)]);
                randomOnStartTake = true;
            }
        }

        if (!HasATarget)
        {
            if (AgentState == AIAgentState.Following || AgentState == AIAgentState.Looking) { SetState(AIAgentState.Searching); }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckVision()
    {
        if (!HasATarget || !bl_PhotonNetwork.IsMasterClient)
        {
            ObstacleBetweenTarget = false;
            return;
        }

        Vector3 relative = m_Transform.InverseTransformPoint(TargetPosition);
        playerInFront = (relative.x < 2f && relative.x > -2f) || (relative.x > -2f && relative.x < 2f);

        if (Physics.Linecast(AIWeapon.GetFirePosition(), TargetPosition, out obsRay, ObstaclesLayer, QueryTriggerInteraction.Ignore))
        {
            ObstacleBetweenTarget = obsRay.transform.root.CompareTag(bl_MFPS.LOCAL_PLAYER_TAG) == false;
        }
        else { ObstacleBetweenTarget = false; }

        if (wasTargetInSight != ObstacleBetweenTarget)
        {
            OnTargetLineOfSightChanged(wasTargetInSight, ObstacleBetweenTarget);
            wasTargetInSight = ObstacleBetweenTarget;
        }
    }

    /// <summary>
    /// Make the bot stay where he is for a given time
    /// </summary>
    /// <param name="forTime"></param>
    public void HoldPosition(float forTime = 10)
    {
        SetState(AIAgentState.HoldingPosition);
        lastDestinationTime = Time.time + forTime;
    }

    /// <summary>
    /// If a enemy is not in range, then make the AI randomly patrol in the map
    /// </summary>
    /// <param name="precision">Patrol closed to the enemy's area</param>
    void RandomPatrol(bool precision)
    {
        if (isDeath || !bl_PhotonNetwork.IsMasterClient)
            return;

        float precisionArea = soldierSettings.farRange;
        if (precision)
        {
            if (TargetDistance < soldierSettings.mediumRange)
            {
                SetDebugState(24);
                if (!HasATarget)
                {
                    SetTarget(GetNearestPlayer());
                }
                SetState(behaviorSettings.agentBehave == AIAgentBehave.Protective ? AIAgentState.Covering : AIAgentState.Looking);
                precisionArea = 5;
            }
            else
            {
                SetDebugState(25);
                SetState(behaviorSettings.agentBehave == AIAgentBehave.Agressive ? AIAgentState.Following : AIAgentState.Searching);
                precisionArea = 8;
            }
        }
        else
        {
            SetDebugState(26);
            SetState(AIAgentState.Patroling);
            if (behaviorSettings.agentBehave == AIAgentBehave.Agressive && !HasATarget)
            {
                SetTarget(GetNearestPlayer());
                SetCrouch(false);
            }
            ForceCoverFire = false;
        }

        SetLookAtState(AILookAt.Path);
        AIWeapon.IsFiring = false;

        if (!Agent.hasPath || (time - lastPathTime) > 5)
        {
            SetDebugState(27);
            bool toAnCover = (Random.value <= behaviorSettings.randomCoverProbability);//probability of get a cover point as random destination
            Vector3 randomDirection = TargetPosition + (Random.insideUnitSphere * precisionArea);
            if (toAnCover) { randomDirection = bl_AICovertPointManager.Instance.GetCoverOnRadius(m_Transform, 20).Position; }
            if (!HasATarget && isOneTeamMode)
            {
                randomDirection += m_Transform.localPosition;
            }
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, precisionArea, 1);
            finalPosition = hit.position;
            lastPathTime = time + Random.Range(1, 5);
            DetermineSpeedBaseOnRange();
            SetCrouch(false);
        }
        else
        {
            if (Agent.hasPath)
            {
                SetDebugState(28);
            }
            else
            {
                SetDebugState(32);
            }
        }
        SetDestination(finalPosition, 1, true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void KillTheTarget()
    {
        if (!HasATarget) return;

        SetTarget(null);
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("type", AIRemoteCallType.SyncTarget);
        data.Add("viewID", -1);

        photonView.RPC(RPC_NAME, RpcTarget.Others, data);
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckConfrontation()
    {
        if (AgentState != AIAgentState.Covering)
        {
            if (lookTime >= 5)
            {
                SetState(AIAgentState.Following);
                lookTime = 0;
                return;
            }
            lookTime += delta;
            SetState(AIAgentState.Looking);
        }

        TriggerFire();
        SetCrouch(playerInFront && !ObstacleBetweenTarget);
    }

    /// <summary>
    /// 
    /// </summary>
    private void LookAtControl()
    {
        if (!isMaster) return;

        if ((Time.frameCount % bl_AIMananger.Instance.updateBotsLookAtEach) == 0 || forceUpdateRotation)
        {
            if (LookingAt != AILookAt.Path && !HasATarget)
            {
                LookingAt = AILookAt.Path;
            }

            switch (LookingAt)
            {
                case AILookAt.Path:
                case AILookAt.PathToTarget:
                    int cID = Agent.path.corners.Length > 1 ? 1 : 0;
                    var v = Agent.path.corners[cID];
                    v.y = m_Transform.localPosition.y + 0.2f;
                    LookAtPosition = v;

                    v = LookAtPosition - m_Transform.localPosition;
                    LookAtDirection = v;
                    break;
                case AILookAt.Target:
                    v = TargetPosition;
                    v.y += 0.22f;
                    LookAtPosition = v;
                    LookAtDirection = LookAtPosition - m_Transform.localPosition;
                    break;
                case AILookAt.HitDirection:
                    LookAtPosition = LastHitDirection;
                    LookAtDirection = LookAtPosition - m_Transform.localPosition;
                    if (LookAtDirection == Vector3.zero) { LookAtDirection = m_Transform.localPosition + (m_Transform.forward * 10); }
                    break;
            }


            if (bl_UtilityHelper.Distance(m_Transform.localPosition, LookAtPosition) <= 3)
            {
                //LookAtPosition = m_Transform.localPosition + (m_Transform.forward * 10);
                Vector3 lap = LookAtPosition;
                lap.y = headBoneReference.position.y;
                LookAtPosition = lap;
                LookAtDirection = LookAtPosition - m_Transform.localPosition;
            }

            localRotation = m_Transform.localEulerAngles;
            localRotation.y = Quaternion.LookRotation(LookAtDirection).eulerAngles.y;

            forceUpdateRotation = false;
        }

        m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, Quaternion.Euler(localRotation), delta * soldierSettings.rotationSmoothing);
    }

    /// <summary>
    /// 
    /// </summary>
    void Follow()
    {
        if (AgentState == AIAgentState.Covering && Random.value > 0.5f) return;

        SetLookAtState(AILookAt.Target);

        SetCrouch(false);
        SetDestination(TargetPosition, 3);
        if (CachedTargetDistance <= 3)
        {
            CoverTime = 0;
            Speed = soldierSettings.walkSpeed;
            if (Cover(true, AIAgentCoverArea.ToTarget) && Random.value >= 0.5f)
            {
                SetDebugState(29);
            }
            else if (Cover(true, AIAgentCoverArea.ToRandomPoint))
            {
                SetDebugState(30);
            }
            else
            {
                SetDebugState(34);
                SetDestination(m_Transform.localPosition - (m_Transform.forward * 3), 0.1f);
            }
            SetState(AIAgentState.Covering);
            CheckConfrontation();
            SetCrouch(false);
            TriggerFire(bl_AIShooterAttackBase.FireReason.Forced);
        }
        else
        {
            DetermineSpeedBaseOnRange();
            SetDebugState(33);
            TriggerFire();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void TriggerFire(bl_AIShooterAttackBase.FireReason reason = bl_AIShooterAttackBase.FireReason.Normal)
    {
        if (LookingAt == AILookAt.Path) SetLookAtState(AILookAt.Target);

        AIWeapon.Fire(reason);
    }

    /// <summary>
    /// 
    /// </summary>
    private void DetermineSpeedBaseOnRange()
    {
        Speed = (Target != null && CachedTargetDistance > soldierSettings.mediumRange) ? soldierSettings.runSpeed : soldierSettings.walkSpeed;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetState(AIAgentState newState)
    {
        if (newState == AgentState) return;

        OnAgentStateChanged(AgentState, newState);
        AgentState = newState;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        if (Target == newTarget) return;

        OnTargetChanged(Target, newTarget);
        Target = newTarget;

        if (!bl_PhotonNetwork.IsMasterClient) return;

        // sync the bot target
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("type", AIRemoteCallType.SyncTarget);

        if (Target == null)
        {
            data.Add("viewID", -1);
            photonView.RPC(RPC_NAME, RpcTarget.Others, data);
        }
        else
        {
            PhotonView view = GetPhotonView(Target.root.gameObject);
            if (view != null)
            {                            
                data.Add("viewID", view.ViewID);
                photonView.RPC(RPC_NAME, RpcTarget.Others, data);
            }
            else
            {
                Debug.Log("This Target " + Target.name + "no have photonview");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetLookAtState(AILookAt newLookAt)
    {
        if (LookingAt == newLookAt) return;
        if (LookingAt == AILookAt.PathToTarget && newLookAt == AILookAt.Target)
        {
            if (ObstacleBetweenTarget) return;
        }

        LookingAt = newLookAt;
        forceUpdateRotation = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetDestination(Vector3 position, float stopedDistance, bool checkRate = false)
    {
        if (checkRate && (time - lastDestinationTime) < 2) return;

        Agent.stoppingDistance = stopedDistance;
        Agent.SetDestination(position);
        lastDestinationTime = time;
    }

    /// <summary>
    /// 
    /// </summary>
    void SetCrouch(bool crouch)
    {
        if (IsCrouch == crouch || time - lastCrouchTime < 0.5f) return;
        lastCrouchTime = time;
        if (crouch && (AgentState == AIAgentState.Following || AgentState == AIAgentState.Looking))
        {
            crouch = false;
        }

        Anim.SetBool(animationHash[0], crouch);
        Speed = crouch ? soldierSettings.crounchSpeed : soldierSettings.walkSpeed;
        if (IsCrouch != crouch)
        {
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("type", AIRemoteCallType.CrouchState);
            data.Add("state", crouch);

            photonView.RPC(RPC_NAME, RpcTarget.Others, data);
            IsCrouch = crouch;
        }
    }

    /// <summary>
    /// This is called when the bot have a Target
    /// this check if other enemy is nearest and change of target if it's require
    /// </summary>
    void CheckEnemysDistances()
    {
        if (!behaviorSettings.checkEnemysWhenHaveATarget || availableTargets.Count <= 0) return;
        if (time < nextEnemysCheck) return;

        CachedTargetDistance = bl_UtilityHelper.Distance(m_Transform.localPosition, TargetPosition);
        for (int i = 0; i < availableTargets.Count; i++)
        {
            //if the enemy transform is not null or the same target that have currently have or death.
            if (availableTargets[i] == null || availableTargets[i] == Target || availableTargets[i].name.Contains("(die)")) continue;
            //calculate the distance from this other enemy
            float otherDistance = bl_UtilityHelper.Distance(m_Transform.localPosition, availableTargets[i].position);
            if (otherDistance > soldierSettings.limitRange) continue;//if this enemy is too far away...
            //and check if it's nearest than the current target (5 meters close at least)
            if (otherDistance < CachedTargetDistance && (CachedTargetDistance - otherDistance) > 5)
            {
                //calculate the angle between this bot and the other enemy to check if it's in a "View Angle"
                Vector3 targetDir = availableTargets[i].position - m_Transform.localPosition;
                float Angle = Vector3.Angle(targetDir, m_Transform.forward);
                if (Angle > -55 && Angle < 55)
                {
                    //so then get it as new dangerous target
                    SetTarget(availableTargets[i]);
                    //prevent to change target in at least the next 3 seconds
                    nextEnemysCheck = time + 3;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdateTargetList()
    {
        if (Target != null) return;

        bl_AIMananger.Instance.GetTargetsFor(this, ref availableTargets);
        AimTarget.name = AIName;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void CheckTargets()
    {
        if (Target != null && Target.name.Contains("(die)"))
        {
            SetTarget(null);
        }
    }

    /// <summary>
    /// This is called when a forced bot respawn is called and this bot is still alive
    /// </summary>
    public override void Respawn()
    {
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("type", AIRemoteCallType.Respawn);
        photonView.RPC(RPC_NAME, RpcTarget.All, data);
    }

    /// <summary>
    /// 
    /// </summary>
    public void FootStep()
    {
        if (velocityMagnitud > 0.2f)
            footstep?.UpdateStep(Agent.speed);
    }

    [System.Serializable]
    public class NetworkMessagesCount
    {
        public AIRemoteCallType CallType;
        public int Count;
    }
    public List<NetworkMessagesCount> networkMessagesCounts = new List<NetworkMessagesCount>();

    [PunRPC]
    public void RPCShooterBot(NetHashTable data, PhotonMessageInfo info)
    {
        var callType = (AIRemoteCallType)data["type"];

        var cid = networkMessagesCounts.FindIndex(x => x.CallType == callType);
        if(cid == -1)
        {
            networkMessagesCounts.Add(new NetworkMessagesCount()
            {
                CallType = callType,
                Count = 1
            });
        }
        else
        {
            networkMessagesCounts[cid].Count++;
        }

        switch (callType)
        {
            case AIRemoteCallType.DestroyBot:
                DestroyBot(data, info);
                break;
            case AIRemoteCallType.SyncTarget:
                SyncTargetAI(data);
                break;
            case AIRemoteCallType.CrouchState:
                Anim.SetBool(animationHash[0], (bool)data["state"]);
                break;
            case AIRemoteCallType.Respawn:
                DoAliveRespawn();
                break;
        }
    }

    /// <summary>
    /// Do a respawn without destroying the bot instance
    /// </summary>
    private void DoAliveRespawn()
    {
        Agent.isStopped = false;
        AIHealth.SetHealth(100, true);
        isGameStarted = false;
        Target = null;
        this.InvokeAfter(2, () => { isGameStarted = bl_MatchTimeManagerBase.Instance.TimeState == RoomTimeState.Started; });
        if (bl_PhotonNetwork.IsMasterClient)
        {
            var spawn = bl_SpawnPointManager.Instance.GetSequentialSpawnPoint(AITeam).transform;
            Agent.Warp(spawn.position);
            m_Transform.position = spawn.position;
            m_Transform.rotation = spawn.rotation;
        }      
    }

    /// <summary>
    /// Called from Master Client on all clients when a bot die
    /// </summary>
    public void DestroyBot(NetHashTable data, PhotonMessageInfo info)
    {
        if (data.ContainsKey("instant"))
        {
            if (bl_PhotonNetwork.IsMasterClient) bl_PhotonNetwork.Destroy(gameObject);
            return;
        }

        Vector3 position = (Vector3)data["direction"];
        References.aiAnimation.Ragdolled(position, data.ContainsKey("explosion"));

        if ((bl_PhotonNetwork.Time - info.SentServerTime) > bl_GameData.Instance.PlayerRespawnTime + 1)
        {
            Debug.LogWarning("The death call get with too much delay.");
        }
        this.InvokeAfter(bl_GameData.Instance.PlayerRespawnTime + 2, () =>
         {
             if (GetGameMode.GetGameModeInfo().onRoundStartedSpawn != GameModeSettings.OnRoundStartedSpawn.WaitUntilRoundFinish)
                 Debug.LogWarning("For some reason this bot has not been destroyed yet.");
         });
    }

    /// <summary>
    /// 
    /// </summary>
    void SyncTargetAI(NetHashTable data)
    {
        var view = (int)data["viewID"];
        if (view == -1)
        {
            SetTarget(null);
            return;
        }

        GameObject pr = FindPlayerRoot(view);
        if (pr == null)
        {
            Debug.Log($"Couldn't find the network view: {view}");
            return;
        }

        Transform t = pr.transform;
        if (t != null)
        {
            SetTarget(t);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalSpawn()
    {
        if (!isOneTeamMode && bl_MFPS.LocalPlayer.Team == AITeam)
        {
            References.namePlateDrawer.enabled = true;
        }
    }

    /// <summary>
    /// When a new player joins in the room
    /// </summary>
    /// <param name="newPlayer"></param>
    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (bl_PhotonNetwork.IsMasterClient && newPlayer.ActorNumber != bl_PhotonNetwork.LocalPlayer.ActorNumber)
        {
            // received in bl_AIShooterHealth
            photonView.RPC("RpcSyncHealth", newPlayer, AIHealth.GetHealth());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onLocalPlayerSpawn += OnLocalSpawn;
        bl_EventHandler.onMatchStart += OnMatchStart;
        bl_EventHandler.onMatchStateChanged += OnMatchStateChange;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalSpawn;
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
        bl_EventHandler.onMatchStart -= OnMatchStart;
        bl_EventHandler.onMatchStateChanged -= OnMatchStateChange;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnMatchStateChange(MatchState state)
    {
        if (state == MatchState.Waiting || state == MatchState.Starting) isGameStarted = false;
        else isGameStarted = bl_MatchTimeManagerBase.Instance.TimeState == RoomTimeState.Started;      
    }

    /// <summary>
    /// 
    /// </summary>
    void OnMatchStart()
    {
        isGameStarted = bl_GameManager.Instance.GameMatchState != MatchState.Waiting && bl_MatchTimeManagerBase.Instance.TimeState == RoomTimeState.Started;
        UpdateTargetList();
    }

    public override void OnDeath() { CancelInvoke(); }

    private float Speed
    {
        get => Agent.speed;
        set
        {
            bool cr = Anim.GetBool(animationHash[0]);
            if (cr)
            {
                Agent.speed = 2;
            }
            else
            {
                Agent.speed = value;
            }
        }
    }

    public override Vector3 TargetPosition
    {
        get
        {
            if (Target != null) { return Target.position; }
            if (!isOneTeamMode && availableTargets.Count > 0)
            {
                Transform t = GetNearestPlayer();
                if (t != null)
                {
                    return t.position;
                }
                else { return m_Transform.position + (m_Transform.forward * 3); }
            }
            return Vector3.zero;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="positionReference"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    private Vector3 GetPositionAround(Vector3 positionReference, float radius)
    {
        var v = Random.insideUnitSphere * radius;
        v.y = 0;
        v += positionReference;
        return v;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Transform GetNearestPlayer()
    {
        if (availableTargets.Count > 0)
        {
            Transform t = null;
            float d = 1000;
            for (int i = 0; i < availableTargets.Count; i++)
            {
                if (availableTargets[i] == null || availableTargets[i].name.Contains("(die)")) continue;
                float dis = bl_UtilityHelper.Distance(m_Transform.localPosition, availableTargets[i].position);
                if (dis < d)
                {
                    d = dis;
                    t = availableTargets[i];
                }
            }
            return t;
        }
        else { return null; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Transform GetTargetOnRange()
    {
        Transform t = null;
        if (availableTargets.Count <= 0) return t;

        List<Transform> inRange = new List<Transform>();
        float distance;
        for (int i = 0; i < availableTargets.Count; i++)
        {
            t = availableTargets[i];
            if (t == null || t.name.Contains("(die)")) continue;

            distance = bl_UtilityHelper.Distance(m_Transform.localPosition, t.position);
            if (distance < soldierSettings.mediumRange)
            {
                inRange.Add(t);
            }
        }

        if (inRange.Count > 0) t = inRange[Random.Range(0, inRange.Count)];
        else t = null;

        return t;
    }

    private MFPSPlayer m_MFPSActor;
    public MFPSPlayer BotMFPSActor
    {
        get
        {
            if (m_MFPSActor == null) { m_MFPSActor = bl_GameManager.Instance.GetMFPSPlayer(AIName); }
            return m_MFPSActor;
        }
    }

    bool isNewDebug = false;
    public void SetDebugState(int stateID, bool initial = false)
    {
        if (!DebugStates) return;
        if (initial && isNewDebug)
        {
            DebugLine = $"{stateID}"; isNewDebug = true; return;
        }
        DebugLine += $"&{stateID}";
    }
    public bool Probability(float probability) { return Random.value <= probability; }
    public bool ForceFollowAtHalfHealth => AIHealth.GetHealth() < 50 && behaviorSettings.forceFollowAtHalfHealth;

    public float TargetDistance { get { return bl_UtilityHelper.Distance(m_Transform.position, TargetPosition); } }
    public bool HasATarget { get => Target != null; }
    private bool CanCover(float inTimePassed) { return ((time - CoverTime) >= inTimePassed); }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (isDeath) return;
        if (Agent != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Agent.destination, 0.3f);
            Gizmos.DrawLine(m_Transform.localPosition, Agent.destination);

            if (Agent.path != null && Agent.path.corners != null)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < Agent.path.corners.Length; i++)
                {
                    if (i == 0)
                    {
                        // Gizmos.DrawSphere(Agent.path.corners[i], 0.05f);
                        continue;
                    }
                    else if (i == 1)
                    {
                        Gizmos.DrawSphere(Agent.path.corners[i], 0.2f);
                    }

                    Gizmos.DrawLine(Agent.path.corners[i - 1], Agent.path.corners[i]);
                }
            }
        }
        if (Target != null)
        {
            Gizmos.color = playerInFront && !ObstacleBetweenTarget ? Color.green : Color.cyan;
            Gizmos.DrawLine(m_Transform.localPosition, TargetPosition);
        }
        if (m_Transform == null) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(m_Transform.localPosition, LookAtPosition);
        Gizmos.DrawWireCube(LookAtPosition, Vector3.one * 0.3f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!DebugStates || soldierSettings == null) return;
        if (m_Transform == null) { m_Transform = transform; }
        Gizmos.color = Color.yellow;
        bl_UtilityHelper.DrawWireArc(m_Transform.position, soldierSettings.limitRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.white;
        bl_UtilityHelper.DrawWireArc(m_Transform.position, soldierSettings.farRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.yellow;
        bl_UtilityHelper.DrawWireArc(m_Transform.position, soldierSettings.mediumRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.white;
        bl_UtilityHelper.DrawWireArc(m_Transform.position, soldierSettings.closeRange, 360, 12, Quaternion.identity);
    }
#endif
}