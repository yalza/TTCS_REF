using UnityEngine;

namespace MFPS.Runtime.Level
{
    /// <summary>
    /// This scripts active / deactivated the local player camera ray detection
    /// when the player enter / exit the trigger collider.
    /// 
    /// How it works?
    /// 
    /// For optimization reasons, the player camera is not detecting (RayCast) every frame.
    /// First you have to let it know when start checking (Firing raycasts)
    /// In order to do that, you have to set the 'Checking' bool of the bl_CameraRay of the local player to = true;
    /// You can do this with a trigger collider that represent the area where the player should start detecting when the player enter in the trigger and 
    /// stop when exit it, check bl_CameraRayActiveTrigger.cs for this implementation,
    /// </summary>
    public class bl_CameraRayActiveTrigger : MonoBehaviour
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (!other.isLocalPlayerCollider()) return;

            SetDetectionActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            if (!other.isLocalPlayerCollider()) return;

            SetDetectionActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetDetectionActive(bool active)
        {
            if (bl_MFPS.LocalPlayerReferences == null) return;

            bl_MFPS.LocalPlayerReferences.cameraRay.IsCurrentlyDetecting = active;
        }
    }
}