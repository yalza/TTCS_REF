using UnityEngine;
using System.Collections.Generic;
using MFPS.Internal;

public class bl_PlayerAnimations : bl_PlayerAnimationsBase
{
    #region Public members
    public AnimationCurve dropTiltAngleCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float blendSmoothness = 10;
    #endregion

    #region Public properties
    public bl_NetworkGun CurrentNetworkGun
    {
        get;
        set;
    }
    public bool useFootSteps { get; set; } = false;
    public float VelocityMagnitude
    {
        get;
        set;
    }
    #endregion

    #region Private members
    private RaycastHit footRay;
    private float reloadSpeed = 1;
    private PlayerState lastBodyState = PlayerState.Idle;
    private bl_Footstep footstep;
    private float deltaTime = 0.02f;
    private Transform m_Transform;
    private Dictionary<string, int> animatorHashes;
    private bool HitType = false;
    private GunType cacheWeaponType = GunType.Machinegun;
    private float vertical, horizontal;
    private Transform PlayerRoot;
    private float turnSpeed;
    private float TurnLerp = 0;
    private float lastYRotation;
    private float movementSpeed;
    private float timeSinceLastMove = 0;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        m_Transform = transform;
        useFootSteps = bl_GameData.Instance.CalculateNetworkFootSteps;
        if (PlayerReferences != null) PlayerRoot = PlayerReferences.transform;
        if (useFootSteps)
        {
            footstep = PlayerReferences.firstPersonController.GetFootStep();
        }
        if (animatorHashes == null)
        {
            FetchHashes();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_AnimatorReloadEvent.OnTPReload += OnTPReload;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_AnimatorReloadEvent.OnTPReload -= OnTPReload;
    }

    /// <summary>
    /// 
    /// </summary>
    void FetchHashes()
    {
        // cache the hashes in a Array will be more appropriate but to be more readable for other users
        // I decide to cached them in a Dictionary with the key name indicating the parameter that contain
        animatorHashes = new Dictionary<string, int>();
        animatorHashes.Add("BodyState", Animator.StringToHash("BodyState"));
        animatorHashes.Add("Vertical", Animator.StringToHash("Vertical"));
        animatorHashes.Add("Horizontal", Animator.StringToHash("Horizontal"));
        animatorHashes.Add("Speed", Animator.StringToHash("Speed"));
        animatorHashes.Add("Turn", Animator.StringToHash("Turn"));
        animatorHashes.Add("isGround", Animator.StringToHash("isGround"));
        animatorHashes.Add("UpperState", Animator.StringToHash("UpperState"));
        animatorHashes.Add("Move", Animator.StringToHash("Move"));
        animatorHashes.Add("GunType", Animator.StringToHash("GunType"));
    }

    /// <summary>
    /// 
    /// </summary>
    void OnTPReload(bool enter, Animator theAnimator, AnimatorStateInfo stateInfo)
    {
        if (theAnimator != Animator || CurrentNetworkGun == null || CurrentNetworkGun.LocalGun == null) return;

        float duration = CurrentNetworkGun.LocalGun != null ? CurrentNetworkGun.LocalGun.GetReloadTime() : CurrentNetworkGun.Info.ReloadTime;
        reloadSpeed = enter ? (stateInfo.length / duration) : 1;
        Animator.SetFloat("ReloadSpeed", reloadSpeed);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        deltaTime = Time.deltaTime;
        ControllerInfo();
        Animate();
        UpperControll();
        UpdateFootstep();
        DropPlayerAngle();
    }

    /// <summary>
    /// 
    /// </summary>
    void ControllerInfo()
    {
        if (PlayerRoot != null)
            LocalVelocity = PlayerRoot.InverseTransformDirection(Velocity);

        float lerp = deltaTime * blendSmoothness;
        vertical = Mathf.Lerp(vertical, LocalVelocity.z, lerp);
        horizontal = Mathf.Lerp(horizontal, LocalVelocity.x, lerp);

        VelocityMagnitude = Velocity.magnitude;
        movementSpeed = Mathf.Lerp(movementSpeed, VelocityMagnitude, lerp);

        if (VelocityMagnitude > 0.1f)
        {
            timeSinceLastMove = Time.time;
        }

        turnSpeed = Mathf.DeltaAngle(lastYRotation, PlayerRoot.localEulerAngles.y);
        TurnLerp = Mathf.Lerp(TurnLerp, turnSpeed, lerp);

        if (BodyState != PlayerState.Idle || Time.time - timeSinceLastMove < 1)
        {
            TurnLerp = 0;
        }

        if (Time.frameCount % 7 == 0) lastYRotation = PlayerRoot.localEulerAngles.y;
    }

    /// <summary>
    /// 
    /// </summary>
    void Animate()
    {
        if (Animator == null)
            return;

        CheckPlayerStates();

        Animator.SetInteger(animatorHashes["BodyState"], (int)BodyState);
        Animator.SetFloat(animatorHashes["Vertical"], vertical);
        Animator.SetFloat(animatorHashes["Horizontal"], horizontal);
        Animator.SetFloat(animatorHashes["Speed"], movementSpeed);
        Animator.SetFloat(animatorHashes["Turn"], TurnLerp);
        Animator.SetBool(animatorHashes["isGround"], IsGrounded);
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckPlayerStates()
    {
        if (BodyState != lastBodyState)
        {
            if (lastBodyState == PlayerState.Sliding && BodyState != PlayerState.Sliding)
            {
                Animator.CrossFade(animatorHashes["Move"], 0.2f, 0);
            }
            if (BodyState == PlayerState.Sliding)
            {
                Animator.Play("Slide", 0, 0);
            }
            else if (OnEnterPlayerState(PlayerState.Dropping))
            {
                Animator.Play("EmptyUpper", 1, 0);
            }
            else if (OnEnterPlayerState(PlayerState.Gliding))
            {
                Animator.Play("EmptyUpper", 1, 0);
                Animator.CrossFade("gliding-1", 0.33f, 0);
            }

            if (OnExitPlayerState(PlayerState.Dropping))
            {
                m_Transform.localRotation = Quaternion.identity;
            }

            lastBodyState = BodyState;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool OnEnterPlayerState(PlayerState playerState)
    {
        if (BodyState == playerState && lastBodyState != playerState)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool OnExitPlayerState(PlayerState playerState)
    {
        if (lastBodyState == playerState && BodyState != playerState)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    void UpperControll()
    {
        int _fpState = (int)FPState;
        if (_fpState == 9) { _fpState = 1; }
        Animator.SetInteger(animatorHashes["UpperState"], _fpState);
    }

    /// <summary>
    /// 
    /// </summary>
    void DropPlayerAngle()
    {
        if (BodyState != PlayerState.Dropping) return;

        Vector3 pangle = m_Transform.localEulerAngles;
        float tilt = dropTiltAngleCurve.Evaluate(Mathf.Clamp01(VelocityMagnitude / (PlayerReferences.firstPersonController.GetSpeedOnState(PlayerState.Dropping) - 10)));
        pangle.x = Mathf.Lerp(0, 70, tilt);
        m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, Quaternion.Euler(pangle), deltaTime * 4);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void BlockWeapons(int blockState)
    {
        if (PlayerReferences == null || PlayerReferences.playerIK == null) return;

        bool baredHands = blockState == 1;

        // Do not control the arms with IK when the player is not using weapons.
        PlayerReferences.playerIK.ControlArmsWithIK = !baredHands;

        if (blockState != 2)
        {
            // -1 is the ID for play the bared arms animations in the player animator controller.
            int id = baredHands ? -1 : (int)cacheWeaponType;

            if (animatorHashes != null) Animator.SetInteger(animatorHashes["GunType"], id);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnGetHit()
    {
        int r = Random.Range(0, 2);
        string hit = (r == 1) ? "Right Hit" : "Left Hit";
        Animator.Play(hit, 2, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateFootstep()
    {
        if (!useFootSteps) return;
        if (VelocityMagnitude < 0.3f) return;

        bool isClimbing = (BodyState == PlayerState.Climbing);
        if ((!IsGrounded && !isClimbing) || BodyState == PlayerState.Sliding)
            return;

        if (BodyState == PlayerState.Stealth)
        {
            footstep?.SetVolumeMuliplier(footstep.stealthModeVolumeMultiplier);
        }
        else
        {
            footstep?.SetVolumeMuliplier(1f);
        }

        footstep?.UpdateStep(movementSpeed);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typ"></param>
    public override void PlayFireAnimation(GunType typ)
    {
        // Check if the network weapon is using custom animations
        if (CurrentNetworkGun != null && CurrentNetworkGun.useCustomPlayerAnimations)
        {
            if (!string.IsNullOrEmpty(CurrentNetworkGun.customFireAnimationName))
            {
                Animator.Play(CurrentNetworkGun.customFireAnimationName, 1, 0);
                return;
            }
        }

        switch (typ)
        {
            case GunType.Knife:
                Animator.Play("FireKnife", 1, 0);
                break;
            case GunType.Machinegun:
                Animator.Play("RifleFire", 1, 0);
                break;
            case GunType.Burst:
                Animator.Play("BurstFire", 1, 0);
                break;
            case GunType.Pistol:
                Animator.Play("PistolFire", 1, 0);
                break;
            case GunType.Launcher:
                Animator.Play("LauncherFire", 1, 0);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void HitPlayer()
    {
        if (Animator != null)
        {
            HitType = !HitType;
            int ht = (HitType) ? 1 : 0;
            Animator.SetInteger("HitType", ht);
            Animator.SetTrigger("Hit");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdateAnimatorParameters()
    {
        ControllerInfo();
        Animate();
        UpperControll();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetNetworkGun(GunType weaponType, bl_NetworkGun networkGun)
    {
        if (Animator == null || !Animator.gameObject.activeInHierarchy) return;
        
        cacheWeaponType = weaponType;
        CurrentNetworkGun = networkGun;

        if (animatorHashes == null) FetchHashes();

        if (networkGun != null)
        {
            Animator?.SetInteger(animatorHashes["GunType"], networkGun.GetUpperStateID());
            Animator.Play("Equip", 1, 0);
        }

        if (CurrentNetworkGun == null || CurrentNetworkGun.LocalGun == null)
        {
            reloadSpeed = 1;
        }

        // @TODO: Find the issue that cause Missing Reference exception when this conditional is not present.
        if (this != null)
        {
            // Do not control the arms with IK while the 'Equip' animation is playing.
            PlayerReferences.playerIK.ControlArmsWithIK = false;

            CancelInvoke(nameof(ResetHandsIK));
            Invoke(nameof(ResetHandsIK), 0.3f);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    void ResetHandsIK() { PlayerReferences.playerIK.ControlArmsWithIK = true; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public GunType GetCurretWeaponType() { return cacheWeaponType; }

    private bl_PlayerReferences _playerReferences = null;
    private bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = transform.GetComponentInParent<bl_PlayerReferences>();
            return _playerReferences;
        }
    }
}