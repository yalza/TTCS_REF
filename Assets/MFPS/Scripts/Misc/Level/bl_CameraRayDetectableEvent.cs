using UnityEngine;

namespace MFPS.Runtime.Level
{
    /// <summary>
    /// This script invoke OnDetected or UnDetected events when the local player is looking at the object
    /// A collider is required and the local player bl_CameraRay.Checking have to be true.
    /// Check bl_CameraRayActiveTrigger.cs
    /// 
    /// How it works?
    /// 
    /// For optimization reasons, the player camera is not detecting (RayCast) every frame.
    /// First you have to let it know when start checking (Firing raycasts)
    /// In order to do that, you have to set the 'Checking' bool of the bl_CameraRay of the local player to = true;
    /// You can do this with a trigger collider that represent the area where the player should start detecting when the player enter in the trigger and 
    /// stop when exit it, check bl_CameraRayActiveTrigger.cs for this implementation,
    /// </summary>
    public class bl_CameraRayDetectableEvent : MonoBehaviour, IRayDetectable
    {
        public bl_EventHandler.UEvent onDetected;
        public bl_EventHandler.UEvent onUndetected;

        /// <summary>
        /// 
        /// </summary>
        public void OnRayDetectedByPlayer()
        {
            onDetected?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnUnDetectedByPlayer()
        {
            onUndetected?.Invoke();
        }
    }
}