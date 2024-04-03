using UnityEngine;

namespace MFPS.Runtime.FriendList
{
    public abstract class bl_FriendListUIBase : MonoBehaviour
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rebuild"></param>
        public abstract void UpdateFriendList(bool rebuild = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public abstract void ShowMessage(string message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public abstract void SetActiveList(bool active);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract bool IsOpen();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerNick"></param>
        /// <returns></returns>
        public abstract bool IsPlayerListed(string playerNick);
    }
}