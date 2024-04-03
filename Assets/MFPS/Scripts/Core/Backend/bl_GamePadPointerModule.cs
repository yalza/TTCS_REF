using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace MFPS.InputManager
{
    [RequireComponent(typeof(EventSystem))]
    public class bl_GamePadPointerModule : PointerInputModule
    {
        public Canvas m_Canvas;
        public KeyCode submitButton = KeyCode.JoystickButton0;

        private Vector2 auxVec2 = Vector2.zero;
        private Vector2 dragLockedPointerPos = Vector2.zero;
        private Vector3 screenPos = Vector3.zero;
        private Vector3 draggedObjectPos = Vector3.zero;

        private bool cursorLocked = true;
        private float lastClickTime = 0;

        [SerializeField]
        private float multipleClicksTimeout = 0.5f;
        [SerializeField]
        private float maxInteractDistance = 1.5f;

        private PointerEventData pointer;
        private RaycastResult raycastResult;
        private new EventSystem eventSystem = null;
        public GameObject defaultModule;

        /// <summary>
        /// 
        /// </summary>
        protected override void Start()
        {
            base.Start();
            eventSystem = GetComponent<EventSystem>();
            CheckImplementation();
        }

        /// <summary>
        /// 
        /// </summary>
        void CheckImplementation()
        {
            if (bl_InputData.Instance.useGamePadNavigation)
            {
                StandaloneInputModule sim = FindObjectOfType<StandaloneInputModule>();
                if (sim)
                {
                    defaultModule = sim.gameObject;
                    sim.gameObject.SetActive(false);
                    Debug.Log("Disabled StandaloneInputModule since GamePadPointerModule is used.");
                }
            }
            else
            {
                gameObject.SetActive(false);
                if (bl_GamePadPointer.Instance != null) { bl_GamePadPointer.Instance.gameObject.SetActive(false); }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (defaultModule != null) defaultModule.SetActive(true);
        }

        private bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;

            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }

        protected override void ProcessDrag(PointerEventData pointerEvent)
        {
            if (pointerEvent.pointerDrag == null)
                return;

            if (!pointerEvent.dragging
                && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
            {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }

            if (pointerEvent.dragging)
            {
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }
        }

        protected override void ProcessMove(PointerEventData pointerEvent)
        {
            var targetGO = pointerEvent.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(pointerEvent, targetGO);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Process()
        {
            if (bl_GamePadPointer.Instance == null) return;

            GetPointerData(0, out pointer, true);

            if (Cursor.lockState == CursorLockMode.None || Cursor.lockState == CursorLockMode.Confined)
            {
                cursorLocked = false;
            }

            if (!cursorLocked && Cursor.lockState == CursorLockMode.Locked)
            {
                cursorLocked = true;
                if (m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    screenPos = bl_GamePadPointer.Instance.Position;
                }
                else
                {
                    screenPos = m_Canvas.worldCamera.WorldToScreenPoint(bl_GamePadPointer.Instance.WorldPosition);
                }
                auxVec2.x = screenPos.x;
                auxVec2.y = screenPos.y;
            }

            if (cursorLocked && pointer.pointerDrag)
            {
                if (m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    draggedObjectPos = pointer.pointerDrag.GetComponent<RectTransform>().anchoredPosition;
                }
                else
                {
                    draggedObjectPos = m_Canvas.worldCamera.WorldToScreenPoint(pointer.pointerDrag.transform.position);
                }
                dragLockedPointerPos = new Vector2(auxVec2.x - draggedObjectPos.x, auxVec2.y - draggedObjectPos.y);
                pointer.position = auxVec2 + dragLockedPointerPos;
            }
            else
            {
                if (m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    screenPos = bl_GamePadPointer.Instance.Position;
                }
                else
                {
                    screenPos = m_Canvas.worldCamera.WorldToScreenPoint(bl_GamePadPointer.Instance.WorldPosition);
                }
                auxVec2.x = screenPos.x;
                auxVec2.y = screenPos.y;

                pointer.position = auxVec2;
            }

            eventSystem.RaycastAll(pointer, this.m_RaycastResultCache);
            raycastResult = FindFirstRaycast(this.m_RaycastResultCache);
            pointer.pointerCurrentRaycast = raycastResult;

            this.ProcessDrag(pointer);
            this.ProcessMove(pointer);

            if (raycastResult.distance > maxInteractDistance)
            {
                //Debug.Log("Distance too great");
                // return;
            }

            if (Input.GetKeyDown(submitButton))
            {
                pointer.pressPosition = auxVec2;
                pointer.clickTime = Time.unscaledTime;
                pointer.pointerPressRaycast = raycastResult;
                pointer.eligibleForClick = true;
                float timeBetweenClicks = pointer.clickTime - lastClickTime;
                if (timeBetweenClicks > multipleClicksTimeout)
                {
                    pointer.clickCount = 0;
                }

                pointer.clickCount++;
                lastClickTime = Time.unscaledTime;

                if (this.m_RaycastResultCache.Count > 0)
                {
                    pointer.selectedObject = raycastResult.gameObject;
                    pointer.pointerPress = raycastResult.gameObject;
                    pointer.rawPointerPress = raycastResult.gameObject;
                    pointer.pointerDrag = ExecuteEvents.ExecuteHierarchy(raycastResult.gameObject, pointer, ExecuteEvents.pointerDownHandler);

                    dragLockedPointerPos = pointer.position;
                }
                else
                {
                    pointer.selectedObject = null;
                    pointer.pointerPress = null;
                    pointer.rawPointerPress = null;
                }
            }
            else if (Input.GetKeyUp(submitButton))
            {
                pointer.pointerPress = ExecuteEvents.ExecuteHierarchy(raycastResult.gameObject, pointer, ExecuteEvents.submitHandler);

                pointer.pointerPress = null;
                pointer.rawPointerPress = null;
                pointer.pointerDrag = null;
                pointer.dragging = false;
                pointer.eligibleForClick = false;
            }
        }

        public void CheckCanvas()
        {
            if (!gameObject.activeInHierarchy) return;
            if (m_Canvas != null) return;

            Canvas[] all = FindObjectsOfType<Canvas>();
            if (all.Length <= 0) return;
            m_Canvas = all[0];
        }

        private static bl_GamePadPointerModule m_instance;
        public static bl_GamePadPointerModule Instance
        {
            get
            {
                if (m_instance == null) { m_instance = FindObjectOfType<bl_GamePadPointerModule>(); }
                return m_instance;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            CheckCanvas();
        }
#endif
    }
}