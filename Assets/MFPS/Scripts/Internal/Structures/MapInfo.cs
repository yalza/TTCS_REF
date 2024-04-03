using System;
using System.Collections.Generic;
using UnityEngine;
using MFPSEditor;
using Object = UnityEngine.Object;

namespace MFPS.Internal.Structures
{
    [Serializable]
    public class MapInfo
    {
        public string ShowName;
        [SerializeField]
        public Object m_Scene;
        [HideInInspector] public string RealSceneName;
        [SpritePreview] public Sprite Preview;
        public List<GameMode> NoAllowedGameModes = new List<GameMode>();

        /// <summary>
        /// Return only the allowed game modes in this map from the given modes list.
        /// </summary>
        /// <returns></returns>
        public GameModeSettings[] GetAllowedGameModes(GameModeSettings[] allAvailables)
        {
            var list = new List<GameModeSettings>();
            for (int i = 0; i < allAvailables.Length; i++)
            {
                if (!NoAllowedGameModes.Contains(allAvailables[i].gameMode))
                {
                    list.Add(allAvailables[i]);
                }
            }
            return list.ToArray();
        }
    }
}