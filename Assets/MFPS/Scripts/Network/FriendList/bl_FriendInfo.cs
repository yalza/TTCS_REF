using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;

namespace MFPS.Runtime.FriendList
{
    public class bl_FriendInfo : bl_FriendInfoBase
    {
        public TextMeshProUGUI NameText = null;
        public TextMeshProUGUI StatusText = null;
        public Image StatusImage = null;
        public GameObject JoinButton = null;
        public GameObject expandInfoUI;
        [Space(5)]
        public Color OnlineColor = new Color(0, 0.9f, 0, 0.9f);
        public Color OffLineColor = new Color(0.9f, 0, 0, 0.9f);

        private string roomName = string.Empty;
        private FriendInfo cacheInfo;
        private string OffLineText = "OFFLINE";
        private string OnlineText = "ONLINE";
        private bl_FriendListUIBase UIManager;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
#if LOCALIZATION
        OffLineText = bl_Localization.Instance.GetText("offline");
        OnlineText = bl_Localization.Instance.GetText("online");
        bl_Localization.Instance.OnLanguageChange += OnLangChange;
#endif
        }

#if LOCALIZATION
    void OnLangChange(Dictionary<string, string> lang)
    {
        OffLineText = lang["offline"];
        OnlineText = lang["online"];
    }
#endif

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
#if LOCALIZATION
        bl_Localization.Instance.OnLanguageChange -= OnLangChange;
#endif
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public override void SetFriendData(FriendInfo info, bl_FriendListUIBase uiManager)
        {
            UIManager = uiManager;
            cacheInfo = info;
            NameText.text = info.UserId;
            UpdateStatusUI(info.IsOnline);
            StatusImage.color = (info.IsOnline) ? OnlineColor : OffLineColor;
            JoinButton.SetActive((info.IsInRoom) ? true : false);
            roomName = info.Room;
            Expand(UIManager.IsOpen());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public override void UpdateFriendInfo(FriendInfo[] infos)
        {
            FriendInfo info = FindMe(infos);
            if (info == null) return;
            NameText.text = info.UserId;
            UpdateStatusUI(info.IsOnline);
            StatusImage.color = (info.IsOnline) ? OnlineColor : OffLineColor;
            JoinButton.SetActive((info.IsInRoom) ? true : false);
            roomName = info.Room;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Expand(bool isExpanded)
        {
            expandInfoUI.SetActive(isExpanded);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="online"></param>
        void UpdateStatusUI(bool online)
        {
            if (StatusText != null) { StatusText.text = string.Format("[{0}]", online ? OnlineText : OffLineText); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private FriendInfo FindMe(FriendInfo[] info)
        {
            for (int i = 0; i < info.Length; i++)
            {
                if (info[i].UserId == cacheInfo.UserId)
                {
                    return info[i];
                }
            }

            Destroy(gameObject);
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void JoinRoom()
        {
            if (!string.IsNullOrEmpty(roomName))
            {
                PhotonNetwork.JoinRoom(roomName);
            }
            else
            {
                Debug.Log("This room doesn't exits more");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Remove()
        {
            bl_FriendListBase.Instance.RemoveFriend(NameText.text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public override void SetActive(bool active) => gameObject.SetActive(active);
    }
}