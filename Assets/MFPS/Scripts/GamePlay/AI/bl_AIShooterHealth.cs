using UnityEngine;
using Photon.Pun;
using MFPS.Runtime.AI;
using MFPS.Audio;

/// <summary>
/// Handle all relating to the bot health
/// This script has not direct references, you can replace with your own script
/// Simply make sure to inherit your script from <see cref="bl_PlayerHealthManagerBase"/>
/// </summary>
public class bl_AIShooterHealth : bl_PlayerHealthManagerBase
{

    [Range(10, 500)] public int Health = 100;

    #region Private members
    private bl_AIShooter m_AIShooter;
    private int LastActorEnemy = -1;
    private RepetingDamageInfo repetingDamageInfo;
    private bl_AIShooterReferences references;
    private bl_AIShooter shooterAgent;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        references = GetComponent<bl_AIShooterReferences>();
        m_AIShooter = references.aiShooter;
        shooterAgent = GetComponent<bl_AIShooter>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damageData"></param>
    public override void DoDamage(DamageData damageData)
    {
        string weaponName = bl_GameData.Instance.GetWeapon(damageData.GunID).Name;
        //if was not killed by a listed weapon
        if (damageData.GunID < 0)
        {
            //try to find the custom weapon name
            var iconData = bl_KillFeedBase.Instance.GetCustomIconByIndex(Mathf.Abs(damageData.GunID + 1));
            if (iconData != null)
            {
                weaponName = $"cmd:{iconData.Name}";
            }
        }

        DoDamage(damageData.Damage, weaponName, damageData.Direction, damageData.ActorViewID, !damageData.MFPSActor.isRealPlayer, damageData.MFPSActor.Team, damageData.isHeadShot);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"></param>
    public override void DoFallDamage(int damage)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoDamage(int damage, string weaponName, Vector3 direction, int viewID, bool fromBot, Team team, bool ishead)
    {
        if (m_AIShooter.isDeath)
            return;

        if (!isOneTeamMode)
        {
            if (!bl_RoomSettings.Instance.CurrentRoomInfo.friendlyFire && team == m_AIShooter.AITeam) return;
        }

        photonView.RPC(nameof(RpcDoDamage), RpcTarget.All, damage, weaponName, direction, viewID, fromBot, ishead);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="weaponName"></param>
    /// <param name="direction"></param>
    /// <param name="viewID"></param>
    /// <param name="fromBot"></param>
    /// <param name="ishead"></param>
    [PunRPC]
    void RpcDoDamage(int damage, string weaponName, Vector3 direction, int viewID, bool fromBot, bool ishead)
    {
        if (m_AIShooter.isDeath)
            return;

        Health -= damage;
        if (LastActorEnemy != viewID)
        {
            if (shooterAgent != null)
                shooterAgent.FocusOnSingleTarget = false;
        }
        LastActorEnemy = viewID;

        if (bl_PhotonNetwork.IsMasterClient)
        {
            shooterAgent?.OnGetHit(direction);
        }
        if (viewID == bl_GameManager.LocalPlayerViewID)//if was me that make damage
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

        if (Health > 0)
        {
            Transform t = bl_GameManager.Instance.FindActor(viewID);
            if (t != null && !t.name.Contains("(die)"))
            {
                if (m_AIShooter.Target == null)
                {
                    if (shooterAgent != null)
                        shooterAgent.FocusOnSingleTarget = true;
                    m_AIShooter.Target = t;
                }
                else
                {
                    if (t != m_AIShooter.Target)
                    {
                        float cd = bl_UtilityHelper.Distance(transform.position, m_AIShooter.Target.position);
                        float od = bl_UtilityHelper.Distance(transform.position, t.position);
                        if (od < cd && (cd - od) > 7)
                        {
                            if (shooterAgent != null)
                                shooterAgent.FocusOnSingleTarget = true;
                            m_AIShooter.Target = t;
                        }
                    }
                }
            }
            references.aiAnimation.OnGetHit();
        }
        else
        {
            Die(viewID, fromBot, ishead, weaponName, direction);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Die(int viewID, bool fromBot, bool ishead, string weaponName, Vector3 direction)
    {
        //Debug.Log($"{gameObject.name} die with {weaponName} from viewID {viewID} Bot?= {fromBot}");
        m_AIShooter.isDeath = true;
        m_AIShooter.OnDeath();
        gameObject.name += " (die)";
        m_AIShooter.AimTarget.name += " (die)";
        references.shooterWeapon.OnDeath();
        m_AIShooter.enabled = false;
        if (bl_PhotonNetwork.IsMasterClient && references.Agent.enabled) references.Agent.isStopped = true;
        GetComponent<bl_NamePlateBase>().SetActive(false);
        //update the MFPSPlayer data
        MFPSPlayer player = bl_GameManager.Instance.GetMFPSPlayer(m_AIShooter.AIName);
        if (player != null)
            player.isAlive = false;

        bl_AIShooter killerBot = null;
        //if was local player who terminated this bot
        if (viewID == bl_GameManager.LocalPlayerViewID && !fromBot)
        {
            Team team = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam();
            //send kill feed message
            int gunID = bl_GameData.Instance.GetWeaponID(weaponName);
            if (weaponName.Contains("cmd:") || gunID == -1)
            {
                weaponName = weaponName.Replace("cmd:", "");
                gunID = -(bl_KillFeedBase.Instance.GetCustomIconIndex(weaponName) + 1);
            }

            var feed = new bl_KillFeedBase.FeedData()
            {
                LeftText = LocalName,
                RightText = m_AIShooter.AIName,
                Team = team
            };
            feed.AddData("gunid", gunID);
            feed.AddData("headshot", ishead);
            
            bl_KillFeedBase.Instance.SendKillMessageEvent(feed);

            var killConsideration = bl_GameData.Instance.howConsiderBotsEliminations;
            if (killConsideration != BotKillConsideration.DoNotCountAtAll)
            {
                if (shooterAgent.AITeam != Team.All && shooterAgent.AITeam != bl_MFPS.LocalPlayer.Team)
                {
                    //Add a new kill and update information
                    bl_PhotonNetwork.LocalPlayer.PostKill(1);//Send a new kill
                    bl_RoomSettings.IncreaseMatchPersistData("bot-kills", 1);
                }
            }

            // only grant score to the kill player if bot eliminations counts as real players.
            if (killConsideration == BotKillConsideration.SameAsRealPlayers)
            {
                int score;
                //If headshot will give you double experience
                if (ishead)
                {
                    bl_GameManager.Instance.Headshots++;
                    score = bl_GameData.Instance.ScoreReward.ScorePerKill + bl_GameData.Instance.ScoreReward.ScorePerHeadShot;
                }
                else
                {
                    score = bl_GameData.Instance.ScoreReward.ScorePerKill;
                }

                if (shooterAgent.AITeam != Team.All && shooterAgent.AITeam != bl_MFPS.LocalPlayer.Team)
                {
                    //Send to update score to player
                    bl_PhotonNetwork.LocalPlayer.PostScore(score);
                }
            }

            //show an local notification for the kill
            var localKillInfo = new KillInfo();
            localKillInfo.Killer = bl_PhotonNetwork.LocalPlayer.NickName;
            localKillInfo.Killed = string.IsNullOrEmpty(m_AIShooter.AIName) ? gameObject.name.Replace("(die)", "") : m_AIShooter.AIName;
            localKillInfo.byHeadShot = ishead;
            localKillInfo.KillMethod = weaponName;
            bl_EventHandler.DispatchLocalKillEvent(localKillInfo);

            //update team score
            bl_GameManager.Instance.SetPoint(1, GameMode.TDM, team);

#if GR
            if (GetGameMode == GameMode.GR)
            {
                bl_GunRace.Instance?.GetNextGun();
            }
#endif
        }
        //if was killed by another bot
        else if (fromBot)
        {
            //make Master handle as the owner
            if (bl_PhotonNetwork.IsMasterClient)
            {
                //find the killer in the scene
                PhotonView p = PhotonView.Find(viewID);
                bl_AIShooter bot = null;
                string killer = "Unknown";
                if (p != null)
                {
                    bot = p.GetComponent<bl_AIShooter>();//killer bot
                    killer = bot.AIName;
                    if (string.IsNullOrEmpty(killer)) { killer = p.gameObject.name.Replace(" (die)", ""); }
                    //update bot stats
                    bl_AIMananger.Instance.SetBotKill(killer);
                }

                //send kill feed message
                int gunID = bl_GameData.Instance.GetWeaponID(weaponName);

                var feed = new bl_KillFeedBase.FeedData()
                {
                    LeftText = killer,
                    RightText = m_AIShooter.AIName,
                    Team = bot.AITeam
                };
                feed.AddData("gunid", gunID);
                feed.AddData("headshot", ishead);

                bl_KillFeedBase.Instance.SendKillMessageEvent(feed);

                if (bot != null)
                {
                    killerBot = bot;
                }
                else
                {
                    Debug.Log("Bot who kill this bot can't be found");
                }
            }
        }//else, (if other player kill this bot) -> do nothing.

        if (bl_GameData.Instance.showDeathIcons && !isOneTeamMode)
        {
            if (m_AIShooter.AITeam == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam())
            {
                GameObject di = bl_ObjectPoolingBase.Instance.Instantiate("deathicon", transform.position, transform.rotation);
            }
        }

        var mplayer = new MFPSPlayer(photonView, false, false);
        bl_EventHandler.DispatchRemotePlayerDeath(mplayer);

        //update the bot deaths count.
        bl_AIMananger.Instance.SetBotDeath(m_AIShooter.AIName);

        if (bl_PhotonNetwork.IsMasterClient)
        {
            //respawn management here
            //only master client called it since the spawn will be sync by PhotonNetwork.Instantiate()
            bl_AIMananger.Instance.OnBotDeath(m_AIShooter, killerBot);

            //Only Master client should send the RPC
            var deathData = bl_UtilityHelper.CreatePhotonHashTable();
            deathData.Add("type", AIRemoteCallType.DestroyBot);
            deathData.Add("direction", direction);
            if (weaponName.Contains("Grenade"))
            {
                deathData.Add("explosion", true);
            }

            //Should buffer this RPC?
            this.photonView.RPC(bl_AIShooterAgent.RPC_NAME, RpcTarget.All, deathData);//callback is in bl_AIShooterAgent.cs -> DestroyBot(...)
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void DestroyEntity()
    {
        var deathData = bl_UtilityHelper.CreatePhotonHashTable();
        deathData.Add("type", AIRemoteCallType.DestroyBot);
        deathData.Add("instant", true);
        this.photonView.RPC(bl_AIShooterAgent.RPC_NAME, RpcTarget.AllBuffered, deathData);//callback is in bl_AIShooterAgent.cs
    }

    /// <summary>
    /// 
    /// </summary>
    public override void DoRepetingDamage(RepetingDamageInfo info)
    {
        repetingDamageInfo = info;
        InvokeRepeating(nameof(MakeDamageRepeting), 0, info.Rate);
    }

    /// <summary>
    /// 
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
        DoDamage(damageinfo.Damage, "[Burn]", damageinfo.Direction, bl_GameManager.LocalPlayerViewID, false, bl_PhotonNetwork.LocalPlayer.GetPlayerTeam(), false);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void CancelRepetingDamage()
    {
        CancelInvoke(nameof(MakeDamageRepeting));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="healthToAdd"></param>
    public override void SetHealth(int healthToAdd, bool overrideHealth)
    {
        if (overrideHealth) Health = healthToAdd;
        else Health += healthToAdd;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool Suicide()
    {
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHealth() => Health;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetMaxHealth() => 100;
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsDeath()
    {
        return m_AIShooter.isDeath;
    }

    [PunRPC]
    void RpcSyncHealth(int _health)
    {
        Health = _health;
    }
}