using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MFPSEditor;
using UnityEngine.Serialization;
using MFPS.Runtime.Settings;
using MFPS.Internal.Structures;
using MFPS.Internal.Scriptables;
using MFPS.Internal.BaseClass;
using MFPS.Runtime.AI;

public class bl_GameData : ScriptableObject
{
    #region Public members
    [Header("Game Settings")]
    [LovattoToogle] public bool offlineMode = false;
    [LovattoToogle] public bool verifySingleSession = true;
    [LovattoToogle] public bool UseLobbyChat = true;
    [LovattoToogle] public bool UseVoiceChat = true;
    [LovattoToogle] public bool BulletTracer = false;
    [LovattoToogle] public bool DropGunOnDeath = true;
    [LovattoToogle] public bool SelfGrenadeDamage = true;
    [LovattoToogle] public bool CanFireWhileRunning = true;
    [LovattoToogle] public bool HealthRegeneration = true;
    [LovattoToogle] public bool ShowTeamMateHealthBar = true;
    [LovattoToogle] public bool CanChangeTeam = false;
    [LovattoToogle] public bool ShowBlood = true;
    [LovattoToogle] public bool DetectAFK = false;
    [LovattoToogle] public bool MasterCanKickPlayers = true;
    [LovattoToogle] public bool ArriveKitsCauseDamage = true;
    [LovattoToogle] public bool CalculateNetworkFootSteps = false;
    [LovattoToogle] public bool ShowNetworkStats = false;
    [LovattoToogle] public bool RememberPlayerName = true;
    [LovattoToogle] public bool ShowWeaponLoadout = true;
    [LovattoToogle] public bool useCountDownOnStart = true;
    [LovattoToogle] public bool showCrosshair = true;
    [LovattoToogle] public bool showDamageIndicator = true;
    [LovattoToogle] public bool doSpawnHandMeshEffect = true;
    [LovattoToogle] public bool playerCameraWiggle = true;
    [LovattoToogle] public bool showDeathIcons = true;
    [LovattoToogle] public bool allowFallDamage = true;
    [LovattoToogle] public bool realisticBullets = true;
    [LovattoToogle] public bool bulletDecals = true;
    [LovattoToogle] public bool showProjectilesTrails = true;
#if MFPSM
    [LovattoToogle] public bool AutoWeaponFire = false;
#endif
    public AmmunitionType AmmoType = AmmunitionType.Bullets;
    public KillFeedWeaponShowMode killFeedWeaponShowMode = KillFeedWeaponShowMode.WeaponIcon;
    public LobbyJoinMethod lobbyJoinMethod = LobbyJoinMethod.WaitingRoom;
    public bl_KillCam.KillCameraType killCameraType = bl_KillCam.KillCameraType.ObserveDeath;
    public bl_LocalKillNotifier.LocalKillDisplay localKillsShowMode = bl_LocalKillNotifier.LocalKillDisplay.Queqe;
    public bl_GunManager.AutoChangeOnPickup switchToPickupWeapon = bl_GunManager.AutoChangeOnPickup.OnlyOnEmptySlots;
    public bl_SpectatorModeBase.LeaveSpectatorModeAction onLeaveSpectatorMode = bl_SpectatorModeBase.LeaveSpectatorModeAction.AllowSelectTeam;
    /// <summary>
    /// Determine how the bot eliminations are considered for real player stats
    /// If you want to avoid players being able to level up exploiting bots eliminations to increase their personal stats.
    /// </summary>
    public BotKillConsideration howConsiderBotsEliminations = BotKillConsideration.SameAsRealPlayers;

    [Header("Rewards")]
    public ScoreRewards ScoreReward;
    public VirtualCoin VirtualCoins;
#if UNITY_2020_2_OR_NEWER
    [NonReorderable]
#endif
    [ScriptableDrawer] public List<MFPSCoin> gameCoins;

    [Header("Settings")]
    public string GameVersion = "1.0";
    public string guestNameFormat = "Guest {0}";
    public string botsNameFormat = "BOT {0}";
    [Range(0, 10)] public int SpawnProtectedTime = 5; // Time were the player can't receive damage after spawn.
    [Range(1, 60)] public int CountDownTime = 7; // Initial countdown time (when countdown is enabled)
    [Range(1, 10)] public float PlayerRespawnTime = 5.0f; // Time that takes to respawn after the player die
    [Range(1, 100)] public int MaxFriendsAdded = 25; // Maximum number of friends that players can add (and save)
    [Range(1, 10)] public int BulletHitDetectionStep = 2; // Each how many frames detect bullets hits? 1 = each frame
    public int afterMatchCountdown = 10; // The time that the game resume shown after the match finish.
    public float AFKTimeLimit = 60; // Time limit that a player can be AFK in a match.
    public int MaxChangeTeamTimes = 3; // Max number of times that a player can change of team per match.
    public int maxSuicideAttempts = 3; // Max number of times that a player can suicide per match.
    public string MainMenuScene = "MainMenu";
    public string OnDisconnectScene = "MainMenu";
    public Color highLightColor = Color.green; // Color to highlight the local player in different UI elements.

    [Header("Levels Manager")]
    [Reorderable]
    public List<MapInfo> AllScenes = new List<MapInfo>();

    [Header("Weapons")]
#if UNITY_2020_2_OR_NEWER
    [NonReorderable] // DO NOT reorder the weapons manually, it will mess up your whole setup!
#endif
    public List<bl_GunInfo> AllWeapons = new List<bl_GunInfo>();

    [Header("Default Settings")]
    [ScriptableDrawer, SerializeField] private bl_RuntimeSettingsProfile defaultSettings;

    [Header("Game Modes Available"), Reorderable, FormerlySerializedAs("AllGameModes")]
    public List<GameModeSettings> gameModes = new List<GameModeSettings>();

    [Header("Teams")]
    public string Team1Name = "Team1";
    public Color Team1Color = Color.blue;
    [Space(5)]
    public string Team2Name = "Team2";
    public Color Team2Color = Color.green;

    [Header("Players")]
    public bl_PlayerNetwork Player1;
    public bl_PlayerNetwork Player2;

    [Header("Bots")]
    public bl_AIShooter BotTeam1;
    public bl_AIShooter BotTeam2;

    [ScriptableDrawer] public MouseLookSettings mouseLookSettings;

    [ScriptableDrawer] public bl_WeaponSlotRuler weaponSlotRuler;

    [ScriptableDrawer, SerializeField] private TagsAndLayersSettings tagsAndLayersSettings;

    public bl_SceneLoaderBase sceneLoader;

    [Header("Game Team")]
    public List<GameTeamInfo> GameTeam = new List<GameTeamInfo>();
    #endregion

    #region Private Members
    public GameTeamInfo CurrentTeamUser { get; set; } = null;
    [HideInInspector] public bool isChating = false;
    [HideInInspector] public string _MFPSLicense = string.Empty;
    [HideInInspector] public int _MFPSFromStore = 2;
    [HideInInspector] public string _keyToken = "";
    private static bl_PhotonNetwork PhotonGameInstance = null;
    public static bool isDataCached = false;
    private static bool isCaching = false;
    private static bl_GameData m_instance;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnRuntimeInit()
    {
        //check that there's an instance of the Photon object in scene
        if (PhotonGameInstance == null && Application.isPlaying)
        {
            PhotonGameInstance = bl_PhotonNetwork.Instance;
            if (PhotonGameInstance == null)
            {
                try
                {
                    var pgo = new GameObject("PhotonGame");
                    PhotonGameInstance = pgo.AddComponent<bl_PhotonNetwork>();
                }
                catch (Exception)
                {
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private static void OnDataCached()
    {
        if (!bl_UtilityHelper.isMobile && Instance.mouseLookSettings.customCursor != null)
        {
            Cursor.SetCursor(Instance.mouseLookSettings.customCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        isDataCached = false;
        RolePrefix = "";
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetInstance()
    {
        m_settingProfile = null;
        isDataCached = false;
    }

    #region Getters
    /// <summary>
    /// Get a weapon info by they ID
    /// </summary>
    /// <param name="ID">the id of the weapon, this ID is the indexOf the weapon info in GameData</param>
    /// <returns></returns>
    public bl_GunInfo GetWeapon(int ID)
    {
        if (ID < 0 || ID > AllWeapons.Count - 1)
            return AllWeapons[0];

        return AllWeapons[ID];
    }

    /// <summary>
    /// Get a weapon info by they Name
    /// </summary>
    /// <param name="gunName"></param>
    /// <returns></returns>
    public int GetWeaponID(string gunName)
    {
        int id = -1;
        if (AllWeapons.Exists(x => x.Name == gunName))
        {
            id = AllWeapons.FindIndex(x => x.Name == gunName);
        }
        return id;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string[] AllWeaponStringList()
    {
        return AllWeapons.Select(x => x.Name).ToList().ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    public int CheckPlayerName(string pName)
    {
        for (int i = 0; i < GameTeam.Count; i++)
        {
            if (pName == GameTeam[i].UserName)
            {
                return 1;
            }
        }
        if (pName.Contains('[') || pName.Contains('{'))
        {
            return 2;
        }
        CurrentTeamUser = null;
        return 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CheckPasswordUse(string PName, string Pass)
    {
        for (int i = 0; i < GameTeam.Count; i++)
        {
            if (PName == GameTeam[i].UserName)
            {
                if (Pass == GameTeam[i].Password)
                {
                    CurrentTeamUser = GameTeam[i];
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    private string _role = string.Empty;
    public string RolePrefix
    {
        get
        {
#if ULSP
            if (!bl_DataBase.IsUserLogged)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(_role))
            {
                _role = bl_DataBase.LocalUserInstance.GetStatusPrefix();
#if CLANS
                if (bl_DataBase.Instance.LocalUser.HaveAClan())
                {
                    if (_role != null && _role.Length > 1) _role += " ";
                    _role += string.Format("{0}", bl_DataBase.Instance.LocalUser.Clan.GetTagPrefix());
                }
#endif
            }
            return _role;
#else
            if (CurrentTeamUser != null && !string.IsNullOrEmpty(CurrentTeamUser.UserName))
            {
                return string.Format("<color=#{1}>[{0}]</color>", CurrentTeamUser.m_Role.ToString(), ColorUtility.ToHtmlStringRGBA(CurrentTeamUser.m_Color));
            }
            else
            {
                return string.Empty;
            }
#endif
        }
        private set => _role = value;
    }

    private bl_RuntimeSettingsProfile m_settingProfile;
    public bl_RuntimeSettingsProfile RuntimeSettings
    {
        get
        {
            if (m_settingProfile == null) m_settingProfile = Instantiate(defaultSettings);
            return m_settingProfile;
        }
    }

    private static TagsAndLayersSettings m_tagsAndLayerSettings;
    public static TagsAndLayersSettings TagsAndLayerSettings
    {
        get
        {
            if (m_tagsAndLayerSettings == null) m_tagsAndLayerSettings = Instance.tagsAndLayersSettings;
            return m_tagsAndLayerSettings;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bl_GameData Instance
    {
        get
        {
            if (m_instance == null && !isCaching)
            {
                if (!isDataCached && Application.isPlaying)
                {
                    Debug.Log("GameData was cached synchronous, that could cause bottleneck on load, try caching it asynchronous with AsyncLoadData()");
                    isDataCached = true;
                    OnDataCached();
                }
                m_instance = Resources.Load("GameData", typeof(bl_GameData)) as bl_GameData;
            }
            return m_instance;
        }
    }
    #endregion

    /// <summary>
    /// cache the GameData from Resources asynchronous to avoid overhead and freeze the main thread the first time we access to the instance
    /// </summary>
    /// <returns></returns>
    public static IEnumerator AsyncLoadData()
    {
        if (m_instance == null)
        {
            isCaching = true;
            ResourceRequest rr = Resources.LoadAsync("GameData", typeof(bl_GameData));
            while (!rr.isDone) { yield return null; }
            m_instance = rr.asset as bl_GameData;
            isCaching = false;
            OnDataCached();
        }
        isDataCached = true;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        for (int i = 0; i < AllScenes.Count; i++)
        {
            if (AllScenes[i].m_Scene == null) continue;
            AllScenes[i].RealSceneName = AllScenes[i].m_Scene.name;
        }
    }
#endif

    #region Local Classes
    [Serializable]
    public class ScoreRewards
    {
        public int ScorePerKill = 50;
        public int ScorePerHeadShot = 25;
        public int ScoreForWinMatch = 100;
        [Tooltip("Per minute played")]
        public int ScorePerTimePlayed = 3;

        public int GetScorePerTimePlayed(int time)
        {
            if (ScorePerTimePlayed <= 0) return 0;
            return time * ScorePerTimePlayed;
        }
    }

    [Serializable]
    public class VirtualCoin
    {
        [MFPSCoinID] public int XPCoin;
        [Tooltip("how much score/xp worth one coin")]
        public int CoinScoreValue = 1000;//how much score/xp worth one coin
        public int InitialCoins
        {
            get
            {
                var coin = bl_MFPS.Coins.GetCoinData(XPCoin);
                return coin == null ? 0 : coin.InitialCoins;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newCoins"></param>
        public void AddCoins(int newCoins, string endPoint = "") => bl_MFPS.Coins.GetCoinData(XPCoin)?.Add(newCoins, endPoint);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public int GetCoinsPerScore(int score)
        {
            if (score <= 0 || score < CoinScoreValue || CoinScoreValue <= 0) return 0;

            return score / CoinScoreValue;
        }
    }

    [Serializable]
    public class GameTeamInfo
    {
        public string UserName;
        public Role m_Role = Role.Moderator;
        public string Password;
        public Color m_Color;

        public enum Role
        {
            Admin = 0,
            Moderator = 1,
        }
    }
    #endregion
}