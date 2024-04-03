using UnityEngine;
using UnityEngine.UI;
using UGUIMiniMap;
using Photon.Pun;

public class bl_MiniMapItem : bl_MonoBehaviour
{

    [Separator("TARGET")]
    [Tooltip("Transform to UI Icon will be follow")]
    public Transform Target = null;
    [Tooltip("Custom Position from target position")]
    public Vector3 OffSet = Vector3.zero;

    [Separator("ICON")]
    public Sprite Icon = null;
    public Sprite DeathIcon = null;
    public Color IconColor = new Color(1, 1, 1, 0.9f);
    [Range(1, 100)] public float Size = 20;

    [System.Serializable]
    public enum IconType
    {
        General,
        Player,
        Bot,
    }
    public IconType m_IconType = IconType.General;

    [Separator("CIRCLE AREA")]
    public bool ShowCircleArea = false;
    [Range(1, 100)] public float CircleAreaRadius = 10;
    public Color CircleAreaColor = new Color(1, 1, 1, 0.9f);

    [Separator("ICON BUTTON")]
    [Tooltip("UI can interact when press it?")]
    [CustomToggle("is Interact-able")]
    public bool isInteractable = true;
    [TextArea(2, 2)] public string InfoItem = "Info Icon here";

    [Separator("SETTINGS")]
    [Tooltip("Can Icon show when is off screen?")]
    [CustomToggle("Off Screen")]
    public bool OffScreen = true;
    [CustomToggle("Destroy Icon with object")] public bool DestroyWithObject = true;
    [Range(0, 5)] public float BorderOffScreen = 0.01f;
    [Range(1, 50)] public float OffScreenSize = 10;
    [CustomToggle("isHoofdPunt")]
    public bool isHoofdPunt = false;
    [Tooltip("Time before render/show item in minimap after instance")]
    [Range(0, 3)] public float RenderDelay = 0.3f;
    public ItemEffect m_Effect = ItemEffect.None;
    public bool useCustomIconPrefab = false;
    public GameObject CustomIconPrefab;

    //Privates
    private Image Graphic = null;
    private RectTransform GraphicRect;
    private RectTransform RectRoot;
    private GameObject cacheItem = null;
    private RectTransform CircleAreaRect = null;
    private Vector3 position;
    private bl_MiniMap MiniMap;
    Vector3 screenPos = Vector3.zero;
    private bool isVisible = true;

    /// <summary>
    /// Get all required component in start
    /// </summary>
    void Start()
    {
        MiniMap = bl_MiniMapUtils.GetMiniMap();
        if (MiniMap != null)
        {
            CreateIcon();
            MiniMap.RegisterItem(this);
        }
        //else { Debug.Log("You need a MiniMap in scene for use MiniMap Items."); }
    }

    /// <summary>
    /// 
    /// </summary>
    void CreateIcon()
    {
        if (MiniMap.hasError || !this.enabled) return;
        //Instantiate UI in canvas
        GameObject g = bl_MiniMapData.Instance.IconPrefab;
        if (useCustomIconPrefab && CustomIconPrefab != null) { g = CustomIconPrefab; }
        cacheItem = Instantiate(g) as GameObject;
        RectRoot = OffScreen ? bl_MiniMapUtils.GetMiniMap().MiniMapUIRoot : bl_MiniMapUtils.GetMiniMap().IconsParent;
        //SetUp Icon UI
        Graphic = cacheItem.GetComponent<Image>();
        GraphicRect = Graphic.GetComponent<RectTransform>();
        if (m_IconType != IconType.General && bl_GameManager.Instance.FirstSpawnDone)
        {
            if (m_IconType == IconType.Player)
            {
                if (GetComponent<PhotonView>().IsMine) { Destroy(cacheItem); Graphic = null; return; }
                isTeamMate = photonView.Owner.isTeamMate();
                Graphic.sprite = isTeamMate ? bl_MiniMapData.Instance.TeamMateIcon : bl_MiniMapData.Instance.EnemyIcon;
                Graphic.color = isTeamMate ? bl_MiniMapData.Instance.TeammateIconColor : bl_MiniMapData.Instance.EnemyIconColor;
            }
            else
            {
                isTeamMate = GetComponent<bl_AIShooter>().isTeamMate;
                Graphic.sprite = isTeamMate ? bl_MiniMapData.Instance.TeamMateIcon : bl_MiniMapData.Instance.EnemyIcon;
                Graphic.color = isTeamMate ? bl_MiniMapData.Instance.TeammateIconColor : bl_MiniMapData.Instance.EnemyIconColor;
            }
        }
        else
        {
            if (Icon != null) { Graphic.sprite = Icon; Graphic.color = IconColor; }
        }
        cacheItem.transform.SetParent(RectRoot.transform, false);
        GraphicRect.anchoredPosition = Vector2.zero;
        cacheItem.GetComponent<CanvasGroup>().interactable = isInteractable;
        if (Target == null) { Target = this.GetComponent<Transform>(); }
        StartEffect();
        bl_IconItem ii = cacheItem.GetComponent<bl_IconItem>();
        ii.DelayStart(RenderDelay);
        ii.GetInfoItem(InfoItem);
        if (ShowCircleArea)
        {
            CircleAreaRect = ii.SetCircleArea(CircleAreaRadius, CircleAreaColor);
        }
        if (isTeamMate) { ShowItem(); }else { HideItem(); }
    }


    /// <summary>
    /// 
    /// </summary>
    public void OnUpdateItem()
    {
        //If a component missing, return for avoid bugs.
        if (Target == null || MiniMap == null)
            return;
        if (Graphic == null || !isVisible)
            return;

        IconControl();
    }

    /// <summary>
    /// 
    /// </summary>
    void IconControl()
    {
        if (MiniMap == null) return;

        if (isHoofdPunt)
        {
            if (MiniMap.Target != null)
            {
                transform.position = MiniMap.Target.TransformPoint((MiniMap.Target.forward) * 100);
            }
        }
        //Setting the modify position
        Vector3 CorrectPosition = TargetPosition + OffSet;
        Vector2 fullSize = RectRoot.rect.size;
        Vector2 size = RectRoot.rect.size * 0.5f;
        //Convert the position of target in ViewPortPoint
        Vector2 wvp = bl_MiniMap.MiniMapCamera.WorldToViewportPoint(CorrectPosition);
        //Calculate the position of target and convert into position of screen
        position = new Vector2((wvp.x * fullSize.x) - size.x, (wvp.y * fullSize.y) - size.y);

        Vector2 UnClampPosition = position;
        //if show off screen
        if (OffScreen)
        {
            //Calculate the max and min distance to move the UI
            //this clamp in the RectRoot sizeDela for border
            position.x = Mathf.Clamp(position.x, -(size.x - BorderOffScreen), (size.x - BorderOffScreen));
            position.y = Mathf.Clamp(position.y, -size.y - BorderOffScreen, (size.y - BorderOffScreen));
        }

        //calculate the position of UI again, determine if off screen
        //if off screen reduce the size
        float Iconsize = Size;
        //Use this (useCompassRotation when have a circle miniMap)
        if (m_miniMap.useCompassRotation)
        {
            //Calculate difference
            Vector3 forward = Target.position - m_miniMap.TargetPosition;
            //Position of target from camera
            Vector3 cameraRelativeDir = bl_MiniMap.MiniMapCamera.transform.InverseTransformDirection(forward);
            //normalize values for screen fix
            cameraRelativeDir.z = 0;
            cameraRelativeDir = cameraRelativeDir.normalized * 0.5f;
            //Convert values to positive for calculate area OnScreen and OffScreen.
            float posPositiveX = Mathf.Abs(position.x);
            float relativePositiveX = Mathf.Abs((0.5f + (cameraRelativeDir.x * m_miniMap.CompassSize)));
            //when target if offScreen clamp position in circle area.
            if (posPositiveX >= relativePositiveX)
            {
                screenPos.x = 0.5f + (cameraRelativeDir.x * m_miniMap.CompassSize)/*/ Camera.main.aspect*/;
                screenPos.y = 0.5f + (cameraRelativeDir.y * m_miniMap.CompassSize);
                position = screenPos;
                Iconsize = OffScreenSize;
            }
            else
            {
                Iconsize = Size;
            }
        }
        else
        {
            if (position.x == size.x - BorderOffScreen || position.y == size.y - BorderOffScreen ||
                position.x == -size.x - BorderOffScreen || -position.y == size.y - BorderOffScreen)
            {
                Iconsize = OffScreenSize;
            }
            else
            {
                Iconsize = Size;
            }
        }
        //Apply position to the UI (for follow)
        GraphicRect.anchoredPosition = position;
        if (CircleAreaRect != null) { CircleAreaRect.anchoredPosition = UnClampPosition; }
        //Change size with smooth transition
        float CorrectSize = Iconsize * MiniMap.IconMultiplier;
        GraphicRect.sizeDelta = Vector2.Lerp(GraphicRect.sizeDelta, new Vector2(CorrectSize, CorrectSize), Time.deltaTime * 8);

        if (MiniMap.RotationAlwaysFront)
        {
            //with this the icon rotation always will be the same (for front)
            Quaternion r = Quaternion.identity;
            r.x = Target.rotation.x;
            GraphicRect.localRotation = r;
        }
        else
        {
            //with this the rotation icon will depend of target
            Vector3 vre = MiniMap.transform.eulerAngles;
            Vector3 re = Vector3.zero;
            //Fix player rotation for apply to el icon.
            re.z = ((-Target.rotation.eulerAngles.y) + vre.y);

            Quaternion q = Quaternion.Euler(re);
            if (MiniMap.m_Mode == bl_MiniMap.RenderMode.Mode2D)
                GraphicRect.rotation = q;
            else
                GraphicRect.localRotation = q;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void StartEffect()
    {
        Animator a = Graphic.GetComponent<Animator>();
        if (m_Effect == ItemEffect.Pulsing)
        {
            a.SetInteger("Type", 2);
        }
        else if (m_Effect == ItemEffect.Fade)
        {
            a.SetInteger("Type", 1);
        }
    }

    /// <summary>
    /// When player or the target die,desactive,remove,etc..
    /// call this for remove the item UI from Map
    /// for change to other icon and desactive in certain time
    /// or destroy immediate
    /// </summary>
    /// <param name="inmediate"></param>
    public void DestroyItem(bool inmediate)
    {
        if (Graphic == null)
        {
            // Debug.Log("Graphic Item of " + this.name + " not exist in scene");
            return;
        }

        if (DeathIcon == null || inmediate)
        {
            Graphic.GetComponent<bl_IconItem>().DestroyIcon(inmediate);
        }
        else
        {
            Graphic.GetComponent<bl_IconItem>().DestroyIcon(inmediate, DeathIcon);
        }
        isVisible = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ico"></param>
    public void SetIcon(Sprite ico)
    {
        if (cacheItem == null)
        {
            Debug.LogWarning("You can't set a icon before create the item.");
            return;
        }

        cacheItem.GetComponent<bl_IconItem>().SetIcon(ico);
    }

    /// <summary>
    /// Show a visible circle area in the minimap with this
    /// item as center
    /// </summary>
    public void SetCircleArea(float radius, Color AreaColor)
    {
        CircleAreaRect = cacheItem.GetComponent<bl_IconItem>().SetCircleArea(radius, AreaColor);
    }

    /// <summary>
    /// 
    /// </summary>
    public void HideCircleArea()
    {
        cacheItem.GetComponent<bl_IconItem>().HideCircleArea();
        CircleAreaRect = null;
    }

    /// <summary>
    /// Call this for hide item in miniMap
    /// For show again just call "ShowItem()"
    /// NOTE: For destroy item call "DestroyItem(bool immediate)" instant this.
    /// </summary>
    public void HideItem()
    {
        if (cacheItem != null)
        {
            cacheItem.SetActive(false);
            isVisible = false;
        }
        /* else
         {
             Debug.Log("There is no item to disable.");
         }*/
    }

    /// <summary>
    /// Call this for show again the item in miniMap when is hide
    /// </summary>
    public void ShowItem()
    {
        if (cacheItem != null)
        {
            cacheItem.SetActive(true);
            cacheItem.GetComponent<bl_IconItem>().SetVisibleAlpha();
            isVisible = true;
            IconControl();
        }
        /* else
         {
             Debug.Log("There is no item to active.");
         }*/
    }

    /// <summary>
    /// If you need destroy icon when this gameObject is destroy.
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (DestroyWithObject)
        {
            DestroyItem(true);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        MiniMap?.RemoveItem(this);
        if (m_IconType != IconType.General)
            bl_EventHandler.onLocalPlayerSpawn += OnPlayerSpawn;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (m_IconType != IconType.General)
            bl_EventHandler.onLocalPlayerSpawn -= OnPlayerSpawn;
    }

    void OnPlayerSpawn()
    {
        if (Graphic != null)
        {
            if (m_IconType == IconType.Player)
            {
                isTeamMate = photonView.Owner.isTeamMate();
                Graphic.sprite = isTeamMate ? bl_MiniMapData.Instance.TeamMateIcon : bl_MiniMapData.Instance.EnemyIcon;
                Graphic.color = isTeamMate ? bl_MiniMapData.Instance.TeammateIconColor : bl_MiniMapData.Instance.EnemyIconColor;
            }
            else if (m_IconType == IconType.Bot)
            {
                isTeamMate = GetComponent<bl_AIShooter>().isTeamMate;
                Graphic.sprite = isTeamMate ? bl_MiniMapData.Instance.TeamMateIcon : bl_MiniMapData.Instance.EnemyIcon;
                Graphic.color = isTeamMate ? bl_MiniMapData.Instance.TeammateIconColor : bl_MiniMapData.Instance.EnemyIconColor;
                if (!isTeamMate)
                {
                    HideItem();
                }
                else { ShowItem(); }
            }
        }
    }

    private bool isTeamMate = true;
    public bool isTeamMateBot()
    {
        if (m_IconType != IconType.Bot) return false;

        return isTeamMate;
    }

    /// <summary>
    /// 
    /// </summary>
    public Vector3 TargetPosition
    {
        get
        {
            if (Target == null)
            {
                return Vector3.zero;
            }

            return new Vector3(Target.position.x, 0, Target.position.z);
        }
    }

    private bl_MiniMap _minimap = null;
    private bl_MiniMap m_miniMap
    {
        get
        {
            if (_minimap == null)
            {
                _minimap = this.cacheItem.transform.root.GetComponentInChildren<bl_MiniMap>();
            }
            return _minimap;
        }
    }
}