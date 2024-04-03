using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace MFPS.Tween
{
    [RequireComponent(typeof(CanvasGroup))]
    public class bl_TweenCurveAlpha : bl_TweenBase, ITween
    {
        [Header("Settings")]
        [LovattoToogle] public bool OnStart = true;
        [LovattoToogle] public bool AlphaOnStart = true;
        [LovattoToogle] public bool Loop = false;
        [Range(0, 10)] public float Delay = 0;
        [Range(0.1f, 10)] public float Duration = 1;
        public AnimationCurve m_Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public EasingType m_EasingInType = EasingType.Quintic;
        public EasingMode m_EasingMode = EasingMode.Out;

        [System.Serializable] public class OnFinish : UnityEvent { }
        [Header("Event")]
        [SerializeField]
        private OnFinish m_OnFinish = new OnFinish();
        private CanvasGroup m_Canvas;
        private float duration = 0;
        private bool isInitializated = false;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            m_Canvas = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// 
        /// </summary>
        void OnEnable()
        {
            if(m_Canvas == null)
            m_Canvas = GetComponent<CanvasGroup>();
            if (m_Canvas == null) { Debug.Log("Not canvas", gameObject); return; }

            if (!isInitializated)
            {
                isInitializated = true;
            }
            if (AlphaOnStart) { m_Canvas.alpha = m_Curve.keys[0].value; }
            if (OnStart)
            {
                StartTween();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartTween()
        {
            duration = 0;
            StopAllCoroutines();
            StartCoroutine(DoTween());
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartReverseTween(bool desactive = false)
        {
            duration = 0;
            StopAllCoroutines();
            StartCoroutine(DoTweenReverse(desactive));
        }


        /// <summary>
        /// 
        /// </summary>
        void OnDisable()
        {
            duration = 0;
            StopAllCoroutines();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator DoTween()
        {
            if (m_Canvas == null) yield break;
            bool isPlaying = Application.isPlaying;
#if UNITY_EDITOR
            if (isPlaying)
            {
                if (Delay > 0) { yield return new WaitForSecondsRealtime(Delay); }
            }
#else
        if(Delay > 0) { yield return new WaitForSecondsRealtime(Delay); }
#endif
            float time = 0;
            while (time < 1)
            {
#if UNITY_EDITOR
                if (isPlaying)
                {
                    duration += Time.deltaTime / Duration;
                }
                else
                {
                    duration += 0.012f / Duration;
                }
#else
                    duration += Time.deltaTime / Duration;
#endif
                time = Mathf.Lerp(time, duration, Easing.Do(duration, m_EasingInType, m_EasingMode));
                m_Canvas.alpha = m_Curve.Evaluate(time);
                yield return null;
            }
            if (!Loop)
            {
                if (m_OnFinish != null)
                    m_OnFinish.Invoke();
            }
            else
            {
                if (Application.isPlaying)
                {
                    duration = 0;
                    StartTween();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator DoTweenReverse(bool desactive)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                if (Delay > 0) { yield return new WaitForSecondsRealtime(Delay); }
            }
            else
            {
                //if (Delay > 0) { yield return new EditorWaitForSeconds(Delay); }
            }
#else
        if(Delay > 0) { yield return new WaitForSecondsRealtime(Delay); }
#endif
            duration = (duration > 0) ? duration : 1;
            duration = Mathf.Clamp(duration, 0, 1);
            float time = duration;
            while (time > 0)
            {
                duration -= Time.deltaTime / Duration;
                m_Canvas.alpha = m_Curve.Evaluate(time);
                time = Mathf.Lerp(time, duration, Easing.Do(1 - duration, m_EasingInType, m_EasingMode));
                yield return null;
            }
            if (m_OnFinish != null)
                m_OnFinish.Invoke();

            if (desactive) { gameObject.SetActive(false); }
        }

#if UNITY_EDITOR
        public override void PlayEditor()
        {
            InitInEditor();
            MFPSEditor.EditorCoroutines.StartBackgroundTask(DoTween());
        }

        public override void PlayReverseEditor()
        {
            InitInEditor();
            MFPSEditor.EditorCoroutines.StartBackgroundTask(DoTweenReverse(false));
        }
        public override void InitInEditor()
        {
            m_Canvas = GetComponent<CanvasGroup>();
            duration = 0;
            if (AlphaOnStart) { m_Canvas.alpha = m_Curve.keys[0].value; }
        }
#endif
    }
}