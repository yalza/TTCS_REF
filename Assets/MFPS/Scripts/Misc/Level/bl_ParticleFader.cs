using System;
using System.Collections;
using UnityEngine;

namespace MFPS.Runtime.Misc
{
    public class bl_ParticleFader : MonoBehaviour
    {
        [LovattoToogle] public bool StartFadeEmit = false;
        [LovattoToogle] public bool DestroyAfterTime = false;
        [Range(1, 20)] public float Emission = 12;
        [Range(1, 10)] public float DestroyTime = 7;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerator Start()
        {
            if (DestroyAfterTime)
            {
                Invoke(nameof(DestroyParticles), DestroyTime);
            }
            if (!StartFadeEmit) yield break;

            ParticleSystem.EmissionModule e = GetComponent<ParticleSystem>().emission;
            e.rateOverTime = 0;
            float t = 0;
            while (t < Emission)
            {
                t += Time.deltaTime * 7;
                e.rateOverTime = t;
                yield return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void DestroyParticles()
        {
            StartCoroutine(FadeOut());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onFinish"></param>
        public void DoFadeOut(bool autoDestroy = true, Action onFinish = null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut(autoDestroy, onFinish));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator FadeOut(bool autoDestroy = true, Action onFinish = null)
        {
            ParticleSystem.EmissionModule e = GetComponent<ParticleSystem>().emission;
            ParticleSystem.MinMaxCurve mc = e.rateOverTime;
            while (mc.constant > 0)
            {
                mc.constant -= Time.deltaTime * 7;
                e.rateOverTime = mc;
                yield return null;
            }
            yield return new WaitForSeconds(GetComponent<ParticleSystem>().main.startLifetime.constant);
            onFinish?.Invoke();

            if (autoDestroy) Destroy(gameObject);
        }
    }
}