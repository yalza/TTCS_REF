using UnityEngine;
using TMPro;

namespace MFPS.Runtime.Misc
{
    public class bl_AFK : bl_MonoBehaviour
    {
        public TextMeshProUGUI afkText;

        private float lastInput;
        private Vector3 oldMousePosition = Vector3.zero;
        private bool Leaving = false;
        private bool Watching = false;
        private float AFKTimeLimit = 60;

        /// <summary>
        /// 
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            AFKTimeLimit = bl_GameData.Instance.AFKTimeLimit;
            if (!bl_GameData.Instance.DetectAFK)
            {
                this.enabled = false;
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnUpdate()
        {
            float time = Time.time;
            //if no movement or action of the player is detected, then start again
            if ((bl_PhotonNetwork.LocalPlayer == null || Input.anyKey) || ((oldMousePosition != Input.mousePosition)))
            {
                lastInput = time;
                if (Watching)
                {
                    afkText.gameObject.SetActive(false);
                    Watching = false;
                }
            }
            else if ((time - lastInput) > AFKTimeLimit * 0.5f)
            {
                Watching = true;
            }
            oldMousePosition = Input.mousePosition;
            if (((lastInput + AFKTimeLimit) - 10f) < time)
            {
                float t = AFKTimeLimit - (time - lastInput);
                if (t >= 0)
                {
                    SetAFKCount(t);
                }
            }
            //If the maximum time is AFK then meets back to the lobby.
            if ((lastInput + AFKTimeLimit) < time && !Leaving)
            {
                bl_UtilityHelper.LockCursor(false);
                bl_PhotonNetwork.Instance.hasAFKKick = true;
                LeaveMatch();
                Leaving = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetAFKCount(float seconds)
        {
            afkText.gameObject.SetActive(true);
#if LOCALIZATION
            afkText.text = string.Format(bl_Localization.Instance.GetText(31), seconds.ToString("F2"));
#else
            afkText.text = string.Format(bl_GameTexts.AFKWarning, seconds.ToString("F2"));
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void LeaveMatch()
        {
            if (bl_PhotonNetwork.IsConnected)
            {
                bl_PhotonNetwork.LeaveRoom();
            }
            else
            {
                bl_UtilityHelper.LoadLevel(bl_GameData.Instance.MainMenuScene);
            }
        }
    }
}