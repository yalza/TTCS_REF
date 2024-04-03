using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

namespace MFPS.Runtime.UI
{
    public class bl_RoomListItemUI : bl_RoomListItemUIBase
    {
        public TextMeshProUGUI RoomNameText = null;
        public TextMeshProUGUI MapNameText = null;
        public TextMeshProUGUI PlayersText = null;
        public TextMeshProUGUI GameModeText = null;
        public TextMeshProUGUI PingText = null;
        [SerializeField] private TextMeshProUGUI MaxKillText = null;
        public GameObject JoinButton = null;
        public GameObject FullText = null;
        [SerializeField] private GameObject PrivateUI = null;
        private RoomInfo cacheInfo = null;

        /// <summary>
        /// This method assign the RoomInfo and get the properties of it
        /// to display in the UI
        /// </summary>
        /// <param name="info"></param>
        public override void SetInfo(RoomInfo info)
        {
            cacheInfo = info;
            if(RoomNameText != null)  RoomNameText.text = info.Name;
            if (MapNameText != null) MapNameText.text = (string)info.CustomProperties[PropertiesKeys.CustomSceneName];
            if (GameModeText != null) GameModeText.text = (string)info.CustomProperties[PropertiesKeys.GameModeKey];
            if (PlayersText != null) PlayersText.text = info.PlayerCount + "/" + info.MaxPlayers;
            if (MaxKillText != null) MaxKillText.text = string.Format("{0} {1}", info.CustomProperties[PropertiesKeys.RoomGoal], info.GetGameMode().GetModeInfo().GoalName);
            if (PingText != null) PingText.text = ((int)info.CustomProperties[PropertiesKeys.MaxPing]).ToString() + " ms";

            bool _active = (info.PlayerCount < info.MaxPlayers) ? true : false;
            PrivateUI.SetActive((string.IsNullOrEmpty((string)cacheInfo.CustomProperties[PropertiesKeys.RoomPassword]) == false));
            JoinButton.SetActive(_active);
            FullText.SetActive(!_active);
        }

        /// <summary>
        /// Join to the room that this UI Row represent
        /// </summary>
        public void JoinRoom()
        {
            //If the local player ping is higher than the max allowed in the room
            if (PhotonNetwork.GetPing() >= (int)cacheInfo.CustomProperties[PropertiesKeys.MaxPing])
            {
                //display the message and Don't join to the room
                bl_LobbyUI.Instance.ShowPopUpWindow("max room ping");
                return;
            }

            //if the room doesn't require a password
            if (string.IsNullOrEmpty((string)cacheInfo.CustomProperties[PropertiesKeys.RoomPassword]))
            {
                bl_LobbyUI.Instance.blackScreenFader.FadeIn(1);
                if (cacheInfo.PlayerCount < cacheInfo.MaxPlayers)
                {
                    PhotonNetwork.JoinRoom(cacheInfo.Name);
                }
            }
            else
            {
                bl_LobbyUI.Instance.CheckRoomPassword(cacheInfo);
            }
        }
    }
}