using UnityEngine;

namespace MFPS.Runtime.Level
{
    public class bl_BulletDecal : bl_BulletDecalBase
    {
        public Renderer meshRender;

        private Transform defaultParent;
        private Transform pendingParent;

        private Transform _transform;
        public Transform Transform
        {
            get
            {
                if (_transform == null) _transform = transform;
                return _transform;
            }
        }

        private GameObject _gameObject;
        public GameObject ThisGameObject
        {
            get
            {
                if (_gameObject == null) _gameObject = gameObject;
                return _gameObject;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bl_BulletDecalBase Init(Transform parent)
        {
            defaultParent = parent;
            Transform.parent = defaultParent;
            ThisGameObject.SetActive(false);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public override bl_BulletDecalBase SetDecalMaterial(Material mat)
        {
            if (ThisGameObject != null && !ThisGameObject.activeSelf) ThisGameObject.SetActive(true);

            // .sharedMaterial since .material create a new material instance for each decal.
            meshRender.sharedMaterial = mat;
            return this;
        }

        /// <summary>
        /// Translate and Rotate to the raycast hit point
        /// </summary>
        /// <returns></returns>
        public override bl_BulletDecalBase SetToHit(RaycastHit hit, bool asPendingParent = false)
        {
            Transform.position = hit.point;
            Transform.rotation = Quaternion.LookRotation(-hit.normal);

            if (asPendingParent) pendingParent = hit.transform;
            else Transform.parent = hit.transform;

            return this;
        }

        /// <summary>
        /// Randomize the decal size
        /// </summary>
        /// <returns></returns>
        public override bl_BulletDecalBase SetScaleVariation(Vector3 scaleBase, Vector2 Range)
        {
            if(pendingParent != null) Transform.parent = null;
            Transform.localScale = scaleBase * Random.Range(Range.x, Range.y);
            if (pendingParent != null)
            {
                Transform.SetParent(pendingParent);
                pendingParent = null;
            }

            return this;
        }

        /// <summary>
        /// Disable and set to the default parent
        /// </summary>
        public override void BackToOrigin()
        {
            Transform.parent = defaultParent;
            ThisGameObject.SetActive(false);
        }
    }
}