using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Runtime.Level
{
    [RequireComponent(typeof(AudioSource))]
    public class bl_JumpPlatform : MonoBehaviour
    {
        [Range(0, 25)] public float JumpForce;
        [SerializeField] private AudioClip JumpSound;

        private void OnTriggerEnter(Collider other)
        {
            if (other.isLocalPlayerCollider())
            {
                var fpc = other.GetComponent<bl_FirstPersonControllerBase>();
                fpc.PlatformJump(JumpForce);
                if (JumpSound != null) { AudioSource.PlayClipAtPoint(JumpSound, transform.position); }
            }
        }
    }
}