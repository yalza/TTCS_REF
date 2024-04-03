using UnityEngine;

namespace MFPS.Runtime.UI
{
    public abstract class bl_LobbyLoadingScreenBase : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public abstract bl_LobbyLoadingScreenBase SetActive(bool active);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public abstract bl_LobbyLoadingScreenBase SetText(string text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay"></param>
        public abstract bl_LobbyLoadingScreenBase HideIn(float delay, bool faded);


        private static bl_LobbyLoadingScreenBase _instance;
        public static bl_LobbyLoadingScreenBase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<bl_LobbyLoadingScreenBase>();
                }
                return _instance;
            }
        }
    }
}