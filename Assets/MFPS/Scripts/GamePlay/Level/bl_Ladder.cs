using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPS.Runtime.Level
{
    public class bl_Ladder : MonoBehaviour
    {

        public enum LadderStatus
        {
            None,
            Attaching,
            Climbing,
            Detaching,
        }

        [LovattoToogle] public bool hideWeapons = true;
        [Tooltip("Climbing position offset (x,z) relative to the ladder transform.")]
        public Vector3 climbOffset;
        [Tooltip("Direction where the player will look at when climbing, indicated by the yellow arrow gizmo.")]
        public Vector3 lookDirection = new Vector3(0, -1, 0);

        [Header("References")]
        [SerializeField] private BoxCollider TopCollider = null;
        [SerializeField] private BoxCollider BottomCollider = null;

        public LadderStatus Status
        {
            get;
            set;
        } = LadderStatus.None;

        public bool Exiting
        {
            get;
            set;
        } = false;

        private float LastTime = 0;
        private Vector3 topLimit, bottomLimit;
        private bl_PlayerReferences activePlayer;
        const float BOUND_OFFSET = 0.1f;

        /// <summary>
        /// 
        /// </summary>
        public void SetUpBounds(bl_PlayerReferences player)
        {
            activePlayer = player;

            Vector3 offset = GetClimbingOffset();
            offset.y = 0;
            bottomLimit = BottomCollider.transform.position + offset;
            float relativeDis = bl_MathUtility.Distance(bottomLimit, TopCollider.transform.position) - (player.characterController.height * 0.5f);
            topLimit = bottomLimit + transform.forward * relativeDis;

            Status = LadderStatus.Attaching;

            if (hideWeapons)
            {
                player.gunManager.BlockAllWeapons();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void WatchLimits()
        {
            if (activePlayer == null || Status != LadderStatus.Climbing) return;

            Vector3 playerPos = activePlayer.LocalPosition;

            if(playerPos.y > topLimit.y)
            {
                DetachPlayer();
            }

            if(playerPos.y < bottomLimit.y)
            {
                DetachPlayer();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void DetachPlayer()
        {
            Status = LadderStatus.Detaching;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public Vector3 GetAttachPosition(Collider trigger, float playerHeight)
        {
            Vector3 basePos = BottomCollider.transform.position;
            float offset = BOUND_OFFSET;
            if (IsTopTrigger(trigger))
            {
                // we use the same offset from the bottom collider, but we add the top collider y position
                float relativeDis = bl_MathUtility.Distance(basePos, TopCollider.transform.position) - (playerHeight * 0.75f);
                basePos = basePos + transform.forward * relativeDis;
                offset = -0.55f;
            }

            basePos = basePos - GetClimbingOffset();
            basePos.y += offset;
            attpos = basePos;
            return basePos;
        }
        Vector3 attpos;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNearestExitPosition(Transform player)
        {
            Vector3 playerPos = player.position;
            float topDis = bl_MathUtility.Distance(playerPos, TopCollider.transform.position);
            float bottomDis = bl_MathUtility.Distance(playerPos, BottomCollider.transform.position);

            if (topDis < bottomDis) return TopCollider.transform.position;

            return BottomCollider.transform.position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 GetLookDirection()
        {
            return transform.TransformDirection(lookDirection);
        }

        /// <summary>
        /// 
        /// </summary>
        public void JumpOut()
        {
            LastTime = Time.time;
            Status = LadderStatus.None;
            Exiting = false;
            if (activePlayer != null)
            {
                if (hideWeapons) { activePlayer.gunManager.ReleaseWeapons(false); }
            }
            activePlayer = null;
        }

        private Vector3 GetClimbingOffset() => transform.InverseTransformDirection(climbOffset);

        public bool IsBottomTrigger(Collider col) => BottomCollider == col;
        public bool IsTopTrigger(Collider col) => TopCollider == col;

        public bool CanUse
        {
            get
            {
                return ((Time.time - LastTime) > 1.5f);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmos()
        {                     
            var matrix = Gizmos.matrix;
            Gizmos.color = Color.yellow;
            if (BottomCollider != null)
            {
                 Gizmos.matrix = BottomCollider.transform.localToWorldMatrix;
                 Gizmos.DrawWireCube(BottomCollider.center, BottomCollider.size);

                 Gizmos.color = Color.red;
                 //Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.2f);

                 Gizmos.matrix = matrix;
#if UNITY_EDITOR
                Handles.color = Color.red;
                Handles.RectangleHandleCap(0, BottomCollider.transform.position, transform.rotation, 0.2f, EventType.Repaint);
                Handles.color = Color.white;
#endif
            }
            Gizmos.DrawCube(attpos, Vector3.one * 0.2f);
            Gizmos.color = Color.yellow;
            if (TopCollider != null)
            {
                Gizmos.matrix = TopCollider.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(TopCollider.center, TopCollider.size);

                Gizmos.color = Color.red;

                Gizmos.matrix = matrix;

#if UNITY_EDITOR
                Handles.color = Color.red;
                Handles.RectangleHandleCap(0, TopCollider.transform.position, transform.rotation, 0.2f, EventType.Repaint);
                Handles.color = Color.white;
#endif
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (TopCollider == null || BottomCollider == null) return;

            Gizmos.color = Color.yellow;

            Vector3 bottomPos = BottomCollider.transform.position;
            bottomPos -= GetClimbingOffset();
            float relativeDis = (bl_MathUtility.Distance(bottomPos, TopCollider.transform.position) - 1.5f);
            Vector3 upDir = transform.forward * relativeDis;
            Gizmos.DrawRay(bottomPos, upDir);

            Vector3 upPos = bottomPos + upDir;
            Vector3 middle = Vector3.Lerp(bottomPos, upPos, 0.5f);

#if UNITY_EDITOR
            Handles.color = Color.yellow;
            Handles.ArrowHandleCap(0, middle, Quaternion.LookRotation(transform.TransformDirection(lookDirection)), 1, EventType.Repaint);
            Handles.color = Color.white;
#endif

            Gizmos.color = Color.white;
        }
    }
}