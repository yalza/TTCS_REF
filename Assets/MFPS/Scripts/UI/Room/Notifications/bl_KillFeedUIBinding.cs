using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MFPS.Internal.Structures;
using TMPro;

namespace MFPS.Runtime.UI.Bindings
{
    public class bl_KillFeedUIBinding : bl_KillFeedUIBindingBase
    {
        [SerializeField] private TextMeshProUGUI KillerText = null;
        [SerializeField] private TextMeshProUGUI KilledText = null;
        [SerializeField] private TextMeshProUGUI WeaponText = null;
        public Image WeaponIconImg;
        [SerializeField] private Image KillTypeImage = null;
        private CanvasGroup Alpha;

        /// <summary>
        /// 
        /// </summary>
        public override void Init(KillFeed feed)
        {
            if (Alpha == null) Alpha = GetComponent<CanvasGroup>();
            StopAllCoroutines();
            SetActiveAll(true);
            Alpha.alpha = 1;
            switch (feed.messageType)
            {
                case KillFeedMessageType.WeaponKillEvent:
                    OnKillMessage(feed);
                    break;
                case KillFeedMessageType.Message:
                    OnMessage(feed);
                    break;
                case KillFeedMessageType.TeamHighlightMessage:
                    OnTeamHighight(feed);
                    break;
            }
            gameObject.SetActive(true);
            StartCoroutine(Hide(10));
        }

        /// <summary>
        /// 
        /// </summary>
        void OnKillMessage(KillFeed info)
        {
            KillerText.text = info.Killer;
            KilledText.text = info.Killed;
            KillerText.color = isLocalPlayerName(info.Killer) ? bl_GameData.Instance.highLightColor : info.KillerTeam.GetTeamColor();
            KilledText.color = isLocalPlayerName(info.Killed) ? bl_GameData.Instance.highLightColor : GetOppositeTeam(info.KillerTeam).GetTeamColor();
            if (bl_GameData.Instance.killFeedWeaponShowMode == KillFeedWeaponShowMode.WeaponName)
            {
                WeaponIconImg.gameObject.SetActive(false);
                WeaponText.text = info.Message;
            }
            else
            {
                WeaponText.gameObject.SetActive(false);
                Sprite icon = null;
                if (info.GunID >= 0)
                {
                    icon = bl_GameData.Instance.GetWeapon(info.GunID).GunIcon;
                }
                else
                {
                    if (!string.IsNullOrEmpty(info.Message))
                        icon = bl_KillFeedBase.Instance.GetCustomIcon(info.Message);

                    if (icon == null)
                    {
                        int normalizedID = Mathf.Abs(info.GunID + 1);
                        if (normalizedID <= bl_KillFeedBase.Instance.customIcons.Count - 1)
                        {
                            icon = bl_KillFeedBase.Instance.customIcons[normalizedID].Icon;
                        }
                    }
                }
                WeaponIconImg.gameObject.SetActive(icon != null);
                WeaponIconImg.sprite = icon;
            }
            KillTypeImage.gameObject.SetActive(info.HeadShot);
        }

        /// <summary>
        /// 
        /// </summary>
        void OnMessage(KillFeed info)
        {
            DisableAll();
            KillerText.gameObject.SetActive(true);
            KillerText.text = info.Message;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnTeamHighight(KillFeed info)
        {
            DisableAll();
            KillerText.gameObject.SetActive(true);
            string hex = ColorUtility.ToHtmlStringRGB(info.KillerTeam.GetTeamColor());
            KillerText.text = string.Format("<color=#{0}>{1}</color> {2}", hex, info.Killer, info.Message);
        }

        /// <summary>
        /// 
        /// </summary>
        void DisableAll()
        {
            SetActiveAll(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        void SetActiveAll(bool active)
        {
            KillerText.gameObject.SetActive(active);
            KilledText.gameObject.SetActive(active);
            WeaponText.gameObject.SetActive(active);
            WeaponIconImg.gameObject.SetActive(active);
            KillTypeImage.gameObject.SetActive(active);
        }

        Team GetOppositeTeam(Team team)
        {
            if (isOneTeamMode || team == Team.None) { return team; }

            return (team == Team.Team1) ? Team.Team2 : Team.Team1;
        }

        bool isLocalPlayerName(string playerName) { return playerName == bl_PhotonNetwork.LocalPlayer.NickName; }

        IEnumerator Hide(float time)
        {
            yield return new WaitForSeconds(time);
            while (Alpha.alpha > 0)
            {
                Alpha.alpha -= Time.deltaTime;
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}