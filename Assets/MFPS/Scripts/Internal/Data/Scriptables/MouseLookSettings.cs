using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "Mouse Settings", menuName = "MFPS/Camera/Mouse Settings")]
    public class MouseLookSettings : ScriptableObject
    {
        [LovattoToogle] public bool useSmoothing = true;
        [Range(2, 12)] public float framesOfSmoothing = 5f;
        [LovattoToogle] public bool lerpMovement = false;
        [Range(2,12)] public float smoothTime = 5f;
        [Tooltip("Relative: To the player camera field of view\nFixed: to a fixed value")]
        public AimSensitivityAdjust aimSensitivityAdjust = AimSensitivityAdjust.Relative;
        public Texture2D customCursor;

        [System.Serializable]
        public enum AimSensitivityAdjust
        {
            Relative,
            Fixed
        }
    }
}