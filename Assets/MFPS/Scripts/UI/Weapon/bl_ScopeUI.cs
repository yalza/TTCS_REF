using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace MFPS.Runtime.UI
{
    public class bl_ScopeUI : bl_ScopeUIBase
    {
        public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public GameObject content;
        public Image scopeImage;
        public CanvasGroup scopeAlpha;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public override void SetActive(bool active)
        {
            scopeAlpha.alpha = 0;
            content.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weapon"></param>
        public override void SetupWeapon(bl_SniperScopeBase weapon)
        {
            scopeImage.sprite = weapon.ScopeTexture;
            scopeAlpha.alpha = 0;
        }

        /// <summary>
        /// Show/Hide the scope UI with a fade transition
        /// </summary>
        /// <param name="fadeIn"></param>
        /// <param name="speed"></param>
        public override void Crossfade(bool fadeIn, float speed, float delay = 0, Action onFinish = null, Action onStart = null)
        {
            StopAllCoroutines();
            StartCoroutine(DoFade(fadeIn, speed, delay, onFinish, onStart));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fadeIn"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        IEnumerator DoFade(bool fadeIn, float speed, float delay = 0, Action onFinish = null, Action onStart = null)
        {
            float d = 0;
            float t;
            float origin = scopeAlpha.alpha;
            float target = fadeIn ? 1 : 0;
            if (fadeIn) content.SetActive(true);

            if (delay > 0) yield return new WaitForSeconds(delay);
            onStart?.Invoke();
            while (d < 1)
            {
                d += Time.deltaTime / speed;
                t = transitionCurve.Evaluate(d);
                scopeAlpha.alpha = Mathf.Lerp(origin, target, t);
                yield return null;
            }
            onFinish?.Invoke();
            if(!fadeIn) content.SetActive(false);
        }
    }
}