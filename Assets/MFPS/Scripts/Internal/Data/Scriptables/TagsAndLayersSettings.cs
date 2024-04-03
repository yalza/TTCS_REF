using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "TagsAndLayerSettings", menuName = "MFPS/Settings/TagsAndLayers")]
    public class TagsAndLayersSettings : ScriptableObject
    {
        public LayerMask LocalPlayerHitableLayers;
        public LayerMask RemotePlayerRootLayer;
        public LayerMask LocalPlayerRootLayer;
        public LayerMask EnvironmentOnly;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetLocalPlayerLayerIndex()
        {
            return (int)Mathf.Log(LocalPlayerRootLayer, 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static int GetLayerMaskIndex(LayerMask mask)
        {
            return (int)Mathf.Log(mask.value, 2);
        }
    }
}