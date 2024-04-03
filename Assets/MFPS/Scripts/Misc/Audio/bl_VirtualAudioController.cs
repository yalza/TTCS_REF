using System.Collections.Generic;
using UnityEngine;
using MFPSEditor;

namespace MFPS.Audio
{
    [System.Serializable]
    public class bl_VirtualAudioController
    {

        [ScriptableDrawer] public bl_AudioBank audioBank;
        public int numberOfAudioInstances = 10;

        private List<bl_AudioInstance> audioInstances = new List<bl_AudioInstance>();

        private bool isInit = false;
        private int currentInstance = 0;

        /// <summary>
        /// 
        /// </summary>
        public void Initialized(MonoBehaviour parent)
        {
            if (isInit || audioBank == null) return;

            GameObject g = new GameObject("Audio List");
            Transform root = g.transform;
            root.parent = parent.transform;
            root.localPosition = Vector3.zero;
            root.localEulerAngles = Vector3.zero;

            g = new GameObject("Audio Instance");
            g.transform.parent = root;
            bl_AudioInstance ai = g.AddComponent<bl_AudioInstance>();
            ai.Init();
            audioInstances.Add(ai);
            g.SetActive(false);
            ai.ID = 0;

            var template = g;

            for (int i = 1; i < numberOfAudioInstances; i++)
            {
                g = GameObject.Instantiate(template) as GameObject;
                g.transform.parent = root;
                ai = g.AddComponent<bl_AudioInstance>();
                ai.ID = i;
                ai.Init();
                g.SetActive(false);
                audioInstances.Add(ai);
            }
            isInit = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audioName"></param>
        /// <returns></returns>
        public bl_AudioInstance PlayClip(string audioName)
        {
            if (audioBank == null || audioBank.AudioBank == null) return null;

            int index = audioBank.AudioBank.FindIndex(x => x.Name.ToLower() == audioName.ToLower());
            if (index < 0) { Debug.LogWarning("Clip: " + audioName + " not found"); return null; }
            return PlayClip(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bl_AudioInstance PlayClip(int id)
        {
            if (audioBank == null || audioBank.AudioBank == null) return null;

            bl_AudioBank.AudioInfo info = audioBank.AudioBank[id];
            bl_AudioInstance ai = audioInstances[currentInstance];
            if (ai.isPlaying)
            {
                ai = null;
                for (int i = 0; i < audioInstances.Count; i++)
                {
                    if (!audioInstances[i].isPlaying) { ai = audioInstances[i]; currentInstance = i; break; }
                }
            }
            if (ai == null) { ai = CreateNewInstance(); }
            ai.gameObject.SetActive(true);
            ai.SetInstance(info);
            return ai;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audioName"></param>
        public void StopClip(string audioName)
        {
            for (int i = 0; i < audioInstances.Count; i++)
            {
                if (audioInstances[i].isPlayingClip(audioName))
                {
                    audioInstances[i].Stop();
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bl_AudioBank.AudioInfo GetAudioInfo(string name)
        {
            if (audioBank == null) return null;
            return audioBank.AudioBank.Find(x => x.Name == name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexID"></param>
        /// <returns></returns>
        public bl_AudioBank.AudioInfo GetAudioInfo(int indexID)
        {
            if (audioBank == null) return null;
            return audioBank.AudioBank[indexID];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bl_AudioInstance CreateNewInstance()
        {
            if (!isInit) { return null; }

            GameObject g = GameObject.Instantiate(audioInstances[0].gameObject) as GameObject;
            g.transform.parent = audioInstances[0].transform.parent;
            bl_AudioInstance ai = g.GetComponent<bl_AudioInstance>();
            audioInstances.Add(ai);
            ai.Init();
            ai.ID = audioInstances.Count;
            return ai;
        }
    }
}