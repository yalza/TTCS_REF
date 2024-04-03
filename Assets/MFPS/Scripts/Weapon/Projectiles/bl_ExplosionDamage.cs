using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using MFPS.Core.Motion;
using UnityEngine.Serialization;
using MFPS.Audio;

public class bl_ExplosionDamage : bl_ExplosionBase
{
    #region Public members
    [FormerlySerializedAs("m_Type")]
    public ExplosionType explosionType = ExplosionType.Normal;
    [LovattoToogle] public bool CheckRootsOnly = false;
    public float explosionDamage = 50f;
    public float explosionRadius = 50f;
    public float DisappearIn = 3f;
    public LayerMask detectLayers;
    public ShakerPresent shakerPresent;
    public string shakerKey = "explosion";
    #endregion

    #region Private members
    private RaycastHit hitInfo;
    private BulletData cachedData;
    private MFPSPlayer creator;
    #endregion

    /// <summary>
    /// is not remote take damage
    /// </summary>
    void Start()
    {
        if (cachedData == null)
        {
            if (explosionType == ExplosionType.Level)
            {
                cachedData = new BulletData()
                {
                    MFPSActor = bl_GameManager.Instance.LocalActor,
                    isNetwork = false,
                    Damage = explosionDamage,
                    Position = transform.position,

                };
                creator = bl_MFPS.LocalPlayer.MFPSActor;
            }
            else
            {
                Debug.LogWarning("Explosion has not been initialized.");
                return;
            }
        }

        if (!cachedData.isNetwork)
        {
            DoDamage();
            ApplyShake();
        }

        StartCoroutine(Init());
    }

    /// <summary>
    /// 
    /// </summary>
    public override void InitExplosion(BulletData bulletData, MFPSPlayer fromPlayer)
    {
        cachedData = bulletData;
        creator = fromPlayer;
        if (cachedData.Damage > 0) explosionDamage = cachedData.Damage;

        SetupAudio();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="radius"></param>
    public override void SetRadius(float radius)
    {
        explosionRadius = radius;
    }

    /// <summary>
    /// applying impact damage from the explosion to enemies
    /// </summary>
    private void DoDamage()
    {
        if (explosionType == ExplosionType.Shake && !bl_GameData.Instance.ArriveKitsCauseDamage)
            return;

        DoPlayersDamage();
        DoCollisionDamage();
    }

    /// <summary>
    /// Apply damage to the real players
    /// The splash calculation for players is due by distance instead of detect colliders
    /// due to its simpler and performs better.
    /// </summary>
    void DoPlayersDamage()
    {
        List<Player> playersInRange = this.GetPlayersInRange();

        if (playersInRange == null || playersInRange.Count <= 0) return;

        foreach (Player player in playersInRange)
        {
            if (player == null)
            {
                Debug.LogError("Player " + player.NickName + " not found in this room.");
                continue;
            }

            GameObject p = FindPhotonPlayer(player);
            if (p == null) continue;

            var pt = p.transform;
            Vector3 pp = pt.position + Vector3.up;
            //check if there is an obstacle between player and explosion
            if (!ExplosionCanHitTarget(pt, new Vector3(0, 0.6f, 0), pp) && !ExplosionCanHitTarget(pt, new Vector3(0, 0.15f, 0), pp)) continue;

            var pdm = p.transform.GetComponentInParent<bl_PlayerHealthManagerBase>();

            var odi = new DamageData();
            odi.Damage = CalculatePlayerDamage(p.transform, player);
            odi.Direction = transform.position;
            odi.From = creator.Name;
            odi.isHeadShot = false;
            odi.Cause = (!creator.isRealPlayer) ? DamageCause.Bot : DamageCause.Explosion;
            odi.GunID = cachedData.WeaponID;
            odi.Actor = bl_PhotonNetwork.LocalPlayer;

            pdm?.DoDamage(odi);
        }
    }

    /// <summary>
    /// Apply damage to objects, items, bots, etc... if the collider is inside the explosion radius
    /// </summary>
    void DoCollisionDamage()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, explosionRadius, detectLayers, QueryTriggerInteraction.Ignore);
        List<string> Hited = new List<string>();

        foreach (Collider c in colls)
        {
            //the damage to real players is handled separately
            if (c.isLocalPlayerCollider() || c.CompareTag("Untagged")) continue;

            var damageable = c.transform.GetComponent<IMFPSDamageable>();
            if (damageable == null) continue;

            if (c.CompareTag(bl_MFPS.HITBOX_TAG) || c.CompareTag(bl_MFPS.AI_TAG))
            {
                if (Hited.Contains(c.transform.root.name)) continue;
                if (!ExplosionCanHitTarget(c.transform.root, new Vector3(0, 0.6f, 0)) && !ExplosionCanHitTarget(c.transform.root, new Vector3(0, 0.15f, 0))) continue;

                Hited.Add(c.transform.root.name);
            }
            else
            {
                if (!ExplosionCanHitTarget(c.transform, new Vector3(0, 0.1f, 0), CheckRootsOnly)) continue;
            }

            int damage = CalculatePlayerDamage(c.transform, null);
            DamageData damageData = new DamageData()
            {
                Damage = (int)damage,
                Direction = transform.position,
                MFPSActor = creator,
                ActorViewID = creator.ActorViewID,
                GunID = cachedData.WeaponID,
                From = creator.Name,
            };
            damageData.Cause = (!creator.isRealPlayer) ? DamageCause.Bot : DamageCause.Explosion;

            if(damageData.MFPSActor == null)
            {
                Debug.Log($"Explosion actor '{creator.ActorViewID}' was not found in the scene, maybe left the match?");
                return;
            }
            damageable.ReceiveDamage(damageData);
        }
    }

    /// <summary>
    /// When Explosion is local, and take player hit
    /// Send only shake movement
    /// </summary>
    void ApplyShake()
    {
        if (isMyInRange() == true)
        {
            bl_EventHandler.DoPlayerCameraShake(shakerPresent, "shakerKey");
        }
    }

    /// <summary>
    /// calculate the damage it generates, based on the distance
    /// between the player and the explosion
    /// </summary>
    private int CalculatePlayerDamage(Transform trans, Player p)
    {
        if (p != null)
        {
            if (!isOneTeamMode)
            {
                if (bl_GameData.Instance.SelfGrenadeDamage && p == bl_PhotonNetwork.LocalPlayer)
                {

                }
                else
                {
                    if ((string)p.CustomProperties[PropertiesKeys.TeamKey] == myTeam)
                    {
                        return 0;
                    }
                }
            }
        }
        float distance = bl_UtilityHelper.Distance(transform.position, trans.position);
        return Mathf.Clamp((int)(explosionDamage * ((explosionRadius - distance) / explosionRadius)), 0, (int)explosionDamage);
    }

    /// <summary>
    /// Do a simple check to see if there's anything between the explosion and the collider target
    /// if that is the case, the explosion should not make any effect to the target.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    private bool ExplosionCanHitTarget(Transform target, Vector3 offset, bool rootOnly = true)
    {
        return ExplosionCanHitTarget(target, offset, target.position, rootOnly);
    }

    /// <summary>
    /// Do a simple check to see if there's anything between the explosion and the collider target
    /// if that is the case, the explosion should not make any effect to the target.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="offset"></param>
    /// <param name="targetPosition"></param>
    /// <param name="rootOnly"></param>
    /// <returns></returns>
    private bool ExplosionCanHitTarget(Transform target, Vector3 offset, Vector3 targetPosition, bool rootOnly = true)
    {
        bool result = false;
        Vector3 rhs = transform.position + offset;
        Vector3 normalized = ((targetPosition + offset) - rhs).normalized;

        if (Physics.Raycast(rhs, normalized, out hitInfo, explosionRadius))
        {
            if (rootOnly)
            {
                if (hitInfo.transform.root == target) { return true; }
            }
            else
            {
                // If the collider has a rigidbody in the transform root, it will always hinder the raycast
                // so lets ignore it.
                if (target.IsChildOf(hitInfo.transform)) return true;

                if (hitInfo.transform == target) { return true; }
            }
        }
        return result;
    }

    /// <summary>
    /// get players who are within the range of the explosion
    /// </summary>
    /// <returns></returns>
    private List<Player> GetPlayersInRange()
    {
        List<Player> list = new List<Player>();
        foreach (Player p in bl_PhotonNetwork.PlayerList)
        {
            GameObject player = FindPhotonPlayer(p);
            if (player == null)
                return null;

            float distance = bl_UtilityHelper.Distance(transform.position, player.transform.position);
            if (!isOneTeamMode)
            {
                if (!creator.isRealPlayer)
                {
                    if (p.GetPlayerTeam() != creator.Team && (distance <= explosionRadius))
                    {
                        list.Add(p);
                    }
                }
                else
                {
                    if (p != bl_PhotonNetwork.LocalPlayer)
                    {
                        if (p.GetPlayerTeam() != bl_PhotonNetwork.LocalPlayer.GetPlayerTeam() && (distance <= explosionRadius))
                        {
                            list.Add(p);
                        }
                    }
                    else
                    {
                        if (bl_GameData.Instance.SelfGrenadeDamage)
                        {
                            if (distance <= explosionRadius)
                            {
                                list.Add(p);
                            }
                        }
                    }
                }
            }
            else
            {
                if (p != bl_PhotonNetwork.LocalPlayer)
                {
                    if (distance <= explosionRadius)
                    {
                        list.Add(p);
                    }
                }
                else
                {
                    if (bl_GameData.Instance.SelfGrenadeDamage)
                    {
                        if (distance <= explosionRadius)
                        {
                            list.Add(p);
                        }
                    }
                }
            }
        }
        return list;
    }

    /// <summary>
    /// Calculate if player local in explosion radius
    /// </summary>
    /// <returns></returns>
    private bool isMyInRange()
    {
        GameObject p = bl_GameManager.Instance.LocalPlayer;

        if (p == null)
        {
            return false;
        }
        if ((bl_UtilityHelper.Distance(this.transform.position, p.transform.position) <= this.explosionRadius))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    private void SetupAudio()
    {
        var Source = GetComponent<AudioSource>();
        if (Source == null) return;

        Source.spatialBlend = 1;
        Source.maxDistance = bl_AudioController.Instance.maxExplosionDistance;
        Source.rolloffMode = bl_AudioController.Instance.audioRolloffMode;
        Source.minDistance = bl_AudioController.Instance.maxExplosionDistance * 0.09f;
        Source.spatialize = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator Init()
    {
        yield return new WaitForSeconds(DisappearIn / 2);
        Destroy(gameObject);
    }

    [System.Serializable]
    public enum ExplosionType
    {
        Normal,
        Shake,
        Level
    }
}