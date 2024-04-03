using UnityEngine;
using MFPS.Internal;
using MFPS.Internal.Structures;
using MFPS.Runtime.UI.Bindings;

namespace MFPS.Runtime.UI
{
    public class bl_RoomNotificationsUI : MonoBehaviour
    {
        public UIListHandler listHandler;
        public float showTime = 5;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            listHandler.Prefab.SetActive(false);
            bl_EventHandler.onLocalNotification += OnLocalNotification;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.onLocalNotification -= OnLocalNotification;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalNotification(MFPSLocalNotification notification)
        {
            listHandler.Initialize();
            listHandler.InstatiateAndGet<bl_UILeftNotifier>().SetInfo(notification.Message, showTime);
        }
    }
}