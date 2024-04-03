using Photon.Realtime;
using TMPro;

namespace MFPS.Runtime.FriendList
{
    public abstract class bl_FriendListBase : bl_PhotonHelper
    {
        /// <summary>
        /// 
        /// </summary>
        public enum Status
        {
            Idle,
            Fetching
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract Status GetStatus();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="friend"></param>
        public abstract void AddFriend(string friend);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        public abstract void AddFriend(TMP_InputField field);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="friend"></param>
        public abstract void RemoveFriend(string friend);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerNick"></param>
        /// <returns></returns>
        public abstract bool IsPlayerFriend(string playerNick);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract bool CanAddMoreFriends();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract FriendInfo[] GetFriends();

        /// <summary>
        /// 
        /// </summary>
        public abstract int FriendsCount { get; }

        /// <summary>
        /// 
        /// </summary>
        private static bl_FriendListBase _instance;
        public static bl_FriendListBase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<bl_FriendListBase>();
                }
                return _instance;
            }
        }
    }
}