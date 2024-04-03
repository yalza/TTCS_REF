using System.Collections;
using TMPro;
using UnityEngine;

namespace MFPS.Runtime.UI
{
    /// <summary>
    /// Default lobby connection loading screen
    /// You can use your custom one by inherit your script from bl_LobbyLoadingScreenBase
    /// and replacing the 'Loading' UI from the Lobby canvas.
    /// </summary>
    public class bl_LobbyLoadingScreen : bl_LobbyLoadingScreenBase
    {
        public GameObject content;
        public TextMeshProUGUI loadingText;
        public CanvasGroup canvasGroup;
        public Animator bottomAnimator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay"></param>
        public override bl_LobbyLoadingScreenBase HideIn(float delay, bool faded)
        {
            StopAllCoroutines();
            StartCoroutine(Hide(delay, faded));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public override bl_LobbyLoadingScreenBase SetActive(bool active)
        {
            content.SetActive(active);
            if (active)
            {
                if (bottomAnimator != null)
                {
                    bottomAnimator.SetBool("show", true);
                }
                if (canvasGroup != null) canvasGroup.alpha = 1;
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public override bl_LobbyLoadingScreenBase SetText(string text)
        {
            if (loadingText != null) loadingText.text = text;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        IEnumerator Hide(float delay, bool faded)
        {
            yield return new WaitForSeconds(delay);
            if(faded)
            {
                float d = 1;
                bottomAnimator.SetBool("show", false);
                while (d > 0)
                {
                    d -= Time.deltaTime / 0.5f;
                    canvasGroup.alpha = bl_LobbyUI.Instance.blackScreenFader.fadeCurve.Evaluate(d);
                    yield return null;
                }
            }
            SetActive(false);
        }
    }
}