using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// MFPS default script to handle the FPWeapon animations.
/// You can use your own script to handle the animations if you inherit your script from bl_WeaponAnimationBase.cs
/// </summary>
public class bl_WeaponAnimation : bl_WeaponAnimationBase
{
    #region Public members
    public AnimationType m_AnimationType = AnimationType.Animation;
    public FireBlendMethod fireBlendMethod = FireBlendMethod.FireSpeed;
    public AnimationClip DrawName;
    public AnimationClip TakeOut;
    public AnimationClip SoloFireClip;
    public AnimationClip[] FireAnimations;
    public AnimationClip FireAimAnimation;
    public AnimationClip ReloadName;
    public AnimationClip IdleClip;
    [Range(0.1f, 5)] public float FireSpeed = 1.0f;
    [Range(0.1f, 5)] public float DrawSpeed = 1.0f;
    [Range(0.1f, 5)] public float HideSpeed = 1.0f;
    public AnimationClip StartReloadAnim;
    public AnimationClip InsertAnim;
    public AnimationClip AfterReloadAnim;
    [Range(0.1f, 5)] public float InsertSpeed = 1.0f;
    public AnimationClip QuickFireAnim;
    public ParticleSystem[] Particles;
    [Range(1, 10)] public float ParticleRate = 5;
    public bool HasParticles = false;
    public bool AnimatedMovements = false;
    public bool DrawAfterFire = false;

    public AudioClip Reload_1;
    public AudioClip Reload_2;
    public AudioClip Reload_3;
    public AudioClip m_Fire;
    #endregion

    #region Private members
    public bl_Gun ParentGun { get; set; }
    private bl_GunManager GunManager;
    private bool cancelReload = false;
    private bl_PlayerReferences playerReferences;
    private AudioSource m_source;
    private GunType gunType = GunType.Machinegun;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (ParentGun == null) { ParentGun = transform.GetComponentInParent<bl_Gun>(); }
        gunType = ParentGun.Info.Type;
        GunManager = transform.GetComponentInParent<bl_GunManager>();
        playerReferences = GunManager.playerReferences;

        m_source = GetComponent<AudioSource>();
        if (m_source == null)
        {
            m_source = gameObject.AddComponent<AudioSource>();
            m_source.playOnAwake = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            Anim.wrapMode = WrapMode.Once;
        }
        else
        {
            if (AnimatedMovements) { playerReferences.weaponBob.AnimatedThis(UpdateAnimated, true); } else { playerReferences.weaponBob.AnimatedThis(null, false); }
        }
    }

    #region Base interface
    /// <summary>
    /// 
    /// </summary>
    /// <param name="aiming"></param>
    public override float PlayFire(AnimationFlags flags)
    {
        switch (gunType)
        {
            case GunType.Knife:
                return KnifeFire(flags.IsEnumFlagPresent(AnimationFlags.QuickFire));
            case GunType.Grenade:
                return FireGrenade(flags.IsEnumFlagPresent(AnimationFlags.QuickFire));
            default:
                if (!flags.IsEnumFlagPresent(AnimationFlags.Aiming))
                    return Fire();
                else return AimFire();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="onFinish"></param>
    public override void PlayReload(float reloadTime, int[] data, AnimationFlags flags, Action onFinish = null)
    {
        if (flags.IsEnumFlagPresent(AnimationFlags.SplitReload))
            SplitReload(reloadTime, data[0]); // data[0] = number of bullets to add to the magazine.
        else
            Reload(reloadTime);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override float PlayTakeIn()
    {
        // the code is not handle here directly because the DrawWeapon() function was there since version 1.
        return DrawWeapon();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override float PlayTakeOut()
    {
        // the code is not handle here directly because the HideWeapon() function was there since version 1.
        return HideWeapon();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="animationType"></param>
    public override void CancelAnimation(WeaponAnimationType animationType)
    {
        if (animationType == WeaponAnimationType.Reload) cancelReload = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override float GetAnimationDuration(WeaponAnimationType animationType, float[] data = null)
    {
        switch (animationType)
        {
            case WeaponAnimationType.Fire:
                return GetFireLenght;
            case WeaponAnimationType.AimFire:
                return FireAimAnimation.length / FireSpeed;
            case WeaponAnimationType.TakeIn:
                return DrawName.length / DrawSpeed;
            case WeaponAnimationType.TakeOut:
                return TakeOut.length / HideSpeed;
            case WeaponAnimationType.Reload:
                if (ParentGun.reloadPer == bl_Gun.ReloadPer.Magazine)
                    return ReloadName.length / ParentGun.Info.ReloadTime;
                else
                {
                    float insertD = InsertAnim.length / InsertSpeed;
                    if(data != null && data.Length > 0)
                    {
                        insertD = insertD * data[0];
                    }
                    return StartReloadAnim.length + AfterReloadAnim.length + insertD;
                }
            default:
                return 0;
        }
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerState"></param>
    void UpdateAnimated(PlayerState playerState)
    {
        if (AnimatedMovements)
        {
            float speedPercentage = playerReferences.firstPersonController.VelocityMagnitude / playerReferences.firstPersonController.GetSpeedOnState(PlayerState.Running, true);
            animator.SetFloat("Speed", speedPercentage);
            animator.SetInteger("PlayerState", (int)playerState);

            if (speedPercentage > 0.55f && ParentGun.FPState != PlayerFPState.Reloading && ParentGun.WeaponType != GunType.Knife && ParentGun.WeaponType != GunType.Grenade)
            {
                animator.CrossFade("Run", 0.2f);
            }
        }
    }

    /// <summary>
    /// Play the fire animation one time
    /// </summary>
    public float Fire()
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            if (FireAnimations.Length <= 0)
                return 0;

            int id = UnityEngine.Random.Range(0, FireAnimations.Length);
            if (FireAnimations[id] == null) { id = 0; }

            string n = FireAnimations[id].name;
            Anim.Rewind(n);
            Anim[n].speed = FireSpeed;
            Anim.Play(n);

            return FireAnimations[id].length / FireSpeed;
        }
        else
        {
            float fireSpeed = FireSpeed;
            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireRateCrossFade)
            {
                fireSpeed = ((SoloFireClip.length * 0.5f) / ParentGun.Info.FireRate);
            }

            animator.SetFloat("FireSpeed", fireSpeed);
            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireSpeed)
            {
                animator.Play("Fire", 0, 0.03f);
            }
            else
            {
                animator.CrossFade("Fire", 0.025f, 0, 0.03f);
            }

            return SoloFireClip.length / FireSpeed;
        }
    }

    /// <summary>
    /// Play the fire animation for the knife
    /// </summary>
    public float KnifeFire(bool Quickfire)
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            if (FireAimAnimation == null)
                return 0;

            string an = Quickfire ? QuickFireAnim.name : FireAimAnimation.name;
           // Anim.Rewind(an);
            Anim[an].speed = FireSpeed;
            Anim.Play(an);

            return Anim[an].length / FireSpeed;
        }
        else
        {
            animator.SetFloat("FireSpeed", FireSpeed);
            string an = Quickfire ? "QuickFire" : "Fire";
            animator.Play(an, 0, 0);
            if (SoloFireClip == null) return FireSpeed;
            return SoloFireClip.length / FireSpeed;
        }
    }

    /// <summary>
    /// Play the fire animation for the grenade
    /// </summary>
    /// <param name="fastFire"></param>
    /// <returns></returns>
    public float FireGrenade(bool fastFire)
    {
        if (fastFire)
        {
            StartCoroutine(FastGrenade());
            return GetFireLenght + (DrawName.length / DrawSpeed);
        }
        else
        {
            if (m_AnimationType == AnimationType.Animation)
            {
                return AimFire();
            }
            else
            {
                animator.SetFloat("FireSpeed", FireSpeed);
                animator.Play("Fire", 0, 0);
                float t = SoloFireClip.length / FireSpeed;
                animator.SetLayerWeight(1, 0);
                if (DrawAfterFire) { Invoke(nameof(DrawWeapon), ((t + ParentGun.Info.ReloadTime)) - (GetAnimationDuration(WeaponAnimationType.TakeIn) * 0.5f)); }
                return t;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator FastGrenade()
    {
        yield return new WaitForSeconds(DrawWeapon());
        AimFire();
    }

    /// <summary>
    /// Throw the grenade prefab (from the bl_Gun.cs)
    /// This should be called from an animation event
    /// </summary>
    public void ThrowProjectile()
    {
        StartCoroutine(ParentGun.ThrowGrenade(false, false));
    }

    /// <summary>
    /// 
    /// </summary>
    public float AimFire()
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            if (FireAimAnimation == null)
                return 0;

            Anim.Rewind(FireAimAnimation.name);
            Anim[FireAimAnimation.name].speed = FireSpeed;
            Anim.Play(FireAimAnimation.name);

            return FireAimAnimation.length / FireSpeed;
        }
        else
        {
            float fireSpeed = FireSpeed;
            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireRateCrossFade)
            {
                fireSpeed = ((SoloFireClip.length * 0.5f) / ParentGun.Info.FireRate);
            }

            animator.SetFloat("FireSpeed", fireSpeed);
            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireSpeed)
            {
                animator.Play("AimFire", 0, 0.02f);
            }
            else
            {
                animator.CrossFade("AimFire", 0.025f, 0, 0.02f);
            }
            return FireAimAnimation.length / FireSpeed;
        }
    }

    /// <summary>
    /// Play the draw/take in animation
    /// </summary>
    public float DrawWeapon()
    {
        if (DrawName == null || !gameObject.activeInHierarchy)
            return 0;

        if (m_AnimationType == AnimationType.Animation)
        {
            Anim.Rewind(DrawName.name);
            Anim[DrawName.name].speed = DrawSpeed;
            Anim[DrawName.name].time = 0;
            Anim.Play(DrawName.name);

            return Anim[DrawName.name].length / DrawSpeed;
        }
        else
        {
            animator.SetFloat("DrawSpeed", DrawSpeed);
            animator.Play("Draw", 0, 0);
            animator.SetLayerWeight(1, 1);
            return DrawName.length / DrawSpeed;
        }
    }

    /// <summary>
    ///  Play the hide/take out animation
    /// </summary>
    public float HideWeapon()
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            if (TakeOut == null)
                return 0;

            Anim[TakeOut.name].speed = HideSpeed;
            Anim[TakeOut.name].time = 0;
            Anim[TakeOut.name].wrapMode = WrapMode.Once;
            Anim.Play(TakeOut.name);
        }
        else
        {
            animator.SetFloat("HideSpeed", HideSpeed);
            animator.CrossFade("Hide", 0.25f);
        }
        return TakeOut == null ? 0 : TakeOut.length / HideSpeed;
    }

    /// <summary>
    /// event called by animation when is a reload state
    /// </summary>
    /// <param name="ReloadTime"></param>
    public void Reload(float ReloadTime)
    {
        if (ReloadName == null)
            return;

        if (m_AnimationType == AnimationType.Animation)
        {
            Anim.Stop(ReloadName.name);
            Anim[ReloadName.name].wrapMode = WrapMode.Once;
            Anim[ReloadName.name].speed = (ReloadName.length / ReloadTime);
            Anim.Play(ReloadName.name);
        }
        else
        {
            animator.SetFloat("ReloadSpeed", ReloadName.length / ReloadTime);
            animator.Play("Reload", 0, 0);
        }
    }

    /// <summary>
    /// event called by animation when is fire
    /// </summary>
    public void FireAudio()
    {
        if (m_source != null && m_Fire != null)
        {
            m_source.clip = m_Fire;
            m_source.pitch = UnityEngine.Random.Range(1, 1.5f);
            m_source.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ReloadTime"></param>
    /// <param name="Bullets"></param>
    public void SplitReload(float ReloadTime, int bulletsToAdd)
    {
        StartCoroutine(StartShotgunReload());
        IEnumerator StartShotgunReload()
        {
            if (m_AnimationType == AnimationType.Animation)
            {
                Anim.CrossFade(StartReloadAnim.name, 0.2f);
                ParentGun.PlayReloadAudio(0);
                yield return new WaitForSeconds(StartReloadAnim.length);
                for (int i = 0; i < bulletsToAdd; i++)
                {
                    Anim[InsertAnim.name].wrapMode = WrapMode.Loop;
                    float speed = Anim[InsertAnim.name].length / InsertSpeed;
                    Anim[InsertAnim.name].speed = speed;
                    Anim.CrossFade(InsertAnim.name);
                    GunManager.HeadAnimation(3, speed);
                    ParentGun.PlayReloadAudio(1);
                    yield return new WaitForSeconds(InsertAnim.length / speed);
                    ParentGun.AddBullet(1);
                    if (cancelReload)
                    {
                        Anim.CrossFade(AfterReloadAnim.name, 0.2f);
                        GunManager.HeadAnimation(0, AfterReloadAnim.length);
                        yield return new WaitForSeconds(AfterReloadAnim.length);
                        ParentGun.FinishReload();
                        cancelReload = false;
                        yield break;
                    }
                }
                Anim.CrossFade(AfterReloadAnim.name, 0.2f);
                GunManager.HeadAnimation(0, AfterReloadAnim.length);
                ParentGun.PlayReloadAudio(2);
                yield return new WaitForSeconds(AfterReloadAnim.length);
                ParentGun.FinishReload();
            }
            else
            {
                animator.speed = 1;
                animator.Play("StartReload", 0, 0);
                ParentGun.PlayReloadAudio(0);
                yield return new WaitForSeconds(StartReloadAnim.length);
                for (int i = 0; i < bulletsToAdd; i++)
                {
                    float speed = InsertAnim.length / InsertSpeed;
                    animator.SetFloat("ReloadSpeed", InsertSpeed);
                    animator.Play("Insert", 0, 0);
                    GunManager.HeadAnimation(3, speed);
                    ParentGun.PlayReloadAudio(1);
                    yield return new WaitForSeconds(speed);
                    ParentGun.AddBullet(1);
                    if (cancelReload)
                    {
                        animator.CrossFade("EndReload", 0.32f, 0);
                        GunManager.HeadAnimation(0, AfterReloadAnim.length);
                        yield return new WaitForSeconds(AfterReloadAnim.length);
                        ParentGun.FinishReload();
                        cancelReload = false;
                        yield break;
                    }
                }
                animator.Play("EndReload", 0, 0);
                GunManager.HeadAnimation(0, AfterReloadAnim.length);
                ParentGun.PlayReloadAudio(2);
                yield return new WaitForSeconds(AfterReloadAnim.length);
                ParentGun.FinishReload();
            }
        }
    }

    /// <summary>
    /// Use this for greater coordination
    /// reload sounds with animation
    /// </summary>
    public void ReloadSound(int index)
    {
        if (m_source == null)
            return;

        switch (index)
        {
            case 0:
                m_source.clip = Reload_1;
                m_source.Play();
                break;
            case 1:
                m_source.clip = Reload_2;
                m_source.Play();
                GunManager.HeadAnimation(3, 1);
                break;
            case 2:
                if (Reload_3 != null)
                {
                    m_source.clip = Reload_3;
                    m_source.Play();
                }
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="motionName"></param>
    public void PlayHeadAnimation(string motionName)
    {
        var animator = GunManager.HeadAnimator;

        if (animator == null) return;

        if (!animator.IsPlaying())
        {
            animator.Play(motionName, 0, 0);
        }
        else
        {
            animator.CrossFade(motionName, 0.2f, 0);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayThrow()
    {
       // Commented since version 1.9.3 to fix an issue with the grenades throwing direction
       // caused by the animation movement.
       
       // GunManager.HeadAnimator.Play("Throw", 0, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void PlayParticle(int id)
    {
        ParticleSystem.EmissionModule m = Particles[id].emission;
        m.rateOverTime = ParticleRate;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void StopParticle(int id)
    {
        ParticleSystem.EmissionModule m = Particles[id].emission;
        m.rateOverTime = 0;
    }

    public void PlayHeadShake() => GunManager.HeadAnimator.Play("shake-small", 0, 0);

    public float GetFireLenght { get { return FireAimAnimation.length / FireSpeed; } }

    [Serializable]
    public enum AnimationType
    {
        Animation,
        Animator,
    }

    public enum FireBlendMethod
    {
        FireRate,
        FireSpeed,
        FireRateCrossFade,
        FireSpeedCrossFade
    }

    private Animator _Animator;
    private Animator animator
    {
        get
        {
            if (_Animator == null)
            {
                _Animator = this.GetComponent<Animator>();
            }
            return _Animator;
        }
    }

    private Animation _Anim;
    private Animation Anim
    {
        get
        {
            if (_Anim == null)
            {
                _Anim = this.GetComponent<Animation>();
            }
            return _Anim;
        }
    }
}