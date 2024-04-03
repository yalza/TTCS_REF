using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Runtime.AI
{
    [CreateAssetMenu(fileName = "AI Behavior Settings", menuName = "MFPS/AI/Behavior Settings")]
    public class bl_AIBehaviorSettings : ScriptableObject
    {
        [Header("Settings")]
        public AIAgentBehave agentBehave = AIAgentBehave.Agressive;
        public AIWeaponAccuracy weaponAccuracy = AIWeaponAccuracy.Casual;
        [LovattoToogle] public bool GetRandomTargetOnStart = true;
        [LovattoToogle] public bool forceFollowAtHalfHealth = true;
        [LovattoToogle] public bool checkEnemysWhenHaveATarget = true;
        public AITargetOutRangeBehave targetOutRangeBehave = AITargetOutRangeBehave.KeepFollowingBasedOnState;

        [Header("Cover")]
        public float maxCoverTime = 10;
        public float coverColdDown = 15;
        [Tooltip("probability of get a cover point as random destination")]
        [Range(0, 1)] public float randomCoverProbability = 0.1f;
    }
}