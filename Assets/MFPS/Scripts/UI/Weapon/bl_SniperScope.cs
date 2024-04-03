using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MFPS.Runtime.UI;

public class bl_SniperScope : bl_SniperScopeBase
{
    #region Public members
    [Range(0.02f, 0.5f)] public float transitionDuration = 0.2f;
    [Range(0, 0.5f)] public float fadeInDelay = 0.2f;
    public float breathingAmplitude = 0.14f;
    [Space]
    [LovattoToogle] public bool ShowDistance = true;
    [Space]
    [Tooltip("Objects to disable when the scope shown, usually the weapon and arms meshes.")]
    public List<GameObject> OnScopeDisable = new List<GameObject>();
    #endregion

    #region Private members
    private bl_Gun m_gun;
    private Vector3 m_point = Vector3.zero;
    private float m_dist = 0.0f;
    private Text DistanceText;
    private bool returnedAim = true;
    private bool aiming = false;
    RaycastHit m_ray;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_gun = GetComponent<bl_Gun>();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_ScopeUIBase.Instance.SetupWeapon(this);
        aiming = false;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        if (bl_ScopeUIBase.Instance != null) bl_ScopeUIBase.Instance.SetActive(false);
        aiming = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (m_gun == null || ScopeTexture == null)
            return;

        if (m_gun.isAiming && !m_gun.isReloading)
        {
            GetDistance();

            if (!aiming)
            {
                returnedAim = false;
                bl_ScopeUIBase.Instance?.Crossfade(true, transitionDuration, fadeInDelay, null, () =>
                  {
                      foreach (GameObject go in OnScopeDisable)
                      {
                          if (go == null) continue;
                          go.SetActive(false);
                      }
                  });

                // Active breathing effect
                if (breathingAmplitude > 0)
                {
                    m_gun.PlayerReferences.cameraMotion.SetActiveBreathing(true, breathingAmplitude);
                }

                aiming = true;
            }
        }
        else
        {
            if (aiming)
            {
                float inverse = bl_MathUtility.InverseLerp(m_gun.GetDefaultPosition(), m_gun.AimPosition, m_gun.transform.localPosition);
                bool wasFullAiming = inverse >= 0.78f;
                bl_ScopeUIBase.Instance?.Crossfade(false, transitionDuration, 0, () =>
                   {
                       if (!returnedAim && m_gun.Info != null)
                       {
                           if(wasFullAiming) m_gun.SetToAim();
                           returnedAim = true;
                       }
                      
                       foreach (GameObject go in OnScopeDisable)
                       {
                           if (go == null) continue;
#if MFPSTPV
                           if (bl_CameraViewSettings.IsThirdPerson()) continue;
#endif
                           go.SetActive(true);
                       }

                   });
                m_gun.PlayerReferences.cameraMotion.SetActiveBreathing(false);
            }
            aiming = false;
        }
        if (ShowDistance && DistanceText)
        {
            DistanceText.text = m_dist.ToString("00") + "<size=10>m</size>";
        }
    }

    /// <summary>
    /// calculate the distance to the first object that raycast hits
    /// </summary>  
    void GetDistance()
    {
        if (!ShowDistance) return;

        Vector3 fwd = bl_GameManager.Instance.CameraRendered.transform.forward;
        if (Physics.Raycast(bl_GameManager.Instance.CameraRendered.transform.position, fwd, out m_ray, 1000))
        {
            m_point = m_ray.point;
            m_dist = bl_UtilityHelper.Distance(m_point, bl_GameManager.Instance.CameraRendered.transform.position);
        }
        else
        {
            m_dist = 0.0f;
        }
    }
}