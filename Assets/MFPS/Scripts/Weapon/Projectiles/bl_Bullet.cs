using UnityEngine;

/// <summary>
/// This the default bullet handler of MFPS
/// If you want to use your custom bullet script, simply append the bl_ProjectileBase class to it
/// Handle the override functions and attach it to a bullet prefab
/// Follow the Bullet documentation to see how to use a custom bullet prefab.
/// You can use this script as reference to see how to apply damage to players and bots.
/// </summary>
public class bl_Bullet : bl_ProjectileBase
{
    #region Public members
    /// <summary>
    /// If is enabled, on all collisions will check if the hitted object have a IMFPSDamageable component
    /// </summary>
    [LovattoToogle] public bool checkDamageables = false;
    public LayerMask HittableLayers;
    public TrailRenderer Trail = null;
    #endregion

    #region Public properties
    public int ActorViewID { get; set; }
    #endregion

    #region Private members
    private Vector3 dir = Vector3.zero;
    private RaycastHit hit;
    private Transform m_Transform;
    private float distanceSinceLastCheck = 0;
    private float totalTraveledDistance = 0;
    private BulletData bulletData;
    private Vector3 velocity, newVelEuler = Vector3.zero; // bullet velocity
    private Vector3 newPos = Vector3.zero;   // bullet's new position
    private Vector3 oldPos = Vector3.zero;   // bullet's previous location
    private bool hasHit = false;             // has the bullet hit something?
    private Vector3 direction, gravity;               // direction bullet is travelling
    private float bulletRange, timeStep = 0;
    private bool realisticBallistics = false;
    private int fixedFrame = 0;
    private int detectionStep = 1;
    private LayerMask bulletMask;
    private int localLayer = -2;
    #endregion

    /// <summary>
    /// Initialize the bullet with the given info from the weapon
    /// </summary>
    public override void InitProjectile(BulletData data)
    {
        // Since the bullet is pooled, make sure to reset all the variables to its initial values.
        ResetBullet();

        m_Transform = transform;
        bulletData = data;

        ActorViewID = data.ActorViewID;

        // direction bullet is traveling, add some spread to it by the weapon accuracy
        direction = m_Transform.TransformDirection(data.Inaccuracity);
        bulletRange = data.Range;
        newPos = m_Transform.position;
        oldPos = newPos;
        velocity = data.Speed * m_Transform.forward; // bullet's velocity determined by direction and bullet speed
        // apply the spread direction
        velocity += direction;
        gravity = bl_MathUtility.Gravity * data.DropFactor;

        realisticBallistics = bl_GameData.Instance.realisticBullets;
        detectionStep = bl_GameData.Instance.BulletHitDetectionStep;

        bulletMask = HittableLayers;
        if (data.IsLocalPlayer)
        {
            if (localLayer == -2) localLayer = bl_GameData.TagsAndLayerSettings.GetLocalPlayerLayerIndex();
            bulletMask = HittableLayers & ~(1 << localLayer);
        }

        if (Trail != null) { if (!bl_GameData.Instance.BulletTracer) { Destroy(Trail); } }
        // schedule for destruction if bullet never hits anything
        Invoke(nameof(Disable), 5);//TTL

        //bl_BulletDebugger.CreateBullet(this);
    }

    /// <summary>
    /// Set all the bullet properties to their initial values.
    /// </summary>
    void ResetBullet()
    {
        bulletData = null;
        hasHit = false;
        ActorViewID = 0;
        totalTraveledDistance = 0;
        fixedFrame = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (hasHit)
            return; // if bullet has already hit its max hits... exit

        Travel();
    }

    /// <summary>
    /// Determine the ballistic movement
    /// Detect if the bullet hit anything during the travel.
    /// </summary>
    void Travel()
    {
        timeStep = Time.fixedDeltaTime;
        if (!realisticBallistics)
        {
            // Calculate the linear ballistic trajectory
            // No external or gravity forces applied here.
            newPos += velocity * timeStep;
        }
        else
        {
            //Calculate the new velocity and position using Heuns method's
            //y_k+1 = y_k + timeStep * 0.5 * (f(t_k, y_k) + f(t_k+1, y_k+1))
            //Where f(t_k+1, y_k+1) is calculated with Forward Euler: y_k+1 = y_k + timeStep * f(t_k, y_k)
            newVelEuler = velocity + timeStep * gravity;
            Vector3 newVel = velocity + timeStep * 0.5f * (gravity + gravity);

            newPos = newPos + timeStep * 0.5f * (velocity + newVelEuler);
            velocity = newVel;
        }

        if (fixedFrame % detectionStep == 0)
        {
            // get the traveled trajectory since last check
            dir = newPos - oldPos;
            distanceSinceLastCheck = dir.magnitude;
            totalTraveledDistance += distanceSinceLastCheck;

            if (distanceSinceLastCheck > 0)
            {
                //bl_BulletDebugger.UpdateBullet(this);

                // Check if we hit anything on the way
                dir /= distanceSinceLastCheck;
                if (Physics.Raycast(oldPos, dir, out hit, distanceSinceLastCheck, bulletMask, QueryTriggerInteraction.Ignore))
                {
                    newPos = hit.point;
                    if (OnHit(hit))
                    {
                        hasHit = true;
                        Disable();
                    }
                }
            }

            if (bulletRange > 0 && totalTraveledDistance > bulletRange)
            {
                Disable();
            }

            oldPos = m_Transform.position;  // set old position to current position
        }

        fixedFrame++;
        m_Transform.position = newPos;  // set current position to the new position
    }

    /// <summary>
    /// 
    /// </summary>
    private void Disable()
    {
        ResetBullet();
        gameObject.SetActive(false);
    }

    #region Bullet On Hits

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    private bool OnHit(RaycastHit hit)
    {
        Ray mRay = new Ray(m_Transform.position, m_Transform.forward);
        if (!bulletData.isNetwork)
        {
            if (hit.rigidbody != null && !hit.rigidbody.isKinematic) // if we hit a rigi body... apply a force
            {
                float mAdjust = 1.0f / (Time.timeScale * (0.02f / Time.fixedDeltaTime));
                hit.rigidbody.AddForceAtPosition(((mRay.direction * bulletData.ImpactForce) / Time.timeScale) / mAdjust, hit.point);
            }
        }

        switch (hit.transform.tag) // decide what the bullet collided with and what to do with it
        {
            case "IgnoreBullet":
                return false;
            case "Projectile":
                // do nothing if 2 bullets collide
                break;
            case bl_MFPS.HITBOX_TAG://Send Damage for other players
                SendPlayerDamage(hit);
                break;
            case "AI":
                //Bullet hit a bot collider, check if we have to apply damage.
                SendBotDamage(hit);
                break;
            case "Player":
                //A bot hit a real player
                SendPlayerDamageFromBot(hit);
                break;
            case "Wood":
                InstanceHitParticle("decalw", hit);
                break;
            case "Concrete":
                InstanceHitParticle("decalc", hit);
                break;
            case "Metal":
                InstanceHitParticle("decalm", hit, true);
                break;
            case "Dirt":
                InstanceHitParticle("decals", hit);
                break;
            case "Water":
                InstanceHitParticle("decalwt", hit);
                break;
            default:
                InstanceHitParticle("decal", hit);
                break;
        }
        Disable();
        return true;
    }
    #endregion

    /// <summary>
    /// Apply damage to real players from a bot or another real player
    /// </summary>
    void SendPlayerDamage(RaycastHit hit)
    {
        if (bulletData.isNetwork) return;

        IMFPSDamageable damageable = hit.transform.GetComponent<IMFPSDamageable>();
        if (damageable != null)
        {
            DamageData damageData = BuildBaseDamageData();
            //check if the bullet comes from a bot or a real player.
            if (bulletData.MFPSActor != null)
            {
                damageData.Cause = bulletData.MFPSActor.isRealPlayer ? DamageCause.Player : DamageCause.Bot;
            }
            else
            {
                damageData.Cause = DamageCause.Player;
            }
            damageable.ReceiveDamage(damageData);
        }

        if (bl_GameData.Instance.ShowBlood)
        {
            GameObject go = bl_ObjectPoolingBase.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            go.transform.parent = hit.transform;
        }
        Disable();
    }

    /// <summary>
    /// Apply damage to a bot from a Real Player or Another bot
    /// </summary>
    void SendBotDamage(RaycastHit hit)
    {
        //apply damage only if this is the Local Player, we could use only On Master Client to have a more "Authoritative" logic but since the
        //Ping of Master Clients can be volatile since it depend of the client connection, that could affect all the players in the room.
        if (!bulletData.isNetwork)
        {
            if (bulletData.MFPSActor == null)
            {
                Debug.LogWarning($"Bullet ownerID {bulletData.ActorViewID} not found, ignore this if a player just enter in the match.");
            }
            if (hit.transform.root != null && bulletData.MFPSActor.Name == hit.transform.root.name) { return; }
            IMFPSDamageable damageable = hit.transform.GetComponent<IMFPSDamageable>();

            if (damageable != null)
            {
                //callback is in bl_HitBox.cs
                var damageData = BuildBaseDamageData();
                damageable.ReceiveDamage(damageData);
            }
        }

        GameObject go = bl_ObjectPoolingBase.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
        go.transform.parent = hit.transform;
        Disable();
    }

    /// <summary>
    /// Apply damage to real players FROM bots.
    /// </summary>
    void SendPlayerDamageFromBot(RaycastHit hit)
    {
        if (bulletData.isNetwork) return;
        //Bots doesn't hit the players HitBoxes like other real players does, instead they hit the Character Controller Collider,
        //So instead of communicate with the hit box script we have to communicate with the player health script directly
        var pdm = hit.transform.GetComponent<bl_PlayerReferences>();
        if (pdm != null && bulletData.MFPSActor != null && !bulletData.MFPSActor.isRealPlayer)
        {
            if (!isOneTeamMode)
            {
                if (!bl_RoomSettings.Instance.CurrentRoomInfo.friendlyFire && pdm.playerSettings.PlayerTeam == bulletData.MFPSActor.Team)//if hit a team mate player
                {
                    Disable();
                    return;
                }
            }
            DamageData info = BuildBaseDamageData();
            info.Actor = null;
            info.Cause = DamageCause.Bot;
            pdm.playerHealthManager.DoDamage(info);
            Disable();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void InstanceHitParticle(string poolPrefab, RaycastHit hit, bool overrideDamageable = false)
    {
        // instantiate particle
        GameObject go = bl_ObjectPoolingBase.Instance.Instantiate(poolPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        // instance decal
        bl_BulletDecalManagerBase.InstantiateDecal(hit);

        if (go != null)
            go.transform.parent = hit.transform;

        if (checkDamageables || overrideDamageable)
        {
            var damageable = hit.transform.GetComponent<IMFPSDamageable>();
            if (damageable == null) return;
            DamageData damageData = BuildBaseDamageData();
            damageData.Cause = DamageCause.Player;
            damageable.ReceiveDamage(damageData);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    DamageData BuildBaseDamageData()
    {
        DamageData data = new DamageData()
        {
            Damage = (int)bulletData.Damage,
            Direction = bulletData.Position,
            MFPSActor = bulletData.MFPSActor,
            ActorViewID = bulletData.ActorViewID,
            GunID = bulletData.WeaponID,
        };
        if (data.MFPSActor != null) { data.From = data.MFPSActor.Name; }
        return data;
    }   
}