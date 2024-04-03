using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class bl_AudioFader : MonoBehaviour
    {
        [Range(0, 1)] public float TargetVolume = 1;
        [Range(0, 5)] public float Delay = 0;
        [Header("Fade In")]
        public bool FadeInOnStart = true;
        public float FadeDutation = 1;
        public AnimationCurve FadeInCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        private AudioSource ASource;

        void OnEnable()
        {
            ASource = GetComponent<AudioSource>();
            if (FadeInOnStart)
            {
                StartCoroutine(FadeIn());
            }
        }

        IEnumerator FadeIn()
        {
            ASource.volume = 0;
            if (Delay > 0) { yield return new WaitForSeconds(Delay); }
            ASource.Play();
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime / FadeDutation;
                float t = FadeInCurve.Evaluate(d);
                ASource.volume = Mathf.Clamp(0, TargetVolume, t);
                yield return null;
            }
        }
    }
}