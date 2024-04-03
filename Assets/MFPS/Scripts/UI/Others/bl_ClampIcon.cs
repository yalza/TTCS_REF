using UnityEngine;

namespace MFPS.Runtime.UI
{
    public class bl_ClampIcon : MonoBehaviour
    {
        public bool isStatic = true;
        public bool isPooled = false;
        public Texture2D Icon;
        public Vector3 Offset;
        public float size = 50;
        public float timeToLive = 10;
        public Color m_Color = Color.white;

        private Transform ThisTransform;
        Vector3 viewportPoint;
        Vector2 drawPosition;
        float clampBorder = 12;
        bool visible = true;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            ThisTransform = transform;
            visible = IsVisible(ThisTransform.position + Offset);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            ThisTransform = transform;
            if (isPooled) Invoke(nameof(Disable), timeToLive);
        }

        /// <summary>
        /// 
        /// </summary>
        void OnGUI()
        {
            if (isStatic)
            {
                if (!bl_RoomMenu.Instance.isCursorLocked)
                    return;
            }
            if (bl_GameManager.Instance.CameraRendered == null)
                return;

            Vector3 position = ThisTransform.position + Offset;
            if (Time.frameCount % 30 == 0)
            {
                visible = IsVisible(position);
            }

            if (!visible) return;

            //Calculate the 2D position of the position where the icon should be drawn
            viewportPoint = bl_GameManager.Instance.CameraRendered.WorldToViewportPoint(position);

            //The viewportPoint coordinates are between 0 and 1, so we have to convert them into screen space here
            drawPosition = new Vector2(viewportPoint.x * Screen.width, Screen.height * (1 - viewportPoint.y));

            //Clamp the position to the edge of the screen in case the icon would be drawn outside the screen
            drawPosition.x = Mathf.Clamp(drawPosition.x, clampBorder, Screen.width - clampBorder);
            drawPosition.y = Mathf.Clamp(drawPosition.y, clampBorder, Screen.height - clampBorder);

            GUI.color = m_Color;
            GUI.DrawTexture(new Rect(drawPosition.x - size * 0.5f, drawPosition.y - size * 0.5f, size, size), Icon);
            GUI.color = Color.white;
        }

        private bool IsVisible(Vector3 fromPos)
        {
            if (bl_GameManager.Instance.CameraRendered == null) return false;

            Plane plane = new Plane(bl_GameManager.Instance.CameraRendered.transform.forward, bl_GameManager.Instance.CameraRendered.transform.position);
            return plane.GetSide(fromPos);
        }

        /// <summary>
        /// 
        /// </summary>
        void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}