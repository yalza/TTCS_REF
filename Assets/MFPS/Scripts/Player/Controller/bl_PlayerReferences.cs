    using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations;

public class bl_PlayerReferences : bl_PlayerReferencesCommon
{
    public bl_PlayerNetwork playerNetwork;
    public bl_FirstPersonControllerBase firstPersonController;
    public bl_PlayerHealthManagerBase playerHealthManager;
    public bl_PlayerSettings playerSettings;
    public bl_GunManager gunManager;
    public bl_PlayerAnimationsBase playerAnimations;
    public bl_PlayerRagdollBase playerRagdoll;
    public bl_RecoilBase recoil;
    public bl_CameraShakerBase cameraShaker;
    public bl_CameraRayBase cameraRay;
    public bl_WeaponBobBase weaponBob;
    public bl_WeaponSwayBase weaponSway;
    public bl_HitBoxManager hitBoxManager;
    public bl_CameraMotionBase cameraMotion;
    public CharacterController characterController;
    public PhotonView photonView;
    public Camera playerCamera;
    public Camera weaponCamera;
    public ParentConstraint leftArmTarget;

    [SerializeField] private Animator m_playerAnimator = null;
    public override Animator PlayerAnimator
    {
        get
        {
            if (m_playerAnimator == null) m_playerAnimator = playerAnimations.Animator;
            return m_playerAnimator;
        }set => m_playerAnimator = value;
    }

    [SerializeField] private Transform m_botAimTarget = null;
    public override Transform BotAimTarget
    {
        get
        {
            if (m_botAimTarget == null) m_botAimTarget = transform;
            return m_botAimTarget;
        }
        set => m_botAimTarget = value;
    }

    #region Getters
    private Transform _transform;
    public Transform Transform
    {
        get
        {
            if (_transform == null) _transform = transform;
            return _transform;
        }
    }

    public override Team PlayerTeam { get => playerSettings.PlayerTeam; }

    public Vector3 Position => Transform.position;
    public Vector3 LocalPosition => Transform.localPosition;
    public Quaternion Rotation => Transform.rotation;
    public Quaternion LocalRotation => Transform.localRotation;
    public GameObject LocalPlayerObjects => playerSettings.LocalObjects;
    public GameObject RemotePlayerObjects => playerSettings.RemoteObjects;

    private Transform m_playerCameraTransform;
    public Transform PlayerCameraTransform
    {
        get
        {
            if (m_playerCameraTransform == null) m_playerCameraTransform = playerCamera.transform;
            return m_playerCameraTransform;
        }
    }

    public static bl_PlayerReferences LocalPlayer
    {
        get
        {
            return bl_GameManager.Instance.LocalPlayerReferences;
        }
    }

    private bl_PlayerIKBase m_playerIK= null;
    public bl_PlayerIKBase playerIK
    {
        get
        {
            if (playerAnimations == null) return null;
            if (m_playerIK == null) m_playerIK = playerAnimations.GetComponentInChildren<bl_PlayerIKBase>();
            return m_playerIK;
        }
    }

    private float m_defaultCameraFOV = -1;
    public float DefaultCameraFOV
    {
        get
        {
            if (m_defaultCameraFOV == -1) m_defaultCameraFOV = playerCamera.fieldOfView;
            return m_defaultCameraFOV;
        }
        set
        {
            m_defaultCameraFOV = value;
            playerCamera.fieldOfView = m_defaultCameraFOV;
        }
    }

    private Collider[] m_allColliders;
    public override Collider[] AllColliders
    {
        get
        {
            if(m_allColliders == null || m_allColliders.Length <= 0)
            {
                m_allColliders = transform.GetComponentsInChildren<Collider>();
            }
            return m_allColliders;
        }
    }

    public int ViewID => photonView.ViewID;
    public int ActorNumber => photonView.Owner.ActorNumber;

#if MFPS_VEHICLE
    private bl_PlayerVehicle m_playerVehicle = null;
    public bl_PlayerVehicle PlayerVehicle
    {
        get
        {
            if (m_playerVehicle == null) m_playerVehicle = GetComponent<bl_PlayerVehicle>();
            return m_playerVehicle;
        }
    }
#endif
    #endregion

    #region Functions

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsDeath()
    {
       return playerHealthManager.IsDeath();
    }
    
    /// <summary>
    /// 
    /// </summary>
    public override void IgnoreColliders(Collider[] list, bool ignore)
    {
        for (int e = 0; e < list.Length; e++)
        {
            for (int i = 0; i < AllColliders.Length; i++)
            {
                if (AllColliders[i] != null)
                {
                    Physics.IgnoreCollision(AllColliders[i], list[e], ignore);
                }
            }
        }
    }
    #endregion

    #region Editor Only
    [HideInInspector] public bl_NetworkGun EditorSelectedGun = null;
    #endregion
}