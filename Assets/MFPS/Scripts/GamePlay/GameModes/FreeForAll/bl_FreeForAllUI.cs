using UnityEngine;
using TMPro;

namespace MFPS.GameModes.FreeForAll
{
    public class bl_FreeForAllUI : MonoBehaviour
    {
        public GameObject Content;
        public TextMeshProUGUI ScoreText;

        public void SetScores(MFPSPlayer bestPlayer)
        {
            string scoreText = string.Format(bl_GameTexts.PlayerStart, bestPlayer.Name);
            ScoreText.text = scoreText;
        }

        public void ShowUp()
        {
            if (!bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.TopScoreBoard)) return;

            Content.SetActive(true);
        }

        public void Hide()
        {
            Content.SetActive(false);
        }

        private static bl_FreeForAllUI _instance;
        public static bl_FreeForAllUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<bl_FreeForAllUI>();
                }
                return _instance;
            }
        }
    }
}