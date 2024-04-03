using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MFPS.Runtime.UI
{
    public class bl_UIInputAxis : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public bool isDown { get; set; } = false;
        public Vector2 Direction { get; set; } = Vector2.zero;
        private Vector2 lastDirection;
        private bool wasDragged = false;

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDown = true;
            lastDirection = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Direction = (eventData.position - lastDirection).normalized;
            lastDirection = eventData.position;
            wasDragged = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDown = false;
            Direction = Vector2.zero;
        }

        void LateUpdate()
        {
            if (wasDragged) Direction = Vector2.zero;
        }
    }
}