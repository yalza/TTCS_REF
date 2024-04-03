using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MFPS.InputManager
{
    public class bl_GamePadPointer : MonoBehaviour
    {
        public float Resposiveness = 2;
        public int smoothing = 2;
        public GameObject UIObject;

        private RectTransform rectTransform;
        private GraphicRaycaster graphicRaycaster;
        private int iteration = 0;
        float[] horizontalTraces;
        float[] verticalTraces;
        private bool isActive = true;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
            rectTransform.SetAsLastSibling();

            horizontalTraces = new float[smoothing];
            verticalTraces = new float[smoothing];
            isActive = gameObject.activeInHierarchy;
        }

        public void SetActive(bool active)
        {
            isActive = active;
            UIObject.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            if (!isActive) return;

            PadControlled();
        }

        /// <summary>
        /// 
        /// </summary>
        void PadControlled()
        {
            horizontalTraces = new float[smoothing];
            verticalTraces = new float[smoothing];
            float xaggregate = 0;
            float yaggregate = 0;
            float v = Input.GetAxisRaw("Mouse Y");
            float h = Input.GetAxisRaw("Mouse X");

            verticalTraces[iteration % smoothing] = v;
            horizontalTraces[iteration % smoothing] = h;
            iteration++;

            foreach (float xmov in horizontalTraces)
            {
                xaggregate += xmov;
            }
            xaggregate = xaggregate / smoothing * Resposiveness;
            foreach (float ymov in verticalTraces)
            {
                yaggregate += ymov;
            }
            yaggregate = yaggregate / smoothing * Resposiveness;

            Vector2 position = rectTransform.anchoredPosition;
            position.y += yaggregate;
            position.x += xaggregate;
            rectTransform.anchoredPosition = position;
        }

        public Vector3 Position => rectTransform.position;
        public Vector3 WorldPosition => rectTransform.position;

        private static bl_GamePadPointer m_instance;
        public static bl_GamePadPointer Instance
        {
            get
            {
                if (m_instance == null) { m_instance = FindObjectOfType<bl_GamePadPointer>(); }
                return m_instance;
            }
        }
    }
}