using System;
using UnityEngine;

namespace MFPS.Internal
{
    public class bl_AnimatorReloadEvent : StateMachineBehaviour
    {
        public static Action<bool, Animator, AnimatorStateInfo> OnTPReload;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (layerIndex != 1) return;
            OnTPReload?.Invoke(true, animator, stateInfo);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (layerIndex != 1) return;
            OnTPReload?.Invoke(false, animator, stateInfo);
        }
    }
}