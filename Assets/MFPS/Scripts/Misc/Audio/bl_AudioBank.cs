using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Audio
{
    [CreateAssetMenu(fileName = "Audio Bank", menuName = "MFPS/Audio/Bank", order = 301)]
    public class bl_AudioBank : ScriptableObject
    {
        [Reorderable] public List<AudioInfo> AudioBank = new List<AudioInfo>();

        public AudioInfo PlayAudioInSource(AudioSource source, string bankInfo)
        {
            var info = GetInfoOf(bankInfo);
            if (info == null) return null;

            source.volume = info.Volume;
            source.loop = info.Loop;
            source.spatialBlend = info.SpacialAudio ? 1 : 0;
            source.clip = info.Clip;
            source.Play();
            return info;
        }

        /// <summary>
        /// 
        /// </summary>
        public AudioInfo GetInfoOf(string bankName)
        {
            return AudioBank.Find(x => x.Name.ToLower() == bankName.ToLower());
        }

        [System.Serializable]
        public class AudioInfo
        {
            public string Name;
            public AudioClip Clip;
            [Range(0, 1)] public float Volume = 0.9f;
            public bool SpacialAudio = false;
            public bool Loop = false;
        }
    }
}