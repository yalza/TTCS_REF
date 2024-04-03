using UnityEngine;
using TMPro;

namespace MFPS.Runtime.UI.Layout
{
    public class bl_RoundFinishScreen : bl_RoundFinishScreenBase
    {
        public GameObject content;
        [SerializeField] private TextMeshProUGUI FinalUIText = null;
        [SerializeField] private TextMeshProUGUI FinalCountText = null;
        [SerializeField] private TextMeshProUGUI FinalWinnerText = null;

        /// <summary>
        /// Show the final round UI
        /// </summary>
        public override void Show(string winner)
        {
            content.SetActive(true);
            FinalUIText.text = (bl_RoomSettings.Instance.CurrentRoomInfo.roundStyle == RoundStyle.OneMacht) ? bl_GameTexts.FinalOneMatch.Localized(38) : bl_GameTexts.FinalRounds.Localized(32);
            FinalWinnerText.text = string.Format("{0} {1}", winner, bl_GameTexts.FinalWinner).Localized(33).ToUpper();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Hide()
        {
            content.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        public override void SetCountdown(int count)
        {
            count = Mathf.Clamp(count, 0, int.MaxValue);
            FinalCountText.text = count.ToString();
        }
    }
}