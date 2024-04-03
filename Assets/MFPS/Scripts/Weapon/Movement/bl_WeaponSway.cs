using UnityEngine;
using System.Collections;

public class bl_WeaponSway : bl_WeaponSwayBase
{
    #region Public members
    private float maxAmount = 0.05F;
    [Header("Movements")]
    [Range(0.2f, 5)] public float delayAmplitude = 2;
    [Range(0.01f, 0.5f)] public float pushAmplutide = 0.2f;
    [Range(1, 7)] public float sideAngleAmplitude = 4;
    public float Smoothness = 3.0F;
    public float aimMultiplier = 0.05f;

    [Header("FallEffect")]
    [Range(0.01f, 1.0f)]
    public float m_time = 0.2f;
    public float m_ReturnSpeed = 5;
    public float SliderAmount = 12;
    public float DownAmount = 13;
    public AnimationCurve downCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public float Amount { get; set; }
    #endregion

    #region Private members
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private Quaternion delayRotation;
    private Vector3 targetVector = Vector3.zero;
    private Transform m_Transform;
    private float factorX, factorY, factorZ = 0;
    private Vector3 defaultEuler;
    private float deltaTime;
    private bool isAiming = false;
    private float amplitudeMultiplier = 1;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_Transform = transform;
        defaultPosition = m_Transform.localPosition;
        defaultRotation = m_Transform.localRotation;
        Amount = delayAmplitude;
        defaultEuler = m_Transform.localEulerAngles;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {     
        deltaTime = Time.smoothDeltaTime;
        DelayMovement();
        SideMovement();

        m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, defaultRotation * delayRotation, deltaTime * m_ReturnSpeed);
    }

    /// <summary>
    /// The delay effect movement when move the camera with the mouse.
    /// </summary>
    void DelayMovement()
    {
        factorX = (-bl_GameInput.MouseX * deltaTime * Amount) * amplitudeMultiplier;
        factorY = (-bl_GameInput.MouseY * deltaTime * Amount) * amplitudeMultiplier;
        factorZ = (-bl_GameInput.Vertical * (isAiming ? pushAmplutide * 0.1f : pushAmplutide)) * amplitudeMultiplier;
        factorX = Mathf.Clamp(factorX, -maxAmount, maxAmount);
        factorY = Mathf.Clamp(factorY, -maxAmount, maxAmount);
        targetVector.Set(defaultPosition.x + factorX, defaultPosition.y + factorY, defaultPosition.z + factorZ);
        delayRotation = Quaternion.Euler((factorY * 100), (-factorX * 100), 0);
        m_Transform.localPosition = Vector3.Slerp(m_Transform.localPosition, targetVector, deltaTime * Smoothness);
    }

    /// <summary>
    /// The angle oscillation movement when the player move sideway
    /// </summary>
    void SideMovement()
    {
        factorX = bl_GameInput.Horizontal;
        defaultEuler.z = (factorX * sideAngleAmplitude) * amplitudeMultiplier;
        defaultEuler.z = -defaultEuler.z;
        defaultRotation = Quaternion.Euler(defaultEuler);
        defaultRotation = Quaternion.Slerp(defaultRotation, Quaternion.identity, deltaTime * Smoothness);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onPlayerLand += this.OnSmallImpact;
        bl_EventHandler.onLocalAimChanged += OnLocalAimChanged;
        bl_EventHandler.onChangeWeapon += OnLocalWeaponChanged;
        bl_EventHandler.onLocalPlayerStateChanged += OnLocalStateChange;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onPlayerLand -= this.OnSmallImpact;
        bl_EventHandler.onLocalAimChanged -= OnLocalAimChanged;
        bl_EventHandler.onChangeWeapon -= OnLocalWeaponChanged;
        bl_EventHandler.onLocalPlayerStateChanged -= OnLocalStateChange;
    }

    /// <summary>
    /// On Impact event
    /// </summary>
    void OnSmallImpact(float impactAmount)
    {
        StartCoroutine(FallEffect(impactAmount));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aim"></param>
    void OnLocalAimChanged(bool aim)
    {
        isAiming = aim;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newWeapon"></param>
    void OnLocalWeaponChanged(int newWeapon)
    {
        isAiming = false;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalStateChange(PlayerState from, PlayerState to)
    {
        if(to == PlayerState.Jumping)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="multiplier"></param>
    public override void SetMotionMultiplier(float multiplier)
    {
        amplitudeMultiplier = multiplier;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UseAimSettings()
    {
        amplitudeMultiplier = aimMultiplier;
    }

    /// <summary>
    /// create a soft impact effect
    /// </summary>
    /// <returns></returns>
    public IEnumerator FallEffect(float amount)
    {
        float mul = Mathf.Clamp01(amount / 3.3f);
        float side = SliderAmount * mul;
        Quaternion m_default = m_Transform.localRotation;
        Quaternion m_finaly = m_Transform.localRotation * Quaternion.Euler(new Vector3(DownAmount * mul, Random.Range(-side, side), m_default.z));
        float t_rate = 1.0f / m_time;
        float t_time = 0.0f;
        float t;

        while (t_time < 1.0f)
        {
            t = downCurve.Evaluate(t_time);
            t_time += Time.deltaTime * t_rate;
            m_Transform.localRotation = Quaternion.Slerp(m_default, m_finaly, t);
            yield return t_rate;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void ResetSettings()
    {
        Amount = delayAmplitude;
        amplitudeMultiplier = 1;
    }
}