using UnityEngine;
using UnityEngine.SceneManagement;

namespace MFPS.Internal.BaseClass
{
    public abstract class bl_SceneLoaderBase : ScriptableObject
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single);

        /// <summary>
        /// 
        /// </summary>
        public abstract void LoadScene(int sceneID, LoadSceneMode loadSceneMode = LoadSceneMode.Single);

        /// <summary>
        /// 
        /// </summary>
        private static bl_SceneLoaderBase _instance;
        public static bl_SceneLoaderBase Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_SceneLoaderBase>(); }
                return _instance;
            }
        }
    }
}