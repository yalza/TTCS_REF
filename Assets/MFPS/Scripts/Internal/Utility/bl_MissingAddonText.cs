using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Internal
{
    public class bl_MissingAddonText : MonoBehaviour
    {
        public string url;
        public int activeIfChilds = 1;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            var parent = transform.parent;
            if (parent == null) return;

            if (activeIfChilds > 0)
            {
                gameObject.SetActive(parent.childCount == activeIfChilds);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnClick()
        {
            if (string.IsNullOrEmpty(url)) return;

            Application.OpenURL(url);
        }
    }
}