using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Runtime.AI
{
    [CreateAssetMenu(fileName = "AI Soldier Settings", menuName = "MFPS/AI/Soldier Settings")]
    public class bl_AISoldierSettings : ScriptableObject
    {
        [Header("Speeds")]
        public float walkSpeed = 4;
        public float runSpeed = 8;
        public float crounchSpeed = 2;
        public float rotationSmoothing = 6.0f;

        [Header("Ranges")]
        public float closeRange = 10.0f;
        public float mediumRange = 25.0f;
        public float farRange = 20f;
        public float limitRange = 50f;
    }
}