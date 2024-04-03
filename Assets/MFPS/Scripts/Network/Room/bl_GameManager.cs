using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Handle the player spawn functions, game mode selection, and many general game utility functions.
/// </summary>
public class bl_GameManager : bl_PhotonHelper, IInRoomCallbacks, IConnectionCallbacks
{
    #region Public members
    /// <summary>
    /// Current general game state
    /// </summary>
    public MatchState GameMatchState;

    /// <summary>
    /// Event called once all the players required to start have joined
    /// This is only called when GameData.JoinMethod is set to DirectToMap
    /// </summary>
    public Action onAllPlayersRequiredIn;

    /// <summary>
    /// The list  of all real connected players
    /// </summary>
    public List<Player> connectedPlayerList = new List<Player>();

    /// <summary>
    /// List with references to all instanced players (bots and real players) in the scene.
    /// This list included all but the local player.
    /// </summary>
    public List<MFPSPlayer> OthersActorsInScene = new List<MFPSPlayer>(); 
    #endregion
    
    #region Public properties
    /// <summary>
    /// Local player MFPSPlayer data
    /// </summary>
    public MFPSPlayer LocalActor 
    { 
        get; 
        set; 
    } = new MFPSPlayer();

    /// <summary>
    /// True when the player is waiting for spawn
    /// </summary>
    public bool IsLocalWaitingForSpawn 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// Headshot count of the local player in this match
    /// </summary>
    public int Headshots 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// The current match game mode main script
    /// </summary>
    public IGameMode GameModeLogic 
    { 
        get; 
        private set; 
    }

    /// <summary>
    /// Have the game finished?
    /// </summary>
    public bool GameFinish 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// Is the Local player currently playing?
    /// </summary>
    public bool IsLocalPlaying 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// Local player game object reference
    /// </summary>
    public new GameObject LocalPlayer 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// Local player script references
    /// </summary>
    public bl_PlayerReferences LocalPlayerReferences 
    { 
        get; 
        set;
    }

    /// <summary>
    /// Player prefab to override the ones from GameData
    /// </summary>
    public bl_OverridePlayerPrefab OverridePlayerPrefab 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// The name of the player prefab with which the local player prefab spawned
    /// </summary>
    public string LastInstancedPlayerPrefabName 
    { 
        get; 
        private set; 
    }

    public static int LocalPlayerViewID = -1;
    public static int SuicideCount = 0;
    public static bool Joined = false;
    #endregion

    #region Private members
    private int WaitingPlayersAmount = 1;
    private float StartPlayTime;
    private bool registered = false;
    /// <summary>
    /// In case you need to filter the player spawn
    /// This function will be invoked right before the instanced the player prefab
    /// if the function return true, the player will not be spawned, otherwise it won't spawn and you will have to handled
    /// the instantiation in your custom function.
    /// </summary>
    private Queue<Func<GameObject, Vector3, Quaternion, Team, bool>> spawnFilters;
    #endregion

    #region Unity Method
    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        CheckViewAllocation();
        if (!registered) { PhotonNetwork.AddCallbackTarget(this); registered = true; }

        PhotonNetwork.IsMessageQueueRunning = true;
        bl_UtilityHelper.BlockCursorForUser = false;
        bl_NamePlateBase.BlockDraw = false;
        Joined = false;
        SuicideCount = 0;
        StartPlayTime = Time.time;

        LocalActor.isRealPlayer = true;
        LocalActor.Name = PhotonNetwork.NickName;

        bl_CrosshairBase.Instance.Show(false);
        if (bl_MFPS.GameData.UsingWaitingRoom() && bl_PhotonNetwork.LocalPlayer.GetPlayerTeam() != Team.None)
        {
            Invoke(nameof(SpawnPlayerWithCurrentTeam), 2);
        }
        Debug.Log($"Game Started, Online: {!bl_PhotonNetwork.OfflineMode} Master: {bl_PhotonNetwork.IsMasterClient} State: {GameMatchState.ToString()} TimeState: {bl_MatchTimeManagerBase.Instance.TimeState.ToString()}");
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if(GameModeLogic == null)// check on start because game mode should be assigned on awake
        {
            Debug.LogWarning("No Game Mode has been assigned yet!");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onRemoteActorChange += OnRemoteActorChange;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onRemoteActorChange -= OnRemoteActorChange;
        if(registered)
        PhotonNetwork.RemoveCallbackTarget(this);
        if(GameModeLogic != null)
        {
            bl_PhotonCallbacks.PlayerEnteredRoom -= GameModeLogic.OnOtherPlayerEnter;
            bl_PhotonCallbacks.RoomPropertiesUpdate -= GameModeLogic.OnRoomPropertiesUpdate;
            bl_PhotonCallbacks.PlayerLeftRoom -= GameModeLogic.OnOtherPlayerLeave;
            bl_EventHandler.onLocalPlayerDeath -= GameModeLogic.OnLocalPlayerDeath;
        }
    }
    #endregion

    /// <summary>
    /// Spawn the local player in the give team
    /// If a player instanced already exist, it will be automatically destroyed and replaced with a new one.
    /// </summary>
    /// <param name="playerTeam">team in which the player will spawn</param>
    /// <returns></returns>
    public bool SpawnPlayer(Team playerTeam)
    {
        if (IsLocalWaitingForSpawn) return false;//there's a reserved spawn incoming, don't spawn before that

        //if there is a local player already instance
        if (LocalPlayer != null)
        {
            PhotonNetwork.Destroy(LocalPlayer);
        }

        //if the game finish
        if (GameFinish)
        {
            bl_RoomCameraBase.Instance?.SetActive(false);
            return false;
        }

        //set the player team to the player properties
        var PlayerTeam = new Hashtable
        {
            { PropertiesKeys.TeamKey, playerTeam.ToString() }
        };
        bl_PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerTeam);
        bl_MFPS.LocalPlayer.Team = playerTeam;

        //spawn the player model
#if !PSELECTOR
        SpawnPlayerModel(playerTeam);
#else
        if (bl_PlayerSelector.InMatch)
        {
            if(bl_PlayerSelector.Instance == null)
            {
                Debug.LogWarning("Player Selector is enabled but is not integrated in this map.");
                return false;
            }
            if (!bl_PlayerSelector.Instance.TrySpawnSelectedPlayer(playerTeam)) return false;
        }
        else
        {
            bl_PlayerSelector.SpawnPreSelectedPlayer(playerTeam);
        }
#endif
        return true;
    }

    /// <summary>
    /// Spawn player based in the team
    /// </summary>
    /// <returns></returns>
    public void SpawnPlayerModel(Team playerTeam)
    {
        Vector3 pos;
        Quaternion rot;
        bl_SpawnPointManager.Instance.GetPlayerSpawnPosition(playerTeam, out pos, out rot);
        
        GameObject playerPrefab = bl_GameData.Instance.Player1.gameObject;
        if (OverridePlayerPrefab == null)
        {
            if (playerTeam == Team.Team2) playerPrefab = bl_GameData.Instance.Player2.gameObject;
        }
        else
        {
            playerPrefab = OverridePlayerPrefab.GetPlayerForTeam(playerTeam);
        }

        if (!InstancePlayer(playerPrefab, pos, rot, playerTeam))
        {
            // if the player was not instanced
            return;
        }

        OnPostSpawn();
    }

    /// <summary>
    /// Spawn the local player in the given position and rotation.
    /// </summary>
    /// <param name="prefab">Player prefab</param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="team"></param>
    /// <returns>Succeed instancing the player.</returns>
    public bool InstancePlayer(GameObject prefab, Vector3 position, Quaternion rotation, Team team)
    {
        if (!bl_PhotonNetwork.InRoom) return false;

        // Check if there're spawn filters
        if (VerifySpawnFilter(prefab, position, rotation, team, out bool filterResult))
        {
            // if there's a filter and it returns True
            if (filterResult)
            {
                // That means we can spawn the player, so lets start this function from the begin (to check any other pending filter)
                InstancePlayer(prefab, position, rotation, team);
            }
            // if the filter returns False, this mean the spawn in canceled here and therefore
            // the spawn will be handle elsewhere (by the filter)
            else return false;
        }

        //set the some common data that will be sync right after the player is instanced in the other clients
        var commonData = new object[1];
        commonData[0] = team;

        LastInstancedPlayerPrefabName = prefab.name;

        //instantiate the player prefab
        LocalPlayer = PhotonNetwork.Instantiate(prefab.name, position, rotation, 0, commonData);

        LocalPlayerReferences = LocalPlayer.GetComponent<bl_PlayerReferences>();
        LocalActor.Actor = LocalPlayer.transform;
        LocalActor.ActorView = LocalPlayer.GetComponent<PhotonView>();
        LocalActor.Team = team;
        LocalActor.AimPosition = LocalPlayer.GetComponent<bl_PlayerSettings>().AimPositionReference.transform;

        if (!Joined) Debug.Log($"Spawned with the player prefab: {prefab.name}");

        bl_EventHandler.DispatchPlayerLocalSpawnEvent();
        return true;
    }

    /// <summary>
    /// Check if there's any spawn filter pending
    /// </summary>
    /// <returns>True if a filter exist, false if not filters availables</returns>
    public static bool VerifySpawnFilter(GameObject prefab, Vector3 position, Quaternion rotation, Team team, out bool filterResult)
    {
        filterResult = false;
        if (Instance.spawnFilters == null || Instance.spawnFilters.Count == 0) return false;

        var filter = Instance.spawnFilters.Dequeue();
        filterResult = filter.Invoke(prefab, position, rotation, team);
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filter"></param>
    public static void AddSpawnFilter(Func<GameObject, Vector3, Quaternion, Team, bool> filter)
    {
        if (Instance == null) return;
        if (Instance.spawnFilters == null) Instance.spawnFilters = new Queue<Func<GameObject, Vector3, Quaternion, Team, bool>>();
        Instance.spawnFilters.Enqueue(filter);
    }

    /// <summary>
    /// Assign a player to a team but not instance it
    /// </summary>
    public void SetLocalPlayerToTeam(Team team)
    {
        Hashtable PlayerTeam = new Hashtable
        {
            { PropertiesKeys.TeamKey, team.ToString() }
        };
        bl_PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerTeam, null);
        bl_MFPS.LocalPlayer.Team = team;
        Joined = true;
    }

    /// <summary>
    /// Instance a Player only if already has been instanced and is alive
    /// </summary>
    public void SpawnPlayerIfAlreadyInstanced()
    {
        if (LocalPlayer == null)
            return;

        Team t = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam();
        SpawnPlayer(t);
    }

    /// <summary>
    /// Respawn the local player after the respawn time delay
    /// </summary>
    /// <param name="respawnTime">define a custom delay or -1 to use the GameData respawn fixed delay</param>
    public void RespawnLocalPlayerAfter(float respawnTime = -1, bool doFadeIn = true)
    {
        if (respawnTime < 0) respawnTime = bl_GameData.Instance.PlayerRespawnTime;

        StartCoroutine(DoWait());
        IEnumerator DoWait()
        {
            float t = respawnTime / 3;
            yield return new WaitForSeconds(t * 2);
            if (!GameFinish && doFadeIn)
            {
                StartCoroutine(bl_UIReferences.Instance.FinalFade(true, false));
            }
            bl_NamePlateBase.BlockDraw = true;
            yield return new WaitForSeconds(t);
            if (SpawnPlayer(bl_PhotonNetwork.LocalPlayer.GetPlayerTeam()))
            {
                bl_KillCamBase.Instance.SetActive(false);
            }
            bl_NamePlateBase.BlockDraw = false;
        }
    }

    /// <summary>
    /// Called after each player spawn.
    /// </summary>
    public static void OnPostSpawn(bool skipFade = false)
    {
        if (Instance == null) return;

        Instance.AfterSpawnSetup(skipFade);

        if (!Instance.FirstSpawnDone && bl_MatchInformationDisplay.Instance != null) { bl_MatchInformationDisplay.Instance.DisplayInfo(); }
        Instance.FirstSpawnDone = true;
        bl_CrosshairBase.Instance.Show(true);
    }
    
    /// <summary>
    /// If Player exist, them destroy
    /// </summary>
    public void DestroyPlayer(bool ActiveCamera)
    {
        if (LocalPlayer != null)
        {
            PhotonNetwork.Destroy(LocalPlayer);
        }
        bl_RoomCameraBase.Instance?.SetActive(ActiveCamera);
    }

    /// <summary>
    /// Called when the room round time finish
    /// </summary>
    public void OnGameTimeFinish(bool gameOver)
    {
        GameFinish = true;
        if(GameModeLogic != null) { GameModeLogic.OnFinishTime(gameOver); }
        SetGameState(MatchState.Finishing);
    }

    /// <summary>
    /// Make the local player spawn in a random spawn point of the current team
    /// </summary>
    public void SpawnPlayerWithCurrentTeam()
    {
        if(SpawnPlayer(bl_PhotonNetwork.LocalPlayer.GetPlayerTeam()))
        bl_RoomMenu.Instance.OnAutoTeam();
    }

    /// <summary>
    /// Called after the local player spawn
    /// </summary>
    public void AfterSpawnSetup(bool skipFade = false)
    {
        bl_RoomCameraBase.Instance?.SetActive(false);
        if (!skipFade) StartCoroutine(bl_UIReferences.Instance.FinalFade(false, false, 0));
        bl_UtilityHelper.LockCursor(true);
        if (!Joined) { StartPlayTime = Time.time; }
        Joined = true;
    }

    #region GameModes
    public bool IsGameMode(GameMode mode, IGameMode logic)
    {
        bool isIt = GetGameMode == mode;
        if (isIt)
        {
            if (GameModeLogic != null)
            {
                Debug.LogError("A GameMode has been assigned before, only 1 game mode can be assigned per match.");
                return false;
            }
            GameModeLogic = logic;
            bl_PhotonCallbacks.PlayerEnteredRoom += logic.OnOtherPlayerEnter;
            bl_PhotonCallbacks.RoomPropertiesUpdate += logic.OnRoomPropertiesUpdate;
            bl_PhotonCallbacks.PlayerLeftRoom += logic.OnOtherPlayerLeave;
            bl_EventHandler.onLocalPlayerDeath += GameModeLogic.OnLocalPlayerDeath;
            Debug.Log("Game Mode: " + mode.GetName());
        }
        return isIt;
    }

    /// <summary>
    /// Should called when the local player score in game
    /// </summary>
    public void SetPointFromLocalPlayer(int points, GameMode mode)
    {
        if (GameModeLogic == null) return;
        if (mode != GetGameMode) return;

        GameModeLogic.OnLocalPoint(points, bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());
    }

    /// <summary>
    /// Should called when a non player object score in game, ex: AI
    /// This should be called by master client only
    /// </summary>
    public void SetPoint(int points, GameMode mode, Team team)
    {
        if (GameModeLogic == null) return;
        if (mode != GetGameMode) return;

        GameModeLogic.OnLocalPoint(points, team);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnLocalPlayerKill()
    {
        if (GameModeLogic == null) return;

        GameModeLogic.OnLocalPlayerKill();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool isLocalPlayerWinner()
    {
        if (GameModeLogic == null) return false;
        return GameModeLogic.isLocalPlayerWinner;
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public bool WaitForPlayers(int MinPlayers)
    {
        if (MinPlayers > 1)
        {
            if (isOneTeamMode)
            {
                if (PhotonNetwork.PlayerList.Length >= MinPlayers) return false;
            }
            else
            {
                if (PhotonNetwork.PlayerList.Length >= MinPlayers)
                {
                    if (PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length > 0 && PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length > 0)
                    {
                        if(onAllPlayersRequiredIn != null) { onAllPlayersRequiredIn.Invoke(); }
                        return false;
                    }
                }
            }
        }
        WaitingPlayersAmount = MinPlayers;
        SetGameState(MatchState.Waiting);
        return true;
    }

    /// <summary>
    /// This is a event callback
    /// here is caching all 'actors' in the scene (players and bots)
    /// </summary>
    public void OnRemoteActorChange(bl_EventHandler.PlayerChangeData data)
    {
        int id = OthersActorsInScene.FindIndex(x => x.Name == data.PlayerName);
        if (id != -1)
        {
            if (data.IsAlive)
            {
                if (data.MFPSActor != null)
                {
                    OthersActorsInScene[id] = data.MFPSActor;
                }
                else
                {
                    OthersActorsInScene[id].isAlive = data.IsAlive;
                }
                OthersActorsInScene[id].ActorView = data.NetworkView;
            }
            else
            {
                if(OthersActorsInScene[id].Actor == null)
                {
                    OthersActorsInScene[id].isAlive = false;
                }
            }
        }
        else
        {
            if (data.IsAlive)
            {
               if(data.MFPSActor == null) { Debug.LogWarning($"Actor data for {data.PlayerName} has not been build yet."); return; }
               if(data.MFPSActor.ActorView == null) { data.MFPSActor.ActorView = data.MFPSActor.Actor?.GetComponent<PhotonView>(); }
                OthersActorsInScene.Add(data.MFPSActor);
            }
        }
    }

    /// <summary>
    /// Add a new player info in the <see cref="OthersActorsInScene"/> list
    /// </summary>
    /// <param name="newPlayer">The new player to register</param>
    /// <param name="force">Replace the information if record already exist for this player</param>
    public void RegisterMFPSPlayer(MFPSPlayer newPlayer, bool force = false)
    {
        if (OthersActorsInScene.Exists(x => x.Name == newPlayer.Name) && !force) return;

        int index = OthersActorsInScene.FindIndex(x => x.Name == newPlayer.Name);

        if(index == -1)
        {
            OthersActorsInScene.Add(newPlayer);
        }
        else
        {
            OthersActorsInScene[index] = newPlayer;
        }
    }

    /// <summary>
    /// Find a player or bot by their PhotonView ID
    /// </summary>
    /// <returns></returns>
    public Transform FindActor(int ViewID)
    {
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if(OthersActorsInScene[i].ActorView != null && OthersActorsInScene[i].ActorView.ViewID == ViewID) 
            {
                return OthersActorsInScene[i].Actor;
            }
        }
        if(LocalPlayer != null && LocalPlayer.GetPhotonView().ViewID == ViewID) { return LocalPlayer.transform; }
        return null;
    }

    /// <summary>
    /// Find a player or bot by their PhotonPlayer
    /// </summary>
    /// <returns></returns>
    public Transform FindActor(Player player)
    {
        if (player == null) return null;
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].ActorView != null && OthersActorsInScene[i].ActorView.Owner != null && OthersActorsInScene[i].ActorView.Owner.ActorNumber == player.ActorNumber)
            {
                return OthersActorsInScene[i].Actor;
            }
        }
        if (LocalPlayer != null && LocalPlayer.GetPhotonView().Owner.ActorNumber == player.ActorNumber) { return LocalPlayer.transform; }
        return null;
    }

    /// <summary>
    /// Find a player or bot by their PhotonPlayer
    /// </summary>
    /// <returns></returns>
    public MFPSPlayer FindActor(string actorName)
    {
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].ActorView != null && OthersActorsInScene[i].Actor.name == actorName)
            {
                return OthersActorsInScene[i];
            }
        }
        if (LocalPlayer != null && LocalPlayer.GetPhotonView().Owner.NickName == actorName) { return LocalActor; }
        return null;
    }

    /// <summary>
    /// Find a player or bot by their ViewID
    /// </summary>
    /// <returns></returns>
    public MFPSPlayer GetMFPSActor(int viewID)
    {
        if (LocalActor.ActorViewID == viewID) { return LocalActor; }
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].ActorViewID == viewID)
            {
                return OthersActorsInScene[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    public MFPSPlayer GetMFPSPlayer(string nickName)
    {
        MFPSPlayer player = OthersActorsInScene.Find(x => x.Name == nickName);
        if(player == null && nickName == LocalName)
        {
            player = LocalActor;
        }
        return player;
    }

    /// <summary>
    /// Get the list of MFPS players that are joined in the given team
    /// </summary>
    /// <param name="team">Team from where the players have to be part of.</param>
    /// <param name="registeredActorsOnly">Fetch only players that has been registered in the <see cref="OthersActorsInScene"/> list.</param>
    /// <returns></returns>
    public MFPSPlayer[] GetMFPSPlayerInTeam(Team team, bool registeredActorsOnly = true)
    {
        List<MFPSPlayer> list = new List<MFPSPlayer>();
        if (registeredActorsOnly)
        {
            for (int i = 0; i < OthersActorsInScene.Count; i++)
            {
                if (OthersActorsInScene[i].Team == team)
                {
                    list.Add(OthersActorsInScene[i]);
                }
            }
            if (LocalActor.Team == team) list.Add(LocalActor);
        }
        else
        {

        }
        return list.ToArray();
    }

    /// <summary>
    /// Get the list of MFPS actors that are not in the same team of the local player
    /// </summary>
    /// <param name="includeBots"></param>
    /// <returns></returns>
    public List<MFPSPlayer> GetNonTeamMatePlayers(bool includeBots = true)
    {
        Team playerTeam = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam();
        List<MFPSPlayer> list = new List<MFPSPlayer>();
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].Team != playerTeam)
            {
                if (OthersActorsInScene[i].isRealPlayer) { list.Add(OthersActorsInScene[i]); }
                else if (includeBots) { list.Add(OthersActorsInScene[i]); }
            }
        }
        return list;
    }

    #region PUN
    [PunRPC]
    void RPCSyncGame(MatchState state)
    {
        Debug.Log("Game sync by master, match state: " + state.ToString());
        GameMatchState = state;
        if (!bl_PhotonNetwork.IsMasterClient)
        {
            bl_MatchTimeManagerBase.Instance.Init();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    public void SetGameState(MatchState state)
    {
        if (!bl_PhotonNetwork.IsMasterClient)
        {
            return;
        }

        CheckViewAllocation();
        PhotonNetwork.RegisterPhotonView(photonView);

        photonView.RPC(nameof(RPCMatchState), RpcTarget.All, state);
    }

    [PunRPC]
    void RPCMatchState(MatchState state)
    {
        GameMatchState = state;
        bl_EventHandler.DispatchMatchStateChange(state);
        CheckPlayersInMatch();
    }

    /// <summary>
    /// Check the minimum required players are meet
    /// </summary>
    void CheckPlayersInMatch()
    {
        //if still waiting
        if (!bl_MatchTimeManagerBase.Instance.IsInitialized && GameMatchState == MatchState.Waiting)
        {
            bool ready = false;
            if (isOneTeamMode)
            {
                ready = PhotonNetwork.PlayerList.Length >= WaitingPlayersAmount;
            }
            else
            {
                int team1Count = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length;
                int team2Count = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length;
                int totalPlayers = PhotonNetwork.PlayerList.Length;

                if (bl_AIMananger.Instance.BotsActive)
                {
                    totalPlayers += bl_AIMananger.Instance.BotsStatistics.Count;
                    team1Count += bl_AIMananger.Instance.GetAllBotsInTeam(Team.Team1).Count;
                    team2Count += bl_AIMananger.Instance.GetAllBotsInTeam(Team.Team2).Count;
                }

                //if the minimum amount of players are in the game
                if (totalPlayers >= WaitingPlayersAmount)
                {
                    //and they are split in both teams
                    if ((team1Count > 0 && team2Count > 0) || WaitingPlayersAmount <= 1)
                    {
                        //we are ready to start
                        ready = true;
                    }
                    else
                    {
                        //otherwise wait until player split in both teams
#if LOCALIZATION
                        bl_UIReferences.Instance.SetWaitingPlayersText(bl_Localization.Instance.GetText(128), true);
#else
                        bl_UIReferences.Instance.SetWaitingPlayersText(bl_GameTexts.WaitingTeamBalance, true);
#endif
                        return;
                    }
                }
            }
            if (ready)//all needed players in game
            {
                //master set the call to start the match
                if (bl_PhotonNetwork.IsMasterClient)
                {
                    bl_MatchTimeManagerBase.Instance.InitAfterWaiting();
                }
                SetGameState(MatchState.Starting);
                bl_MatchTimeManagerBase.Instance.SetTimeState(RoomTimeState.Started, true);
                onAllPlayersRequiredIn?.Invoke();
                bl_UIReferences.Instance.SetWaitingPlayersText("", false);
            }
            else
            {
                bl_UIReferences.Instance.SetWaitingPlayersText(string.Format(bl_GameTexts.WaitingPlayers, PhotonNetwork.PlayerList.Length, 2), true);
            }
        }
        else
        {
            bl_UIReferences.Instance.SetWaitingPlayersText("", false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnApplicationQuit()
    {
        PhotonNetwork.Disconnect();
    }

    //PLAYER EVENTS
    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player connected: " + newPlayer.NickName);
        if (bl_PhotonNetwork.IsMasterClient)
        {
            //master sync the require match info to be sure all players have the same info at the start
            photonView.RPC(nameof(RPCSyncGame), newPlayer, GameMatchState);
        }

        // if the new player has a team, it means it came from the waiting room
        if (newPlayer.GetPlayerTeam() != Team.None)
        {
            // Try register the player info right away
            // This is important to do since in game modes where the player have to wait until a round finish before he can spawn
            // the player record won't be added until the second time he spawn
            var playerData = new MFPSPlayer()
            {
                Name = newPlayer.NickName,
                Team = newPlayer.GetPlayerTeam(),
                isRealPlayer = true,
                isAlive = false,
            };
            RegisterMFPSPlayer(playerData);
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player disconnected: " + otherPlayer.NickName);
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
      
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //when a player has join to a team
        if (changedProps.ContainsKey(PropertiesKeys.TeamKey))
        {
            //make sure has join to a team
            if ((string)changedProps[PropertiesKeys.TeamKey] != Team.None.ToString())
            {
                CheckPlayersInMatch();
            }
            else
            {
                if (GameMatchState == MatchState.Waiting)
                {
                    bl_UIReferences.Instance.SetWaitingPlayersText(string.Format(bl_GameTexts.WaitingPlayers, PhotonNetwork.PlayerList.Length, WaitingPlayersAmount), true);
                }
            }
        }
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("The old masterclient left, we have a new masterclient: " + newMasterClient.NickName);
        bl_RoomChatBase.Instance?.SetChatLocally($"We have a new MasterClient: {newMasterClient.NickName}");
    }

    public void OnConnected()
    {     
    }

    public void OnConnectedToMaster()
    {    
    }

    public void OnDisconnected(DisconnectCause cause)
    {
#if UNITY_EDITOR
        if (bl_RoomMenu.Instance.isApplicationQuitting) { return; }
#endif
        Debug.Log("Clean up a bit after server quit, cause: " + cause.ToString());
        PhotonNetwork.IsMessageQueueRunning = false;
        bl_UtilityHelper.LoadLevel(bl_GameData.Instance.OnDisconnectScene);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {    
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {       
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {     
    }
    #endregion

    #region Getters
    private Camera cameraRender = null;
    public Camera CameraRendered
    {
        get
        {
            if (cameraRender == null)
            {
                // Debug.Log("Not Camera has been setup.");
                return Camera.current;
            }
            return cameraRender;
        }
        set
        {
            if (cameraRender != null && cameraRender.isActiveAndEnabled)
            {
                //if the current render over the set camera, keep it as renderer camera
                if (cameraRender.depth >= value.depth) return;
            }
            cameraRender = value;
        }
    }

    private bool m_enterInGame = false;
    public bool FirstSpawnDone
    {
        get
        {
            return m_enterInGame;
        }
        set
        {
            m_enterInGame = value;
        }
    }

    public float PlayedTime => (Time.time - StartPlayTime);

    private static bl_GameManager _instance;
    public static bl_GameManager Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_GameManager>(); }
            return _instance;
        }
    }
    #endregion
}