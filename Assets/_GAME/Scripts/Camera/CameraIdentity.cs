using System;
using UnityEngine;
using System.Collections;

namespace _GAME.Scripts.Camera
{
    public class CameraIdentity : MonoBehaviour
    {
        private UnityEngine.Camera _camera;

        private void OnEnable()
        {
            StartCoroutine(Set());
        }

        private void OnDisable()
        {
            StartCoroutine(Set());
        }

        IEnumerator Set()
        {
            yield return new WaitForSeconds(0.5f);
            if (_camera == null)
            {
                _camera = GetComponent<UnityEngine.Camera>();
            }
            else
            {
                
            }
        }

        
    }
    
    
}
