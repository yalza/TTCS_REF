using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MFPS.Internal.Structures;
using System.Linq;

/// <summary>
/// Default crosshair implementation
/// You can use your own crosshair system by inherited your main crosshair script from <see cref="bl_CrosshairBase"/>
/// </summary>
public class bl_Crosshair : bl_CrosshairBase
{

    #region Public members
    [Header("Settings")]
    [LovattoToogle] public bool fadeOnAim = true;
    [Range(1, 10)] public float ScaleLerp = 5;
    [Range(0.1f, 5)] public float RotationSpeed = 2;
    [Range(0.01f, 1)] public float OnFireScaleRate = 0.1f;
    [Range(0.01f, 1)] public float OnCrouchReduce = 8;

    [Header("Hit Marker")]
    [Range(0.1f, 3)] public float Duration = 1;
    [Range(5, 50)] public float IncreaseAmount = 25;
    public Color HitMarkerColor = Color.white;
    public AnimationCurve hitScaleCurve;
    public AnimationCurve hitAlphaCurve;

    [Header("References")]
    [SerializeField] private bl_UCrosshairInfo genericCrosshair = null;
    [SerializeField] private WeaponCrosshair[] weaponCrosshairs = null;

    [SerializeField] private RectTransform RootContent = null;
    [SerializeField] private RectTransform HitMarkerRoot = null;
    #endregion

    #region Private members
    private Vector2 InitSizeDelta;
    private bool isAim = false;
    private Canvas m_Canvas;
    private Vector3 InitialRotation;
    private float lastTimeFire;
    private CanvasGroup m_HitAlpha;
    private Vector2 defaultHitSize;
    private float hitDuration;
    private CanvasGroup CrossAlpha;
    private bool m_allowFade = true;
    private PlayerState playerState = PlayerState.Idle;
    private Vector2 extraScale = Vector2.zero;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (RootContent != null)
        {
            InitSizeDelta = RootContent.sizeDelta;
            InitialRotation = RootContent.eulerAngles;
            CrossAlpha = RootContent.GetComponent<CanvasGroup>();
        }
        if (HitMarkerRoot != null)
        {
            m_HitAlpha = HitMarkerRoot.GetComponent<CanvasGroup>();
            defaultHitSize = HitMarkerRoot.sizeDelta;
            if (m_HitAlpha != null) { m_HitAlpha.alpha = 0; }
            Graphic[] hmg = HitMarkerRoot.GetComponentsInChildren<Graphic>();
            foreach (Graphic g in hmg) { g.color = HitMarkerColor; }
        }
        m_Canvas = transform.root.GetComponent<Canvas>();
        if (m_Canvas == null) { m_Canvas = transform.root.GetComponentInChildren<Canvas>(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (GetCrosshair.isStatic)
            return;

        ScaleContent();
        FadeControll();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        if (m_HitAlpha != null)
        {
            m_HitAlpha.alpha = 0;
        }
        bl_EventHandler.onLocalPlayerDeath += OnLocalDeath;
        bl_EventHandler.onLocalPlayerStateChanged += OnLocalPlayerStateChange;
        RootContent.gameObject.SetActive(bl_GameData.Instance.showCrosshair);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onLocalPlayerDeath -= OnLocalDeath;
        bl_EventHandler.onLocalPlayerStateChanged -= OnLocalPlayerStateChange;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalDeath()
    {
        playerState = PlayerState.Idle;
        isAim = false;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalPlayerStateChange(PlayerState from, PlayerState to)
    {
        playerState = to;
    }

    /// <summary>
    /// 
    /// </summary>
    void ScaleContent()
    {
        if (RootContent == null)
            return;

        Vector2 target = (isAim) ? new Vector2(GetCrosshair.OnAimScaleAmount, GetCrosshair.OnAimScaleAmount) : (InitSizeDelta + extraScale);
        Vector2 size = (isCrouch) ? target - CrouchVector : target;
        RootContent.sizeDelta = Vector2.Lerp(RootContent.sizeDelta, size, Time.unscaledDeltaTime * ScaleLerp);
    }

    /// <summary>
    /// 
    /// </summary>
    void FadeControll()
    {
        if (CrossAlpha == null)
            return;

        float at = (isAim && CanFade) ? 0 : 1;
        CrossAlpha.alpha = Mathf.Lerp(CrossAlpha.alpha, at, Time.deltaTime * 10);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gun"></param>
    public override void SetupCrosshairForWeapon(bl_Gun gun)
    {
        if (gun == null) return;

        extraScale = Vector2.one * gun.CrossHairScale;
        SetupCrosshairForWeapon(gun.WeaponType);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gun"></param>
    public override void SetupCrosshairForWeapon(GunType gun)
    {
        for (int i = 0; i < weaponCrosshairs.Length; i++)
        {
            if (weaponCrosshairs[i] == null) continue;
            weaponCrosshairs[i].crosshair?.SetActive(false);
        }

        int crossId = weaponCrosshairs.ToList().FindIndex(x => x.gunType == gun);
        if (crossId == -1)
        {
            ActiveGenericCrosshair();
        }
        else
        {
            weaponCrosshairs[crossId].crosshair?.SetActive(true);
            m_currentCrosshair = weaponCrosshairs[crossId].crosshair;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ActiveGenericCrosshair()
    {
        genericCrosshair.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public override void Show(bool show)
    {
        if (Block || !bl_GameData.Instance.showCrosshair)
            return;

        RootContent.gameObject.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFire()
    {
        if (!Interactable)
            return;
        if (Time.time < lastTimeFire)
            return;

        Vector2 size = (isCrouch) ? new Vector2(GetCrosshair.OnFireScaleAmount, GetCrosshair.OnFireScaleAmount) - CrouchVector : new Vector2(GetCrosshair.OnFireScaleAmount, GetCrosshair.OnFireScaleAmount);
        RootContent.sizeDelta = size;
        lastTimeFire = Time.time + OnFireScaleRate;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnAim(bool aim)
    {
        if (!Interactable)
            return;

        isAim = aim;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnHit()
    {
        if (HitMarkerRoot == null)
            return;

        StopCoroutine(nameof(OnHitMarker));
        StartCoroutine(nameof(OnHitMarker));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="allow"></param>
    public override void AllowFade(bool allow)
    {
       m_allowFade = allow;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reset()
    {
        RootContent.sizeDelta = new Vector2(GetCrosshair.NormalScaleAmount, GetCrosshair.NormalScaleAmount);
        RootContent.eulerAngles = InitialRotation;
    }

    /// <summary>
    /// 
    /// </summary>
    private bool CanFade
    {
        get => m_allowFade && fadeOnAim;
    }

    private bool Interactable => RootContent != null && !GetCrosshair.isStatic;

    /// <summary>
    /// 
    /// </summary>
    private bl_UCrosshairInfo m_currentCrosshair;
    public bl_UCrosshairInfo GetCrosshair
    {
        get
        {
            if (m_currentCrosshair == null) m_currentCrosshair = genericCrosshair;
            return m_currentCrosshair;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator OnHitMarker()
    {
        hitDuration = 0;
        HitMarkerRoot.sizeDelta = defaultHitSize;
        Vector2 sizeTarget = new Vector2(IncreaseAmount, IncreaseAmount);

        while (hitDuration < 1)
        {
            hitDuration += Time.unscaledDeltaTime / Duration;
            HitMarkerRoot.sizeDelta = Vector2.Lerp(defaultHitSize, sizeTarget, hitScaleCurve.Evaluate(hitDuration));
            if (m_HitAlpha != null) { m_HitAlpha.alpha = hitAlphaCurve.Evaluate(hitDuration); }
            yield return null;
        }
    }

    private Vector2 CrouchVector
    {
        get
        {
            return isCrouch ? Vector2.one * OnCrouchReduce : Vector2.zero;
        }
    }

    private bool isCrouch => playerState == PlayerState.Crouching;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (RootContent == null)
            return;

        RootContent.sizeDelta = new Vector2(GetCrosshair.NormalScaleAmount, GetCrosshair.NormalScaleAmount);
    }
#endif

    [System.Serializable]
    public class WeaponCrosshair
    {
        public GunType gunType;
        public bl_UCrosshairInfo crosshair;
    }
}