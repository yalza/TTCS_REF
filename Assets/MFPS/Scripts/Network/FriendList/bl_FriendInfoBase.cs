using Photon.Realtime;
using UnityEngine;

namespace MFPS.Runtime.FriendList
{
    public abstract class bl_FriendInfoBase : MonoBehaviour
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="uiManager"></param>
        public abstract void SetFriendData(FriendInfo info, bl_FriendListUIBase uiManager);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="infos"></param>
        public abstract void UpdateFriendInfo(FriendInfo[] infos);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public abstract void SetActive(bool active);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expand"></param>
        public abstract void Expand(bool expand);
    }
}