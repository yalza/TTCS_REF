using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Runtime.UI
{
    public class bl_EmptyPanelText : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            Check();
        }

        private void OnTransformParentChanged()
        {
            Debug.Log("change");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Check()
        {
            var childs = transform.parent.childCount;
            if(childs <= 1)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}