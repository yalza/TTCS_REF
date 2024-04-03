using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using MFPSEditor;

public class bl_PlayerSettings : bl_PhotonHelper
{

    #region Public members
    public Team PlayerTeam = Team.None;
    public List<MonoBehaviour> RemoteOnlyScripts = new List<MonoBehaviour>();
    public List<MonoBehaviour> LocalOnlyScripts = new List<MonoBehaviour>();
    [Space(5)]
    public GameObject LocalObjects;
    public GameObject RemoteObjects;
    [Header("Player References")]
    public Transform FlagPosition;
    public Transform carrierPoint;
    public GameObject AimPositionReference;
    public Mesh directionMesh;

    [Header("Hands Textures")]
    [ScriptableDrawer] public bl_FPArmsMaterial armsMaterial;
    private List<bl_FPArmsMaterial.MaterialColor> currentWeaponMaterials = new List<bl_FPArmsMaterial.MaterialColor>(); 
    #endregion

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (!bl_PhotonNetwork.IsConnected || !bl_PhotonNetwork.InRoom) return;
        PlayerTeam = (Team)photonView.InstantiationData[0];
        if (isMine)
        {
            OnLocalPlayer();
        }
        else
        {
            OnRemotePlayer();
        }
    }

    /// <summary>
    /// We call this function only if this is a Remote player
    /// </summary>
    public void OnRemotePlayer()
    {
        for (int i = 0; i < LocalOnlyScripts.Count; i++)
        {
            if (LocalOnlyScripts[i] != null)
            {
                 LocalOnlyScripts[i].enabled = false;
            }
        }
        LocalObjects.SetActive(false);
        gameObject.tag = bl_MFPS.REMOTE_PLAYER_TAG;
        gameObject.layer = (int)Mathf.Log(bl_GameData.TagsAndLayerSettings.RemotePlayerRootLayer.value, 2);

        //Build Player Data
        MFPSPlayer playerData = new MFPSPlayer()
        {
            Name = photonView.Owner.NickName,
            Team = PlayerTeam,
            Actor = transform,
            isRealPlayer = true,
            isAlive = true,
            ActorView = photonView,
            AimPosition = carrierPoint,
        };

        bl_EventHandler.DispatchRemoteActorChange(new bl_EventHandler.PlayerChangeData()
        {
            PlayerName = photonView.Owner.NickName,
            MFPSActor = playerData,
            IsAlive = true,
            NetworkView = photonView
        });
    }

    /// <summary>
    /// We call this function only if we are Local player
    /// </summary>
    public void OnLocalPlayer()
    {
        gameObject.name = bl_PhotonNetwork.NickName;
        for (int i = 0; i < RemoteOnlyScripts.Count; i++)
        {
            if (RemoteOnlyScripts[i] != null)
            {
                RemoteOnlyScripts[i].enabled = false;
            }
        }
        RemoteObjects.SetActive(false);
        gameObject.tag = bl_MFPS.LOCAL_PLAYER_TAG;
        gameObject.layer = bl_GameData.TagsAndLayerSettings.GetLocalPlayerLayerIndex();

        armsMaterial?.SelectTeamMaterial(PlayerTeam);
        if (bl_GameData.Instance.doSpawnHandMeshEffect)
        {
            StartCoroutine(DoSpawnLoop());
        }
        playerReferences.DefaultCameraFOV = (float)bl_MFPS.Settings.GetSettingOf("FOV");
        bl_EventHandler.onGameSettingsChange += OnGameSettingsChanged;
#if GR
        transform.GetComponentInChildren<bl_GunManager>().isGunRace = (GetGameMode == GameMode.GR);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        SetDeafultWeaponRender();
        if (isMine)
        {
            bl_EventHandler.onGameSettingsChange -= OnGameSettingsChanged;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDestroy()
    {
        if (!isMine)
        {
            bl_EventHandler.DispatchRemoteActorChange(new bl_EventHandler.PlayerChangeData()
            {
                PlayerName = gameObject.name,
                MFPSActor = null,
                IsAlive = false,
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGameSettingsChanged()
    {
        playerReferences.DefaultCameraFOV = (float)bl_MFPS.Settings.GetSettingOf("FOV");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(carrierPoint == null)
        {
            var pa = GetComponent<bl_PlayerReferences>().playerAnimations;
            if (pa != null)
            {
                Animator animator = pa.Animator;
                if (animator != null) { carrierPoint = animator.GetBoneTransform(HumanBodyBones.UpperChest); }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (directionMesh == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawMesh(directionMesh, transform.position, transform.rotation, Vector3.one * 0.4f);
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public void DoSpawnWeaponRenderEffect(Renderer[] renderers)
    {
        if (!bl_GameData.Instance.doSpawnHandMeshEffect) return;
        if (currentWeaponMaterials.Count > 0)
        {
            //set default color
            foreach (var item in currentWeaponMaterials)
            {
                if (item.Material == null) continue;
                item.Material.color = item.Color;
            }
        }
        currentWeaponMaterials.Clear();
        foreach (var item in renderers)
        {
            if (item == null) continue;
            Material[] mats = item.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null || !mats[i].HasProperty("_Color")) continue;
                bl_FPArmsMaterial.MaterialColor mc = new bl_FPArmsMaterial.MaterialColor(mats[i]);
                currentWeaponMaterials.Add(mc);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DoSpawnLoop()
    {
        float d = 0;
        float st = bl_GameData.Instance.SpawnProtectedTime;
        float value = 0;
        Color teamColor = PlayerTeam.GetTeamColor();
        while (d < 1)
        {
            d += Time.deltaTime / st;
            value = Mathf.PingPong(Time.time, 0.25f) * 4;
            if (currentWeaponMaterials.Count > 0)
            {
                for (int i = 0; i < currentWeaponMaterials.Count; i++)
                {
                    if (currentWeaponMaterials[i].Material == null) continue;
                    currentWeaponMaterials[i].Material.color = Color.Lerp(currentWeaponMaterials[i].Color, teamColor, value);
                }
            }
            yield return null;
        }
        SetDeafultWeaponRender();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetDeafultWeaponRender()
    {
        if (currentWeaponMaterials.Count > 0)
        {
            foreach (var item in currentWeaponMaterials)
            {
                item.Material.color = item.Color;
            }
        }
    }

    public bool isLocal { get { return photonView.IsMine; } }
    public PhotonView View { get { return photonView; } }
    public bool isTeamMate { get { return (PlayerTeam == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam() && !isOneTeamMode); } }

    private bl_PlayerReferences _playerReferences;
    public bl_PlayerReferences playerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = GetComponentInParent<bl_PlayerReferences>();
            return _playerReferences;
        }
    }

}