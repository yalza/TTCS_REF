using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Runtime.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class bl_CanvasGroupFader : MonoBehaviour
    {
        [Range(0.1f, 3)] public float duration = 0.75f;
        public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private CanvasGroup m_canvasGroup;
        private CanvasGroup canvasGroup
        {
            get
            {
                if (m_canvasGroup == null) m_canvasGroup = GetComponent<CanvasGroup>();
                return m_canvasGroup;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alpha"></param>
        public void SetAlpha(float alpha)
        {
            canvasGroup.alpha = alpha; if(alpha > 0) gameObject.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Coroutine FadeIn(float lenght = 0, Action onFinish = null)
        {
            StopAllCoroutines();
            gameObject.SetActive(true);
            return StartCoroutine(DoFadeIn(lenght, onFinish));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Coroutine FadeOut(float lenght = 0, Action onFinish = null)
        {
            StopAllCoroutines();
            gameObject.SetActive(true);
            return StartCoroutine(DoFadeOut(lenght, onFinish));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator DoFadeIn(float lenght = 0, Action onFinish = null)
        {
            float d = 0;
            float t = 0;
            float dur = lenght <= 0 ? duration : lenght;
            while (d < 1)
            {
                d += Time.deltaTime / dur;
                t = fadeCurve.Evaluate(d);
                canvasGroup.alpha = t;
                yield return null;
            }
            onFinish?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator DoFadeOut(float lenght = 0, Action onFinish = null)
        {
            float d = 0;
            float t = 0;
            float dur = lenght <= 0 ? duration : lenght;
            while (d < 1)
            {
                d += Time.deltaTime / dur;
                t = fadeCurve.Evaluate(d);
                canvasGroup.alpha = 1 - t;
                yield return null;
            }
            onFinish?.Invoke();
            gameObject.SetActive(false);
        }
    }
}