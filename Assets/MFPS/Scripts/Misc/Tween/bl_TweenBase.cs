using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPS.Tween
{
    public class bl_TweenBase : MonoBehaviour
    {

        [SerializeField] public class UEvent : UnityEvent { }

        public float Delta
        {
            get
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    return Time.deltaTime;
                }
                else { return Time.deltaTime; }
#else
             return Time.unscaledDeltaTime;
#endif
            }
        }

        [Serializable]
        public enum TweenOrigin
        {
            From,
            To
        }


#if UNITY_EDITOR
        public virtual void CacheDefaultValues() { }
        public virtual void ResetDefault() { }
        public virtual void InitInEditor() { }
        public virtual void PlayEditor() { }
        public virtual void PlayReverseEditor() { }
#endif
    }
}