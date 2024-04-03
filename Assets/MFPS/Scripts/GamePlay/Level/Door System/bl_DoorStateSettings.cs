using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "Door Setting", menuName = "MFPS/Level/Door Setting")]
    public class bl_DoorStateSettings : ScriptableObject
    {
        public float TransitionDuration = 1;
        [Space]
        public Vector3 CloseRotation;
        public Vector3 OpenInsideRotation;
        public Vector3 OpenOutsideRotation;
        [Space]
        public AudioClip OpenSound;
        public AudioClip CloseSound;
        [Space]
        public AnimationCurve TransitionEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
}