using UnityEngine;

namespace MFPS.Runtime.Level
{
    public abstract class bl_BulletDecalBase : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public abstract bl_BulletDecalBase Init(Transform parent);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public abstract bl_BulletDecalBase SetDecalMaterial(Material mat);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="asPendingParent"></param>
        /// <returns></returns>
        public abstract bl_BulletDecalBase SetToHit(RaycastHit hit, bool asPendingParent = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scaleBase"></param>
        /// <param name="Range"></param>
        /// <returns></returns>
        public abstract bl_BulletDecalBase SetScaleVariation(Vector3 scaleBase, Vector2 Range);

        /// <summary>
        /// 
        /// </summary>
        public abstract void BackToOrigin();
    }
}