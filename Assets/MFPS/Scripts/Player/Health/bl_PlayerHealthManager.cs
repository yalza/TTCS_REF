using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using MFPS.Core.Motion;
using MFPS.Audio;
using TMPro;
using MFPSEditor;
#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

public class bl_PlayerHealthManager : bl_PlayerHealthManagerBase
{
    #region Public members
    [Header("Settings")]
    [Range(0, 100)] public int health = 100;
    [Range(1, 100)] public int maxHealth = 100;
    [Range(1, 10)] public float StartRegenerateIn = 4f;
    [Range(1, 5)] public float RegenerationSpeed = 3f;
    [Range(10, 100)] public int RegenerateUpTo = 100;

    [UnityEngine.Header("GUI")]
    /// <summary>
    /// Color of UI when player health is low
    /// </summary>
    public Gradient HealthColorGradient;
    private Color CurColor = new Color(0, 0, 0);
    [Header("Shake")]
    [ScriptableDrawer] public ShakerPresent damageShakerPresent;

    [Header("Effects")]
    public AudioClip[] HitsSound;
    [SerializeField] private AudioClip[] InjuredSounds = null;

    private bool m_HealthRegeneration = false;
    public bool HealthRegeneration { get => m_HealthRegeneration; set => m_HealthRegeneration = value; }
    #endregion

    #region Private members
    private TextMeshProUGUI HealthTextUI;
    private Image HealthBar;
    const string FallMethod = "FallDown";
    private CharacterController m_CharacterController;
    private bool isDead = false;
    private string lastDamageGiverActor;
    private int ScorePerKill, ScorePerHeatShot;
    private bl_PlayerNetwork PlayerSync;
    private float TimeToRegenerate = 4;
    private bl_GunManager GunManager;
    private bool isSuscribed = false;
    private int protecTime = 0;
    private RepetingDamageInfo repetingDamageInfo;
    private CanvasGroup DamageAlpha;
    private float damageAlphaValue, uiFadeDelay = 0;
    private Team thisPlayerTeam = Team.None;
    private bool showIndicator = false;
    private float nextHealthSend = 0;
    private bl_PlayerReferences playerReferences;
#if !ACTK_IS_HERE
     private int currentHealth = 100;
#else
    private ObscuredInt currentHealth = 100;
#endif
#endregion

    #region Unity Callbacks
    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!PhotonNetwork.IsConnected) return;

        base.Awake();
        playerReferences = GetComponent<bl_PlayerReferences>();
        m_CharacterController = playerReferences.characterController;
        PlayerSync = playerReferences.playerNetwork;
        GunManager = playerReferences.gunManager;
        DamageAlpha = bl_UIReferences.Instance.PlayerUI.DamageAlpha;
        m_HealthRegeneration = bl_GameData.Instance.HealthRegeneration;
        protecTime = bl_GameData.Instance.SpawnProtectedTime;
        showIndicator = bl_GameData.Instance.showDamageIndicator;
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (!isConnected)
            return;

        currentHealth = health;
        thisPlayerTeam = (Team)photonView.InstantiationData[0];
        if (isMine)
        {
            bl_MFPS.LocalPlayer.IsAlive = true;
            gameObject.name = PhotonNetwork.NickName;
            HealthTextUI = bl_UIReferences.Instance.PlayerUI.HealthText;
            HealthBar = bl_UIReferences.Instance.PlayerUI.HealthBar;
            UpdateUI();
        }
        if (protecTime > 0) { InvokeRepeating(nameof(OnProtectCount), 1, 1); }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        if (this.isMine)
        {
            bl_GameManager.LocalPlayerViewID = this.photonView.ViewID;
            bl_EventHandler.onPickUpHealth += this.OnPickUp;
            bl_EventHandler.onRoundEnd += this.OnRoundEnd;
            bl_PhotonCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
            isSuscribed = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        if (isSuscribed)
        {
            bl_EventHandler.onPickUpHealth -= this.OnPickUp;
            bl_EventHandler.onRoundEnd -= this.OnRoundEnd;
            bl_PhotonCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (isMine)
        {
            DamageUI();
            RegenerateHealth();
        }
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damageData"></param>
    public override void DoDamage(DamageData damageData)
    {
        GetDamage(damageData);
    }

    /// <summary>
    /// Call this to make a new damage to the player
    /// </summary>
    public void GetDamage(DamageData e)
    {
        bool canDamage = true;
        if (!DamageEnabled)
        {
            //Fix: bots can't damage Master Client teammates.
            if (e.MFPSActor != null && (e.MFPSActor.Team != thisPlayerTeam || bl_RoomSettings.Instance.CurrentRoomInfo.friendlyFire)) { canDamage = true; }
            else canDamage = false;
        }

        if (!canDamage || isProtectionEnable || bl_GameManager.Instance.GameMatchState == MatchState.Waiting)
        {
            if (!isMine)
            {
                playerReferences.playerAnimations.OnGetHit();
            }
            return;
        }

        photonView.RPC(nameof(SyncDamage), RpcTarget.AllBuffered, e.Damage, e.From, e.Cause, e.Direction, e.isHeadShot, e.GunID, bl_PhotonNetwork.LocalPlayer);
    }

    /// <summary>
    /// Call this when Player Take Damage From fall impact
    /// </summary>
    public override void DoFallDamage(int damage)
    {
        if (!bl_GameData.Instance.allowFallDamage || isProtectionEnable || playerReferences.firstPersonController.State == PlayerState.InVehicle || bl_GameManager.Instance.GameMatchState == MatchState.Waiting)
            return;

        Vector3 downpos = transform.position - transform.TransformVector(new Vector3(0, 5, 1));
        photonView.RPC(nameof(SyncDamage), RpcTarget.AllBuffered, damage, bl_PhotonNetwork.NickName, DamageCause.FallDamage, downpos, false, 103, bl_PhotonNetwork.LocalPlayer);
    }

    /// <summary>
    /// This function is called on all clients when this player receive damage
    /// </summary>
    [PunRPC]
    void SyncDamage(int damage, string killer, DamageCause cause, Vector3 hitDirection, bool isHeatShot, int weaponID, Player m_sender)
    {
        if (isDead || isProtectionEnable)
            return;

        if (DamageEnabled)
        {
            if (currentHealth >= 0)
            {
                if (isMine)
                {
                    damageAlphaValue = Mathf.Max(1f - (((float)currentHealth - (float)damage) / (float)maxHealth), 0.25f);
                    uiFadeDelay = StartRegenerateIn;
                    bl_EventHandler.DoPlayerCameraShake(damageShakerPresent, "damage");
                    if (showIndicator) bl_DamageIndicatorBase.Instance?.SetHit(hitDirection);
                    TimeToRegenerate = StartRegenerateIn;
                    bl_EventHandler.DispatchLocalPlayerReceiveDamage(damage);
                }
                else
                {
                    // When the damage is given by the local player
                    if (m_sender != null && m_sender.NickName == LocalName && cause != DamageCause.Bot)
                    {
                        bl_CrosshairBase.Instance.OnHit();
                        bl_AudioController.Instance.PlayClip("body-hit");
                        bl_EventHandler.DispatchLocalPlayerHitEnemy(new MFPSHitData()
                        {
                            HitTransform = transform,
                            HitPosition = transform.position,
                            Damage = damage,
                            HitName = gameObject.name
                        });
                    }
                }
            }

            if (cause != DamageCause.FallDamage && cause != DamageCause.Fire)
            {
                if (HitsSound.Length > 0)//Audio effect of hit
                {
                    AudioSource.PlayClipAtPoint(HitsSound[Random.Range(0, HitsSound.Length)], transform.position, 1.0f);
                }
            }
            else
            {
                AudioSource.PlayClipAtPoint(InjuredSounds[Random.Range(0, InjuredSounds.Length)], transform.position, 1.0f);
            }

            lastDamageGiverActor = killer;
        }

        if (currentHealth > 0)
        {
            currentHealth -= damage;
            if (!isMine)
            {
                playerReferences.playerAnimations.OnGetHit();
            }
            else
            {
                UpdateUI();
            }
        }

        if (currentHealth < 1)
        {
            currentHealth = 0;

            if (isMine)
            {
                bl_MFPS.LocalPlayer.IsAlive = false;
                bl_EventHandler.DispatchPlayerLocalDeathEvent();
            }

            Die(lastDamageGiverActor, isHeatShot, cause, weaponID, hitDirection, m_sender);
        }
    }

    /// <summary>
    /// This is called when this player die in match
    /// This function determine the cause of death and update the require stats
    /// </summary>
	void Die(string killer, bool isHeadshot, DamageCause cause, int gunID, Vector3 hitPos, Player sender)
    {
        // Debug.Log($"{gameObject.name} die cause {cause.ToString()} from {killer} and GunID {gunID}");
        isDead = true;
        transform.parent = null;
        m_CharacterController.enabled = false;
        bl_GameManager.Instance.GetMFPSPlayer(gameObject.name).isAlive = false;
        bl_GunInfo gunInfo = bl_GameData.Instance.GetWeapon(gunID);
        bool isExplosion = gunInfo.Type == GunType.Grenade || gunInfo.Type == GunType.Launcher;

        if (!isMine)
        {
            playerReferences.playerRagdoll.Ragdolled(new bl_PlayerRagdollBase.RagdollInfo()
            {
                ForcePosition = hitPos,
                IsFromExplosion = isExplosion,
                AutoDestroy = true
            });// convert into ragdoll the remote player
        }
        else
        {
            //Make the remote players drop their weapon
            Transform ngr = (bl_GameData.Instance.DropGunOnDeath) ? null : PlayerSync.NetGunsRoot;
            playerReferences.playerRagdoll.SetLocalRagdoll(new bl_PlayerRagdollBase.RagdollInfo()
            {
                ForcePosition = hitPos,
                Velocity = m_CharacterController.velocity,
                IsFromExplosion = isExplosion,
                AutoDestroy = true,
                RightHandChild = ngr
            });
        }

        //disable all other player prefabs child's
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        string weapon = cause.ToString();
        if (cause == DamageCause.Player || cause == DamageCause.Bot || cause == DamageCause.Explosion)
        {
            weapon = gunInfo.Name;
        }

        if (!isMine)// when player is not ours
        {
            //if the local player was how kill this player
            if (lastDamageGiverActor == LocalName)
            {
                AddKill(isHeadshot, weapon, gunID);
            }

            if (bl_GameData.Instance.showDeathIcons && !isOneTeamMode)
            {
                if (photonView.Owner.GetPlayerTeam() == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam())
                {
                    bl_ObjectPoolingBase.Instance.Instantiate("deathicon", transform.position, transform.rotation);
                }
            }

            var mplayer = new MFPSPlayer(photonView, true, false);
            bl_EventHandler.DispatchRemotePlayerDeath(mplayer);
        }
        else//when is local player who dies
        {

            //Set to respawn again
            if (GetGameMode.GetGameModeInfo().onPlayerDie == GameModeSettings.OnPlayerDie.SpawnAfterDelay)
            {
                bl_GameManager.Instance.RespawnLocalPlayerAfter();
            }

            if (cause == DamageCause.Bot)
            {
                // increase the deaths count for the local player
               if(bl_GameData.Instance.howConsiderBotsEliminations == MFPS.Runtime.AI.BotKillConsideration.SameAsRealPlayers) 
                    bl_PhotonNetwork.LocalPlayer.PostDeaths(1);
            }
            else
            {
                // increase the deaths count for the local player
                bl_PhotonNetwork.LocalPlayer.PostDeaths(1);
            }

            playerReferences.playerRagdoll.gameObject.name = "YOU";

            //Show the kill camera
            bl_KillCamBase.Instance.SetTarget(new bl_KillCamBase.KillCamInfo()
            {
                RealPlayer = sender,
                TargetName = killer,
                GunID = gunID,
                Target = transform,
                FallbackTarget = playerReferences.playerRagdoll.transform
            }).SetActive(true);

#if ELIM
            if (GetGameMode == GameMode.ELIM) { bl_Elimination.Instance.OnLocalDeath(); }
#endif
#if DM
            if (GetGameMode == GameMode.DM) { bl_Demolition.Instance.OnLocalDeath(); }
#endif

            if (killer == LocalName)
            {
                if (cause == DamageCause.FallDamage)
                {
                    bl_KillFeedBase.Instance.SendTeamHighlightMessage(LocalName, bl_GameTexts.DeathByFall.Localized(20), bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());

                }
                else
                {
                    bl_KillFeedBase.Instance.SendTeamHighlightMessage(LocalName, bl_GameTexts.CommittedSuicide.Localized(19), bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());
                }
            }

            if (bl_GameData.Instance.DropGunOnDeath)
            {
                GunManager.ThrowCurrent(true, PlayerSync.NetGunsRoot.position + transform.forward);
            }

            if (cause == DamageCause.Bot)
            {
                var feed = new bl_KillFeedBase.FeedData()
                {
                    LeftText = killer,
                    RightText = gameObject.name,
                    Team = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam()
                };
                feed.AddData("gunid", gunID);
                feed.AddData("headshot", isHeadshot);

                bl_KillFeedBase.Instance.SendKillMessageEvent(feed);
                //the local player will update the bot stats instead of the Master in this case.
                bl_AIMananger.Instance.SetBotKill(killer);
            }
            StartCoroutine(DestroyThis());
        }
    }

    /// <summary>
    /// when we get a new kill, synchronize and add points to the player
    /// </summary>
    public void AddKill(bool isHeadshot, string m_weapon, int gunID)
    {
        //send kill feed kill message
        var feed = new bl_KillFeedBase.FeedData()
        {
            LeftText = LocalName,
            RightText = gameObject.name,
            Team = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam()
        };
        feed.AddData("gunid", gunID);
        feed.AddData("headshot", isHeadshot);

        bl_KillFeedBase.Instance.SendKillMessageEvent(feed);

        //Add a new kill and update the player information
        bl_PhotonNetwork.LocalPlayer.PostKill(1);
        // calculate the score gained for this kill
        int score = (isHeadshot) ? bl_GameData.Instance.ScoreReward.ScorePerKill + bl_GameData.Instance.ScoreReward.ScorePerHeadShot : bl_GameData.Instance.ScoreReward.ScorePerKill;

        //show an local notification for the kill
        var localKillInfo = new KillInfo();
        localKillInfo.Killer = lastDamageGiverActor;
        localKillInfo.Killed = gameObject.name;
        localKillInfo.byHeadShot = isHeadshot;
        localKillInfo.KillMethod = m_weapon;
        bl_EventHandler.DispatchLocalKillEvent(localKillInfo);

        var elimatedTeam = photonView.Owner.GetPlayerTeam();
        if (elimatedTeam != Team.All && elimatedTeam != bl_PhotonNetwork.LocalPlayer.GetPlayerTeam())
        {
            // add the score to the player total gained score in this match
            bl_PhotonNetwork.LocalPlayer.PostScore(score);
        }

        bl_GameManager.Instance.OnLocalPlayerKill();

#if GR
        if (GetGameMode == GameMode.GR)
        {
            bl_GunRace.Instance?.GetNextGun();
        }
#endif
    }

    /// <summary>
    /// Do constant damage to the player in a loop until cancel.
    /// </summary>
    public override void DoRepetingDamage(RepetingDamageInfo info)
    {
        repetingDamageInfo = info;
        InvokeRepeating(nameof(MakeDamageRepeting), 0, info.Rate);
    }

    /// <summary>
    /// Apply damage from a custom loop
    /// </summary>
    void MakeDamageRepeting()
    {
        if (repetingDamageInfo == null)
        {
            CancelRepetingDamage();
            return;
        }

        var damageinfo = repetingDamageInfo.DamageData;
        if (damageinfo == null)
        {
            damageinfo = new DamageData();
            damageinfo.Direction = Vector3.zero;
            damageinfo.Cause = DamageCause.Map;
        }
        damageinfo.Damage = repetingDamageInfo.Damage;

        GetDamage(damageinfo);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void CancelRepetingDamage()
    {
        CancelInvoke(nameof(MakeDamageRepeting));
    }

    /// <summary>
    /// Update the player health UI with this player stats
    /// </summary>
    void UpdateUI()
    {
        float h = Mathf.Max(currentHealth, 0);
        float deci = h * 0.01f;
        CurColor = HealthColorGradient.Evaluate(deci);
        if (HealthTextUI != null)
        {
            HealthTextUI.text = Mathf.FloorToInt(currentHealth).ToString();
            HealthTextUI.color = CurColor;
        }
        if (HealthBar != null) { HealthBar.fillAmount = deci; HealthBar.color = CurColor; }
    }

    /// <summary>
    /// 
    /// </summary>
    void DamageUI()
    {
        if (damageAlphaValue <= 0)
        {
            DamageAlpha.alpha = 0;
            return;
        }

        DamageAlpha.alpha = Mathf.Lerp(DamageAlpha.alpha, damageAlphaValue, Time.deltaTime * 6);
        
        if (uiFadeDelay <= 0)
        {
            damageAlphaValue -= Time.deltaTime;
        }
        else
        {
            uiFadeDelay -= Time.deltaTime;          
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void RegenerateHealth()
    {
        if (!m_HealthRegeneration || currentHealth < 1) return;
        if (currentHealth >= RegenerateUpTo) return;

        int projectedHealth = currentHealth;
        if (TimeToRegenerate <= 0)
        {
            projectedHealth = currentHealth;
            projectedHealth += 1;
            uiFadeDelay = 0;
        }
        else
        {
            TimeToRegenerate -= Time.deltaTime * 1.15f;
        }

        if (Time.time - nextHealthSend >= (1 / RegenerationSpeed))
        {
            nextHealthSend = Time.time;
            photonView.RPC(nameof(PickUpHealth), RpcTarget.All, projectedHealth);
        }
        UpdateUI();
    }

    /// <summary>n
    /// Make the local player kill himself
    /// </summary>
    public override bool Suicide()
    {
        if (!isMine || !bl_MFPS.LocalPlayer.IsAlive) return false;
        if (isProtectionEnable) return false;

        DamageData e = new DamageData();
        e.Damage = 500;
        e.From = base.LocalName;
        e.Direction = transform.position;
        e.isHeadShot = false;
        GetDamage(e);
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnProtectCount()
    {
        protecTime--;
        if (isMine)
        {
            bl_UIReferences.Instance.OnSpawnCount(protecTime);
        }
        if (protecTime <= 0)
        {
            CancelInvoke(nameof(OnProtectCount));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroyThis()
    {
        yield return new WaitForSeconds(0.3f);
        DestroyEntity();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void DestroyEntity()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }

    /// <summary>
    /// This event is called when player pick up a med kit
    /// </summary>
    /// <param name="amount"> amount for sum at current health</param>
    void OnPickUp(int amount)
    {
        SetHealth(amount);
    }

    /// <summary>
    /// Add health to this player and sync with all clients
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="replace"></param>
    public override void SetHealth(int amount, bool replace = false)
    {
        if (currentHealth < 1 || isDead) return;

        if (photonView.IsMine)
        {
            int newHealth = currentHealth + amount;
            if (replace) newHealth = amount;

            currentHealth = newHealth;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
                damageAlphaValue = 1;
            }
            uiFadeDelay = 0;
            UpdateUI();
            photonView.RPC(nameof(PickUpHealth), RpcTarget.OthersBuffered, newHealth);
        }
    }

    [PunRPC]
    void RpcSyncHealth(int newHealth, PhotonMessageInfo info)
    {
        if (info.photonView.ViewID == photonView.ViewID)
        {
            currentHealth = (int)newHealth;
        }
    }

    /// <summary>
    /// Sync Health when pick up a med kit.
    /// </summary>
    [PunRPC]
    void PickUpHealth(int t_amount)
    {
        if (currentHealth < 1 || isDead) return;

        currentHealth = (int)t_amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    private bool isProtectionEnable { get { return (protecTime > 0); } }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHealth() => currentHealth;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetMaxHealth() => maxHealth;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsDeath()
    {
        return isDead;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="newPlayer"></param>
    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RpcSyncHealth), newPlayer, (int)currentHealth);
        }
    }

    /// <summary>
    /// When round is end 
    /// desactive some functions
    /// </summary>
    void OnRoundEnd()
    {
        DamageEnabled = false;
    }
}