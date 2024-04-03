using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using MFPS.Audio;
using MFPS.Internal.Structures;
using MFPS.Runtime.UI;
using Random = UnityEngine.Random;
using System.Linq;

[DefaultExecutionOrder(-998)]
public class bl_Lobby : bl_PhotonHelper, IConnectionCallbacks, ILobbyCallbacks, IMatchmakingCallbacks
{
    #region Public members
    [Header("Photon")]
    public SeverRegionCode DefaultServer = SeverRegionCode.usw;
    [LovattoToogle] public bool ShowPhotonStatistics;

    [Header("Room Options")]
    //Room Max Ping
    public int[] MaxPing = new int[] { 100, 200, 500, 1000 };
    #endregion

    #region Public properties
    public LobbyConnectionState connectionState { get; private set; } = LobbyConnectionState.Disconnected;
    public int CurrentMaxPing { get; set; }
    public int ServerRegionID { get; set; } = -1;
    public GameModeSettings[] GameModes { get; set; }
    public bool rememberMe { get; set; }
    public string justCreatedRoomName { get; set; }
    public string CachePlayerName { get; set; }
    public Action onShowMenu;
    #endregion

    #region Private members
    private RoomInfo checkingRoom;
    private bool quittingApp = false;
    private bool isSeekingMatch = false;
    bool alreadyLoadHome = false;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
#if ULSP
        if (bl_ULoginUtility.RedirectToLoginIfNeeded()) return;
#endif
        SetupPhotonSettings();
        bl_UtilityHelper.BlockCursorForUser = false;
        bl_UtilityHelper.LockCursor(false);

        // Loading process
        StartCoroutine(InitialLoadProcess());

        if (bl_AudioController.Instance != null) { bl_AudioController.Instance.PlayBackground(); }
        bl_LobbyUI.Instance.InitialSetup();
    }

    /// <summary>
    /// Setup Photon
    /// </summary>
    void SetupPhotonSettings()
    {
        PhotonNetwork.AddCallbackTarget(this);
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
        bl_PhotonNetwork.OfflineMode = false;
        bl_PhotonNetwork.IsMessageQueueRunning = true;
        PhotonNetwork.EnableCloseConnection = true;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = DefaultServer.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator InitialLoadProcess()
    {
        bl_LobbyLoadingScreenBase.Instance
            .SetActive(true)
            .SetText(bl_GameTexts.LoadingLocalContent.Localized(39));

        if (!bl_GameData.isDataCached)
        {
            // Load GameData asynchronously
            yield return StartCoroutine(bl_GameData.AsyncLoadData());
            yield return new WaitForEndOfFrame();
        }

        SetUpGameModes();
        bl_LobbyUI.Instance.Setup();

        yield return bl_LobbyUI.Instance.blackScreenFader.FadeOut(1);

        bl_LobbyLoadingScreenBase.Instance
            .SetActive(true);

        GetPlayerName();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ConnectPhoton()
    {
        if (bl_PhotonNetwork.IsConnected) return;

        connectionState = LobbyConnectionState.Connecting;
        PhotonNetwork.GameVersion = bl_GameData.Instance.GameVersion;
        AuthenticationValues authValues = new AuthenticationValues(bl_PhotonNetwork.NickName);

#if ULSP  
        if (bl_DataBase.IsUserLogged && bl_LoginProDataBase.Instance.usePhotonAuthentication)
        {
            authValues.AuthType = CustomAuthenticationType.Custom;
            authValues.AddAuthParameter("type", "0");
            authValues.AddAuthParameter("photonId", bl_PhotonNetwork.NickName);
            authValues.AddAuthParameter("userId", bl_DataBase.LocalLoggedUser.ID.ToString());
            authValues.AddAuthParameter("ip", bl_DataBase.LocalLoggedUser.IP);
            if (bl_LoginProDataBase.UseDeviceId())
            {
                authValues.AddAuthParameter("deviceId", SystemInfo.deviceUniqueIdentifier);
            }
        }
#endif

        PhotonNetwork.AuthValues = authValues;

        //if we don't have a custom region to connect or we are using a self hosted server
        if (ServerRegionID == -1 || !PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
        {
            //connect using the default PhotonServerSettings
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            //Change the cloud server region
            ConnectToServerRegion(ServerRegionID, true);
        }

        bl_LobbyLoadingScreenBase.Instance
            .SetActive(true)
            .SetText(bl_GameTexts.ConnectingToGameServer.Localized(40));
    }

    /// <summary>
    /// Change the server region (Photon PUN Only)
    /// </summary>
    public void ConnectToServerRegion(int id, bool initialConnection = false)
    {
        if (PhotonNetwork.IsConnected && !initialConnection)
        {
            // When the player manually change of region
            connectionState = LobbyConnectionState.ChangingRegion;
            bl_LobbyLoadingScreenBase.Instance.SetActive(true)
           .SetText(bl_GameTexts.ConnectingToGameServer.Localized(40));

            ServerRegionID = id;
            Invoke(nameof(DelayDisconnect), 0.2f);
            return;
        }

        if (!string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
        {
            connectionState = LobbyConnectionState.Connecting;
            var code = SeverRegionCode.usw;
            if (id > 3) { id++; } // servers ids jumps from 3 to 5
            code = (SeverRegionCode)id;
            PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
            PhotonNetwork.ConnectToRegion(code.ToString());
            PlayerPrefs.SetString(PropertiesKeys.GetUniqueKey("preferredregion"), code.ToString());
        }
        else
        {
            Debug.LogWarning("Need your AppId for change server, please add it in PhotonServerSettings");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Disconect()
    {
        SetLobbyChat(false);
        if (bl_PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DelayDisconnect() => PhotonNetwork.Disconnect();

    /// <summary>
    /// This is called from the server every time the room list is updated.
    /// </summary>
    public void ServerList(List<RoomInfo> roomList)
    {
        bl_LobbyRoomListBase.Instance.SetRoomList(roomList);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetNickName(string nick)
    {
        nick = nick.Replace("\n", "").Trim();
        if (bl_GameData.Instance.verifySingleSession)
        {
            if (string.IsNullOrEmpty(bl_PhotonNetwork.NickName))
            {
                // Session verification is required, which will be done after connect to the server.
                CachePlayerName = nick;
            }
            else bl_PhotonNetwork.NickName = nick; // in case a nick name change happens, make sure to update it.
        }
        else bl_PhotonNetwork.NickName = nick;

        if (rememberMe)
        {
            PlayerPrefs.SetString(PropertiesKeys.GetUniqueKey("remembernick"), nick);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GetPlayerName()
    {
        if (string.IsNullOrEmpty(bl_PhotonNetwork.NickName))
        {
#if ULSP
            // if the user has log in with an account
            if (bl_DataBase.IsUserLogged)
            {
                SetNickName(bl_DataBase.Instance.LocalUser.NickName);
                bl_EventHandler.DispatchCoinUpdate(null);
                GoToMainMenu();
            }
            else
            {
                GeneratePlayerName();
            }
#else
            GeneratePlayerName();
#endif
        }
        else
        {
            bl_EventHandler.DispatchCoinUpdate(null);
            GoToMainMenu();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GeneratePlayerName()
    {
        connectionState = LobbyConnectionState.WaitingForUserName;
        if (!rememberMe)
        {
            string storedNick = PropertiesKeys.GetUniqueKey("playername");
            if (!PlayerPrefs.HasKey(storedNick) || !bl_GameData.Instance.RememberPlayerName)
            {
                CachePlayerName = string.Format(bl_GameData.Instance.guestNameFormat, Random.Range(1, 9999));
            }
            else if (bl_GameData.Instance.RememberPlayerName)
            {
                CachePlayerName = PlayerPrefs.GetString(storedNick, string.Format(bl_GameData.Instance.guestNameFormat, Random.Range(1, 9999)));
            }
            bl_LobbyUI.Instance.PlayerNameField.text = CachePlayerName;
            SetNickName(CachePlayerName);
            bl_LobbyUI.Instance.ChangeWindow("player name");
            bl_LobbyLoadingScreenBase.Instance.HideIn(0.2f, true);
        }
        else
        {
            // Assign the saved nick name automatically
            CachePlayerName = PlayerPrefs.GetString(PropertiesKeys.GetUniqueKey("remembernick"));
            if (string.IsNullOrEmpty(CachePlayerName))
            {
                rememberMe = false;
                GeneratePlayerName();
                return;
            }
            SetNickName(CachePlayerName);
            GoToMainMenu();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GoToMainMenu()
    {
        if (!bl_PhotonNetwork.IsConnected)
        {
            ConnectPhoton();
        }
        else
        {
            if (!PhotonNetwork.InLobby)
            {
                if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
                {
                    PhotonNetwork.JoinLobby();
                }
            }
            else
            {

                if (string.IsNullOrEmpty(bl_PhotonNetwork.NickName))
                {
                    Debug.LogError("Player nick name has not been authenticated!");
                }

                connectionState = LobbyConnectionState.Connected;
                if (!alreadyLoadHome) { bl_LobbyUI.Instance.Home(); alreadyLoadHome = true; }
                SetLobbyChat(true);
                ResetValues();
                bl_LobbyLoadingScreenBase.Instance.HideIn(0.2f, true);
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public void SetPlayerName(string InputName)
    {
        CachePlayerName = InputName;
        PlayerPrefs.SetString(PropertiesKeys.GetUniqueKey("playername"), CachePlayerName);
        SetNickName(CachePlayerName);
        ConnectPhoton();
    }

    /// <summary>
    /// The password of the special accounts
    /// See 'Game Staff' in the documentation for more information
    /// </summary>
    public bool EnterPassword(string password)
    {
        if (bl_GameData.Instance.CheckPasswordUse(CachePlayerName, password))
        {
            SetNickName(CachePlayerName);
            ConnectPhoton();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoMatch()
    {
        if (isSeekingMatch)
            return;

        isSeekingMatch = true;
        StartCoroutine(DoSearch());
        IEnumerator DoSearch()
        {
            //active the search match UI
            bl_LobbyUI.Instance.SeekingMatchUI.SetActive(true);
            // This is not actually needed :)
            yield return new WaitForSeconds(3);
            PhotonNetwork.JoinRandomRoom();
            isSeekingMatch = false;
            bl_LobbyUI.Instance.SeekingMatchUI.SetActive(false);
        }
    }

    /// <summary>
    /// This function is called when hit Play button but there is not rooms to join in.
    /// </summary>
    public void OnNoRoomsToJoin(short returnCode, string message)
    {
        Debug.Log("No games to join found on matchmaking, creating one.");
        //create random room properties
        int scid = Random.Range(0, bl_GameData.Instance.AllScenes.Count);
        var mapInfo = bl_GameData.Instance.AllScenes[scid];

        var allModes = mapInfo.GetAllowedGameModes(GameModes);
        int modeRandom = Random.Range(0, allModes.Length);
        var gameMode = allModes[modeRandom];

        int maxPlayersRandom = Random.Range(0, gameMode.maxPlayers.Length);
        int timeRandom = Random.Range(0, gameMode.timeLimits.Length);
        int randomGoal = Random.Range(0, gameMode.GameGoalsOptions.Length);

        var roomInfo = new MFPSRoomInfo();
        roomInfo.roomName = string.Format("[PUBLIC] {0}{1}", bl_PhotonNetwork.NickName.Substring(0, 2), Random.Range(0, 9999));
        roomInfo.gameMode = gameMode.gameMode;
        roomInfo.time = gameMode.timeLimits[timeRandom];
        roomInfo.sceneName = mapInfo.RealSceneName;
        roomInfo.roundStyle = gameMode.GetAllowedRoundMode();
        roomInfo.autoTeamSelection = gameMode.AutoTeamSelection;
        roomInfo.mapName = mapInfo.ShowName;
        roomInfo.goal = gameMode.GetGoalValue(randomGoal);
        roomInfo.friendlyFire = false;
        roomInfo.maxPing = MaxPing[CurrentMaxPing];
        roomInfo.password = string.Empty;
        roomInfo.withBots = gameMode.supportBots;
        roomInfo.maxPlayers = gameMode.maxPlayers[maxPlayersRandom];

        CreateRoom(roomInfo);
    }

    /// <summary>
    /// Create a room with the settings specified in the Room Creator menu.
    /// </summary>
    public void CreateRoom()
    {
        var roomInfo = bl_LobbyRoomCreator.Instance.BuildRoomInfo();
        CreateRoom(roomInfo);
    }

    /// <summary>
    /// Create and Join in a room with the given info.
    /// </summary>
    /// <param name="roomInfo"></param>
    public void CreateRoom(MFPSRoomInfo roomInfo)
    {
        SetLobbyChat(false);
        justCreatedRoomName = roomInfo.roomName;

        //Save Room properties for load in room
        var roomOption = new ExitGames.Client.Photon.Hashtable();
        roomOption[PropertiesKeys.TimeRoomKey] = roomInfo.time;
        roomOption[PropertiesKeys.GameModeKey] = roomInfo.gameMode.ToString();
        roomOption[PropertiesKeys.SceneNameKey] = roomInfo.sceneName;
        roomOption[PropertiesKeys.RoomRoundKey] = roomInfo.roundStyle;
        roomOption[PropertiesKeys.TeamSelectionKey] = roomInfo.autoTeamSelection;
        roomOption[PropertiesKeys.CustomSceneName] = roomInfo.mapName;
        roomOption[PropertiesKeys.RoomGoal] = roomInfo.goal;
        roomOption[PropertiesKeys.RoomFriendlyFire] = roomInfo.friendlyFire;
        roomOption[PropertiesKeys.MaxPing] = roomInfo.maxPing;
        roomOption[PropertiesKeys.RoomPassword] = roomInfo.password;
        roomOption[PropertiesKeys.WithBotsKey] = roomInfo.withBots;

        PhotonNetwork.CreateRoom(roomInfo.roomName, new RoomOptions()
        {
            MaxPlayers = (byte)roomInfo.maxPlayers,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = roomOption,
            CleanupCacheOnLeave = true,
            CustomRoomPropertiesForLobby = roomOption.Keys.Select(x => x.ToString()).ToArray(),
            PublishUserId = true,
            EmptyRoomTtl = 0,
            BroadcastPropsChangeToAll = true,
        }, null);

        bl_LobbyUI.Instance.blackScreenFader.FadeIn(0.3f);
        if (bl_AudioController.Instance != null) { bl_AudioController.Instance.StopBackground(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SignOut()
    {
        PlayerPrefs.SetString(PropertiesKeys.GetUniqueKey("remembernick"), string.Empty);
        Disconect();
        bl_LobbyUI.Instance.ChangeWindow("player name");
#if ULSP
        if (bl_PhotonNetwork.Instance != null) bl_PhotonNetwork.LocalPlayer.NickName = string.Empty;
        bl_LoginProDataBase.Instance.SignOut();
#endif
    }

    #region UGUI
    public void SetRememberMe(bool value)
    {
        rememberMe = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckRoomPassword(RoomInfo room)
    {
        checkingRoom = room;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool SetRoomPassworld(string pass)
    {
        if (checkingRoom == null)
        {
            Debug.Log("Checking room is not assigned more!");
            return false;
        }

        if ((string)checkingRoom.CustomProperties[PropertiesKeys.RoomPassword] == pass && checkingRoom.PlayerCount < checkingRoom.MaxPlayers)
        {
            if (PhotonNetwork.GetPing() < (int)checkingRoom.CustomProperties[PropertiesKeys.MaxPing])
            {
                bl_LobbyUI.Instance.blackScreenFader.FadeIn(1);
                if (checkingRoom.PlayerCount < checkingRoom.MaxPlayers)
                {
                    PhotonNetwork.JoinRoom(checkingRoom.Name);
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadLocalLevel(string level)
    {
        bl_LobbyUI.Instance.blackScreenFader.FadeIn(0.75f, () =>
         {
             bl_UtilityHelper.LoadLevel(level);
         });
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveToGameScene()
    {
        //Wait for check
        SetLobbyChat(false);
        if (bl_AudioController.Instance != null) { bl_AudioController.Instance.StopBackground(); }
        while (!bl_PhotonNetwork.InRoom)
        {
            yield return null;
        }
        bl_PhotonNetwork.IsMessageQueueRunning = false;
        bl_UtilityHelper.LoadLevel((string)bl_PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.SceneNameKey]);
    }

    /// <summary>
    /// 
    /// </summary>
    void SetUpGameModes()
    {
        var gameModes = new List<GameModeSettings>();
        for (int i = 0; i < bl_GameData.Instance.gameModes.Count; i++)
        {
            if (bl_GameData.Instance.gameModes[i].isEnabled)
            {
                gameModes.Add(bl_GameData.Instance.gameModes[i]);
            }
        }
        GameModes = gameModes.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnApplicationQuit()
    {
        quittingApp = true;
        bl_GameData.isDataCached = false;
        Disconect();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetLobbyChat(bool connect)
    {
        if (bl_LobbyChat.Instance == null) return;
        if (!bl_GameData.Instance.UseLobbyChat) return;

        if (connect) { bl_LobbyChat.Instance.Connect(bl_GameData.Instance.GameVersion); }
        else { bl_LobbyChat.Instance.Disconnect(); }
    }

    #region Photon Callbacks

    /// <summary>
    /// Called once the game successfully establish connection with the game server
    /// </summary>
    public void OnConnected()
    {
        if (PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
        {
            Debug.Log("Server connection established to: " + PhotonNetwork.CloudRegion);
        }
        else
        {
            Debug.Log($"Server connection established to: {PhotonNetwork.ServerAddress} ({PhotonNetwork.Server.ToString()})");
        }
    }

    /// <summary>
    /// Called once the game successfully establish connection with the master server
    /// </summary>
    public void OnConnectedToMaster()
    {
        // MFPS intermediately connect to a 'common' lobby.
        PhotonNetwork.JoinLobby();
        Debug.Log("Connected to Master.");
    }

    /// <summary>
    /// Called when the player successfully connect to the common lobby
    /// </summary>
    public void OnJoinedLobby()
    {
        bl_LobbyUI.Instance.Home();

        // Require session verification
        if (string.IsNullOrEmpty(bl_PhotonNetwork.NickName))
        {
            connectionState = LobbyConnectionState.Authenticating;
            Debug.Log($"Player has joined to the lobby, now verifying session...");
            RequestSessionVerification();
            bl_LobbyLoadingScreenBase.Instance.SetText(bl_GameTexts.VerifyingSession.Localized(199));
        }
        else
        {
            connectionState = LobbyConnectionState.Connected;
            Debug.Log($"Player <b>{bl_PhotonNetwork.LocalPlayer.NickName}</b> joined to the lobby, UserId: {bl_PhotonNetwork.LocalPlayer.UserId}");
            bl_LobbyLoadingScreenBase.Instance.SetActive(true).HideIn(2, true);
        }

        SetLobbyChat(true);
        ResetValues();
    }

    /// <summary>
    /// 
    /// </summary>
    private void RequestSessionVerification()
    {
        var user = new string[] { CachePlayerName };
        PhotonNetwork.FindFriends(user);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cause"></param>
    public void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.DisconnectByClientLogic)
        {
            if (quittingApp)
            {
                Debug.Log("Disconnect from Server!");
                return;
            }
            if (ServerRegionID == -1 && connectionState != LobbyConnectionState.Authenticating && PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
            {
                Debug.Log("Disconnect from cloud!");
            }
            else if (connectionState == LobbyConnectionState.ChangingRegion)
            {
                Debug.Log($"Changing server! region to: {ServerRegionID}");
                ConnectToServerRegion(ServerRegionID);
                bl_LobbyRoomListBase.Instance?.ClearList();
                return;
            }
            else if (connectionState == LobbyConnectionState.Authenticating)
            {
                Debug.Log($"Authentication complete, finishing connection...");
                bl_LobbyLoadingScreenBase.Instance.SetText(bl_GameTexts.FinishingUp.Localized(198));
                if (PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
                {
                    PhotonNetwork.AuthValues = new AuthenticationValues(bl_PhotonNetwork.NickName);
                    if (ServerRegionID == -1)
                    {
                        ConnectPhoton();
                    }
                    else
                    {
                        ConnectToServerRegion(ServerRegionID);
                    }
                }
                else // if photon server is used
                {
                    ConnectPhoton();
                }
                return;
            }
            else
            {
                Debug.Log("Disconnect from Server.");
            }
        }
        else
        {
            bl_LobbyUI.Instance.blackScreenFader.FadeOut(1);
            bl_LobbyUI.Instance.connectionPopupMessage.SetConfirmationText(bl_GameTexts.TryAgain.ToUpper());

            // CustomAuthenticationFailed is handled in OnCustomAuthenticationFailed(...)
            if (cause != DisconnectCause.CustomAuthenticationFailed)
            {
                bl_LobbyUI.Instance.connectionPopupMessage.AskConfirmation(string.Format(bl_GameTexts.DisconnectCause.Localized(41), cause.ToString()), () =>
                {
                    ConnectPhoton();
                    Debug.LogWarning("Failed to connect to server, cause: " + cause);
                });
            }         
        }
        connectionState = LobbyConnectionState.Disconnected;
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room list updated, total rooms: " + roomList.Count);
        ServerList(roomList);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {

    }

    public void ResetValues()
    {
        if (bl_LobbyRoomCreatorUI.Instance == null) { bl_LobbyRoomCreatorUI.Instance = transform.GetComponentInChildren<bl_LobbyRoomCreatorUI>(true); }
        //Create a random name for a future room that player create
        bl_LobbyRoomCreatorUI.Instance.SetupSelectors();
        bl_PhotonNetwork.IsMessageQueueRunning = true;
    }

    public void OnJoinedRoom()
    {
        if (!bl_MFPS.GameData.UsingWaitingRoom())
        {
            Debug.Log($"Local client joined to the room '{bl_PhotonNetwork.CurrentRoom.Name}'");
            StartCoroutine(MoveToGameScene());
        }
        else
        {
            SetLobbyChat(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        Debug.LogWarning($"Custom authentication response");
        foreach (var item in data)
        {
            Debug.Log($"CA Pair: {item.Key}: {item.Value}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="debugMessage"></param>
    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        bl_LobbyUI.Instance.connectionPopupMessage.AskConfirmation($"Custom Authentication Failed: {debugMessage}", () =>
        {
            ConnectPhoton();
        });

        Debug.LogWarning($"Custom Authentication Failed: {debugMessage}");
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
    }

    public void OnLeftLobby()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="friendList"></param>
    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        if (string.IsNullOrEmpty(bl_PhotonNetwork.NickName))
        {
            foreach (var player in friendList)
            {
                if(player.UserId == CachePlayerName && player.IsOnline)
                {
                    // Another session is open
                    Debug.Log("This account has a session open.");
                    bl_LobbyUI.Instance.connectionPopupMessage.SetConfirmationText(bl_GameTexts.Ok.ToUpper());
                    bl_LobbyUI.Instance.connectionPopupMessage.AskConfirmation(bl_GameTexts.AnotherSessionOpen.Localized(197), () =>
                    {
                        connectionState = LobbyConnectionState.Disconnected;
                        Disconect();
                        bl_LobbyLoadingScreenBase.Instance.HideIn(0.2f, true);
#if !ULSP
                        bl_LobbyUI.Instance.ChangeWindow("player name");
#else
                        SignOut();
#endif
                    });
                    return;
                }
            }

            // No session open for this account
            connectionState = LobbyConnectionState.Authenticating;
            bl_PhotonNetwork.NickName = CachePlayerName;
            bl_EventHandler.DispatchCoinUpdate(null);
            Disconect();
        }
    }

    public void OnCreatedRoom()
    {
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        OnNoRoomsToJoin(returnCode, message);
    }

    public void OnLeftRoom()
    {
    }
#endregion

    public AddonsButtonsHelper AddonsButtons = new AddonsButtonsHelper();
    public class AddonsButtonsHelper
    {
        public GameObject this[int index]
        {
            get
            {
                return bl_LobbyUI.Instance.AddonsButtons[index];
            }
        }
    }

    private static bl_Lobby _instance;
    public static bl_Lobby Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<bl_Lobby>();
            }
            return _instance;
        }
    }
}