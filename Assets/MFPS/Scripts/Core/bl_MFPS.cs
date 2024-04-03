using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using MFPS.Runtime.Settings;
using MFPS.Internal.Scriptables;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using static bl_CameraRayBase;

public static class bl_MFPS
{
    public static float MusicVolume = 1;

    public static bl_RuntimeSettingsProfile Settings => bl_GameData.Instance.RuntimeSettings;
    public static bl_PlayerReferences LocalPlayerReferences => bl_GameManager.Instance.LocalPlayerReferences;
    public static List<bl_GunInfo> AllWeapons => bl_GameData.Instance.AllWeapons;

    public const string LOCAL_PLAYER_TAG = "Player";
    public const string REMOTE_PLAYER_TAG = "Remote";
    public const string AI_TAG = "AI";
    public const string HITBOX_TAG = "BodyPart";
    
    /// <summary>
    /// Class helper with some useful function referenced to the local player only.
    /// </summary>
    public static class LocalPlayer
    {
        public static int ViewID => bl_GameManager.LocalPlayerViewID;
        public static MFPSPlayer MFPSActor => bl_GameManager.Instance.LocalActor;

        /// <summary>
        /// The team of the local player
        /// </summary>
        public static Team Team
        {
            get;
            set;
        }

        /// <summary>
        /// Stats of the local player
        /// </summary>
        public static class Stats
        {
            /// <summary>
            /// Get the all time kill count of the local player
            /// </summary>
            /// <returns></returns>
            public static int GetAllTimeKills()
            {
#if ULSP
                if (bl_DataBase.IsUserLogged)
                {
                    return bl_DataBase.LocalUserInstance.Kills;
                }
                else return 0;
#else
                return PlayerPrefs.GetInt(PropertiesKeys.GetUniqueKeyForPlayer("kills", bl_PhotonNetwork.NickName));
#endif
            }

            /// <summary>
            /// Get the all time death count of the local player
            /// </summary>
            /// <returns></returns>
            public static int GetAllTimeDeaths()
            {
#if ULSP
                if (bl_DataBase.IsUserLogged)
                {
                    return bl_DataBase.LocalUserInstance.Deaths;
                }
                else return 0;
#else
                return PlayerPrefs.GetInt(PropertiesKeys.GetUniqueKeyForPlayer("deaths", bl_PhotonNetwork.NickName));
#endif
            }

            /// <summary>
            /// Get the all time score of the local player
            /// </summary>
            /// <returns></returns>
            public static int GetAllTimeScore()
            {
#if ULSP
                if (bl_DataBase.IsUserLogged)
                {
                    return bl_DataBase.LocalUserInstance.Score;
                }
                else return 0;
#else
                return PlayerPrefs.GetInt(PropertiesKeys.GetUniqueKeyForPlayer("score", bl_PhotonNetwork.NickName));
#endif
            }
        }

        /// <summary>
        /// Make the local player die
        /// </summary>
        public static bool Suicide(bool increaseWarnings = true)
        {
            if (!PhotonNetwork.InRoom || bl_GameManager.Instance.LocalPlayerReferences == null) return false;

            var pdm = bl_GameManager.Instance.LocalPlayerReferences.playerHealthManager;
            if (!pdm.Suicide()) return false;

            bl_UtilityHelper.LockCursor(true);
            if(increaseWarnings)
            bl_GameManager.SuicideCount++;
            //if player is a joker o abuse of suicide, them kick of room
            if (bl_GameManager.SuicideCount > bl_GameData.Instance.maxSuicideAttempts)
            {              
                IsAlive = false;
                bl_UtilityHelper.LockCursor(false);
                bl_RoomMenu.Instance.LeaveRoom();
            }
            return true;
        }

        /// <summary>
        /// Is the Local player alive?
        /// </summary>
        public static bool IsAlive
        {
            get
            {
                return bl_GameManager.Instance.LocalActor.isAlive;
            }
            set
            {
                bl_GameManager.Instance.LocalActor.isAlive = value;
            }
        }

        /// <summary>
        /// Will Return the local player nick name with the clan/crew tag (if any)
        /// </summary>
        /// <returns></returns>
        public static string FullNickName()
        {
            string nick = $"{bl_GameData.Instance.RolePrefix} {bl_PhotonNetwork.NickName}";
            return nick;
        }

        /// <summary>
        /// Make the local player ignore colliders
        /// </summary>
        public static void IgnoreColliders(Collider[] colliders, bool ignore)
        {
            if (LocalPlayerReferences == null) return;

            LocalPlayerReferences.playerRagdoll.IgnoreColliders(colliders, ignore);
        }

        /// <summary>
        /// Make the player detect an object when the camera look at it
        /// </summary>
        public static byte AddCameraRayDetection(DetecableInfo detecableInfo)
        {
            if (LocalPlayerReferences == null) return 0;

            return LocalPlayerReferences.cameraRay.AddTrigger(detecableInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RemoveCameraDetection(DetecableInfo detecableInfo)
        {
            if (LocalPlayerReferences == null) return;

            LocalPlayerReferences.cameraRay.RemoveTrigger(detecableInfo);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class Network
    {
        /// <summary>
        /// Send a RPC-like call (without a Photon view required) to all other clients in the same room.
        /// </summary>
        public static void SendNetworkCall(byte code, Hashtable data) => bl_PhotonNetwork.Instance.SendDataOverNetwork(code, data);
    }

    /// <summary>
    /// Static functions for the MFPS coins
    /// </summary>
    public static class Coins
    {
        /// <summary>
        /// Get one of the listed coins in GameData by their ID.
        /// </summary>
        /// <param name="coinID"></param>
        /// <returns></returns>
        public static MFPSCoin GetCoinData(int coinID)
        {
            if (coinID >= bl_GameData.Instance.gameCoins.Count) return null;

            return bl_GameData.Instance.gameCoins[coinID];
        }

        /// <summary>
        /// Get the coin index/id by the coin data
        /// </summary>
        /// <param name="coin"></param>
        /// <returns></returns>
        public static int GetIndexOfCoin(MFPSCoin coin)
        {
            return GetAllCoins().FindIndex(x => x.CoinName == coin.CoinName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<MFPSCoin> GetAllCoins() => bl_GameData.Instance.gameCoins;
    }

    /// <summary>
    /// 
    /// </summary>
    public static class Utility
    {

        /// <summary>
        /// Check if the given photon player is the local player
        /// </summary>
        /// <param name="player">Player to check if is local</param>
        /// <returns></returns>
        public static bool IsLocalPlayer(Player player)
        {
            return player.ActorNumber == bl_PhotonNetwork.LocalPlayer.ActorNumber;
        }

        /// <summary>
        /// Check if the given viewID is the local player
        /// </summary>
        /// <param name="player">ViewID to check if is local</param>
        /// <returns></returns>
        public static bool IsLocalPlayer(int viewID)
        {
            return viewID == LocalPlayer.ViewID;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class GameData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool UsingWaitingRoom()
        {
            return bl_GameData.Instance.lobbyJoinMethod == MFPS.Internal.Structures.LobbyJoinMethod.WaitingRoom;
        }
    }
}