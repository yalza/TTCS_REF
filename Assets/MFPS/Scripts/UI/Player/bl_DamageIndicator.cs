using UnityEngine;

namespace MFPS.Runtime.UI
{
    public class bl_DamageIndicator : bl_DamageIndicatorBase
    {
        #region Public members
        [Range(1, 5)] public float FadeTime = 3;
        [Header("References")]
        public RectTransform indicatorPivot;
        public CanvasGroup indicatorAlpha; 
        #endregion

        #region Private members
        private float alpha = 0.0f;
        private float rotationOffset;
        Vector3 eulerAngle = Vector3.zero;
        Vector3 forward;
        Vector3 rhs;
        private Vector3 attackDirection; 
        #endregion

        /// <summary>
        /// 
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            enabled = bl_GameData.Instance.showDamageIndicator;
            bl_EventHandler.onLocalPlayerSpawn += OnLocalSpawn;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            bl_EventHandler.onLocalPlayerSpawn -= OnLocalSpawn;
            if (indicatorAlpha != null)
                indicatorAlpha.alpha = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalSpawn()
        {
            indicatorAlpha.alpha = 0;
            alpha = 0;
        }

        /// <summary>
        /// Use this to send a new direction of attack
        /// </summary>
        public override void SetHit(Vector3 direction)
        {
            if (direction == Vector3.zero)
                return;

            attackDirection = direction;
            alpha = 3f;
        }

        /// <summary>
        /// if this is visible Update position
        /// </summary>
        public override void OnUpdate()
        {
            if (alpha <= 0) return;
            if (bl_MFPS.LocalPlayerReferences == null) return;

            alpha -= Time.deltaTime;
            UpdateDirection();
        }

        /// <summary>
        /// update direction as the arrow shows
        /// </summary>
        void UpdateDirection()
        {
            rhs = attackDirection - bl_MFPS.LocalPlayerReferences.PlayerCameraTransform.position;
            rhs.y = 0;
            rhs.Normalize();
            if (bl_GameManager.Instance.CameraRendered != null)
            {
                forward = bl_GameManager.Instance.CameraRendered.transform.forward;
            }
            else
            {
                forward = transform.forward;
            }
            float GetPos = Vector3.Dot(forward, rhs);
            if (Vector3.Cross(forward, rhs).y > 0)
            {
                rotationOffset = (1f - GetPos) * 90;
            }
            else
            {
                rotationOffset = (1f - GetPos) * -90;
            }
            if (indicatorPivot != null)
            {
                indicatorAlpha.alpha = alpha;
                eulerAngle.z = -rotationOffset;
                indicatorPivot.eulerAngles = eulerAngle;
            }
        }
    }
}