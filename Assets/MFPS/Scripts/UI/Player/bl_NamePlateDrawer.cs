using UnityEngine;
#if !UNITY_WEBGL && PVOICE
using Photon.Voice.Unity;
using Photon.Voice.PUN;
#endif
using UnityEngine.Serialization;
using MFPS.Internal.BaseClass;

/// <summary>
/// Default MFPS Name Plate (Name above head) script
/// If you want to make a different approach like use UGUI instead of OnGUI
/// Create a custom script and inherited it from bl_NamePlateBase.cs
/// </summary>
public class bl_NamePlateDrawer : bl_NamePlateBase
{
    #region Public members
    [Header("Settings")]
    public float distanceModifier = 1;
    public float hideDistance = 25;

    [Header("References")]
    public bl_NamePlateStyle StylePresent;
    [FormerlySerializedAs("m_Target")]
    public Transform positionReference;
    #endregion

    #region Private members
    private float distance;
    private Transform myTransform;
#if !UNITY_WEBGL && PVOICE
    private PhotonVoiceView voiceView;
    private float RightPand = 0;
    private float NameSize = 0;
#endif
    private bl_PlayerHealthManagerBase healthManager;
    private bool ShowHealthBar = false;
    private bool isFinish = false;
    float screenHeight = 0;
    Vector3 screenPoint;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
#if !UNITY_WEBGL && PVOICE
         voiceView = GetComponent<PhotonVoiceView>();
#endif
        healthManager = GetComponent<bl_PlayerHealthManagerBase>();
        ShowHealthBar = bl_GameData.Instance.ShowTeamMateHealthBar;
        isFinish = bl_MatchTimeManagerBase.Instance.IsTimeUp();
        this.myTransform = this.transform;

        if(bl_GameManager.Instance.CameraRendered != null)
        distance = bl_UtilityHelper.Distance(myTransform.position, bl_GameManager.Instance.CameraRendered.transform.position);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onRoundEnd += OnGameFinish;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onRoundEnd -= OnGameFinish;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="DrawName"></param>
    public override void SetName(string DrawName)
    {
        PlayerName = DrawName;
#if !UNITY_WEBGL && PVOICE
        NameSize = StylePresent.style.CalcSize(new GUIContent(PlayerName)).x;
        RightPand = (NameSize / 2) + 2;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public override void SetActive(bool active)
    {
        this.enabled = active;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGameFinish()
    {
        isFinish = true;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGUI()
    {
        if (BlockDraw || bl_GameManager.Instance.CameraRendered == null || isFinish || bl_PauseMenuBase.IsMenuOpen)
            return;
        if (StylePresent == null) return;

        screenPoint = bl_GameManager.Instance.CameraRendered.WorldToScreenPoint(positionReference.position);
        if (screenPoint.z > 0)
        {
            int vertical = ShowHealthBar ? 15 : 10;
            screenHeight = Screen.height;
            if (this.distance < hideDistance)
            {
                float distanceDifference = Mathf.Clamp(distance - 0.1f, 1, 12);
                screenPoint.y += distanceDifference * distanceModifier;              

                GUI.Label(new Rect(screenPoint.x - 5, (screenHeight - screenPoint.y) - vertical, 10, 11), PlayerName, StylePresent.style);
                if (ShowHealthBar)
                {
                    GUI.color = StylePresent.HealthBackColor;
                    GUI.DrawTexture(new Rect(screenPoint.x - (50), (screenHeight - screenPoint.y), 100, StylePresent.HealthBarThickness), StylePresent.HealthBarTexture);
                    GUI.color = StylePresent.HealthBarColor;
                    GUI.DrawTexture(new Rect(screenPoint.x - (50), (screenHeight - screenPoint.y), healthManager.GetHealth(), StylePresent.HealthBarThickness), StylePresent.HealthBarTexture);
                    GUI.color = Color.white;
                }

#if !UNITY_WEBGL && PVOICE
                //voice chat icon
                if (voiceView != null && voiceView.IsSpeaking)
                {
                    GUI.DrawTexture(new Rect(screenPoint.x + RightPand, (screenHeight - screenPoint.y) - vertical, 14, 14), StylePresent.TalkingIcon);
                }
#endif
            }
            else
            {
                float iconSize = StylePresent.IndicatorIconSize;
                GUI.DrawTexture(new Rect(screenPoint.x - (iconSize * 0.5f), (screenHeight - screenPoint.y) - (iconSize * 0.5f), iconSize, iconSize), StylePresent.IndicatorIcon);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnSlowUpdate()
    {
        if (bl_GameManager.Instance.CameraRendered == null)
            return;

        distance = bl_UtilityHelper.Distance(myTransform.position, bl_GameManager.Instance.CameraRendered.transform.position);
    }
}