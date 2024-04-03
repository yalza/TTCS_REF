using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[DefaultExecutionOrder(-1000)]
public class bl_OfflineRoom : MonoBehaviour, IConnectionCallbacks
{
    [Header("Offline Room")]
    public GameMode gameMode = GameMode.FFA;
    [LovattoToogle] public bool forceOffline = false;
    [LovattoToogle] public bool withBots = false;
    [LovattoToogle] public bool autoTeamSelection = true;
    [LovattoToogle] public bool friendlyFire = false;
    [Range(1, 64)] public int maxPlayers = 1;
    public int MatchTime = 9989;
    public int gameModeGoal = 100;
    public RoundStyle roundStyle = RoundStyle.OneMacht;
    [Header("References")]
    public GameObject PhotonObject;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
        if (!bl_PhotonNetwork.IsConnectedInRoom)
        {
            if (bl_GameData.Instance.offlineMode || forceOffline)
            {
#if CLASS_CUSTOMIZER
                bl_ClassManager.Instance.Init();
#endif
                bl_Input.Initialize();
                PhotonNetwork.OfflineMode = true;
                PhotonNetwork.NickName = "Offline Player";
                if(bl_PhotonNetwork.Instance == null)
                Instantiate(PhotonObject);
            }
            else
            {
                PhotonNetwork.OfflineMode = false;
            }
        }
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
    public void OnConnectedToMaster()
    {
        Debug.Log("Offline Connected to Master");
        Hashtable roomOption = new Hashtable();
        roomOption[PropertiesKeys.TimeRoomKey] = MatchTime;
        roomOption[PropertiesKeys.GameModeKey] = gameMode.ToString();
        roomOption[PropertiesKeys.SceneNameKey] = bl_GameData.Instance.AllScenes[0].RealSceneName;
        roomOption[PropertiesKeys.RoomRoundKey] = roundStyle;
        roomOption[PropertiesKeys.TeamSelectionKey] = autoTeamSelection;
        roomOption[PropertiesKeys.CustomSceneName] = bl_GameData.Instance.AllScenes[0].ShowName;
        roomOption[PropertiesKeys.RoomGoal] = gameModeGoal;
        roomOption[PropertiesKeys.RoomFriendlyFire] = friendlyFire;
        roomOption[PropertiesKeys.MaxPing] = 1000;
        roomOption[PropertiesKeys.RoomPassword] = "";
        roomOption[PropertiesKeys.WithBotsKey] = withBots;
        PhotonNetwork.CreateRoom("Offline Room", new RoomOptions()
        {
            MaxPlayers = (byte)maxPlayers,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = roomOption,
            CleanupCacheOnLeave = true,
            PublishUserId = true,
            EmptyRoomTtl = 0,
        }, null);
    }


    public void OnConnected()
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {      
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {     
    }

    public void OnDisconnected(DisconnectCause cause)
    {     
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {      
    }

    private static bl_OfflineRoom _instance;
    public static bl_OfflineRoom Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_OfflineRoom>(); }
            return _instance;
        }
    }
}