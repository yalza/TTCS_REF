using System.Collections;
using UnityEngine;

namespace MFPS.Audio
{
    public class bl_AudioController : MonoBehaviour
    {
        [Header("Audio Handler")]
        public bl_VirtualAudioController audioController;

        [Header("Scene Settings")]
        public float maxWeaponDistance = 75;
        public float maxExplosionDistance = 100;
        public float maxFootstepDistance = 30;
        public AudioRolloffMode audioRolloffMode = AudioRolloffMode.Logarithmic;

        [Header("Background")]
        [SerializeField] private AudioClip BackgroundClip;
        public float MaxBackgroundVolume = 0.3f;
        public AudioSource backgroundSource;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            audioController.Initialized(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            MaxBackgroundVolume = bl_MFPS.MusicVolume;
        }

        /// <summary>
        /// 
        /// </summary>
        public void PlayClip(string clipName)
        {
            audioController.PlayClip(clipName);
        }

        /// <summary>
        /// 
        /// </summary>
        public void PlayBackground()
        {
            if (BackgroundClip == null) return;
            if (backgroundSource == null) { backgroundSource = gameObject.AddComponent<AudioSource>(); }

            backgroundSource.clip = BackgroundClip;
            backgroundSource.volume = 0;
            backgroundSource.playOnAwake = false;
            backgroundSource.loop = true;
            StartCoroutine(FadeAudio(backgroundSource, true, MaxBackgroundVolume));
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopBackground()
        {
            if (backgroundSource == null) return;

            FadeAudio(backgroundSource, false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ForceStopAllFades()
        {
            StopAllCoroutines();
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator FadeAudio(AudioSource source, bool up, float volume = 1)
        {
            if (up)
            {
                source.Play();
                while (source.volume < volume)
                {
                    source.volume += Time.deltaTime * 0.01f;
                    yield return null;
                }
            }
            else
            {
                while (source.volume > 0)
                {
                    source.volume -= Time.deltaTime * 0.5f;
                    yield return null;
                }
            }
        }

        public float BackgroundVolume
        {
            set
            {
                if (backgroundSource != null) { backgroundSource.volume = value; }
            }
        }


        private static bl_AudioController _instance;
        public static bl_AudioController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<bl_AudioController>();
                }
                return _instance;
            }
        }
    }
}