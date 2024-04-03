using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Audio
{
    public class bl_AudioInstance : MonoBehaviour
    {
        private AudioSource aSource;
        public int ID { get; set; }
        public bl_AudioBank.AudioInfo playingAudio { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            if (aSource == null) { aSource = gameObject.AddComponent<AudioSource>(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public int SetInstance(bl_AudioBank.AudioInfo info)
        {
            if (aSource == null) { aSource = gameObject.AddComponent<AudioSource>(); }

            aSource.clip = info.Clip;
            aSource.volume = info.Volume;
            aSource.spatialBlend = info.SpacialAudio ? 1 : 0;
            aSource.loop = info.Loop;

            aSource.Play();
            playingAudio = info;
            if (!info.Loop)
            {
                Invoke("Stop", info.Clip.length);
            }
            return ID;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            aSource.Stop();
            gameObject.SetActive(false);
        }

        public bool isPlaying
        {
            get
            {
                if (aSource == null) return false;
                
                return aSource.isPlaying;
            }
        }
        public bool isPlayingClip(string audioName) { return (isPlaying && (playingAudio.Name.ToLower() == audioName.ToLower())); }
    }
}