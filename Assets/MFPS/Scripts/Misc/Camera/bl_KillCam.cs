using UnityEngine;
using System.Collections;

public class bl_KillCam : bl_KillCamBase
{

    #region Public members
    public Transform target = null;
    public float distance = 10.0f;
    public float distanceMax = 15f;
    public float distanceMin = 0.5f;
    public float xSpeed = 120f;
    public float yMaxLimit = 80f;
    public float yMinLimit = -20f;
    public float ySpeed = 120f;
    public LayerMask layers; 
    #endregion

    #region Private members
    float x = 0;
    float y = 0;
    private int CurrentTarget = 0;
    private bl_GameManager Manager;
    private bool canManipulate = false;
    private KillCameraType cameraType = KillCameraType.ObserveDeath;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onLocalPlayerSpawn += OnLocalSpawn;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalSpawn;
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        transform.parent = null;
        Manager = bl_GameManager.Instance;
        cameraType = bl_GameData.Instance.killCameraType;
        if (target != null)
        {
            transform.LookAt(target);
            StartCoroutine(ZoomOut());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (target != null)
        {
            Orbit();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalSpawn()
    {
        target = null;
        canManipulate = false;
        bl_KillCamUIBase.Instance?.Hide();
        SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeTarget(bool next)
    {
        if (Manager.OthersActorsInScene.Count <= 0)
            return;

        if (next) { CurrentTarget = (CurrentTarget + 1) % Manager.OthersActorsInScene.Count; }
        else
        {
            if (CurrentTarget > 0) { CurrentTarget--; } else { CurrentTarget = Manager.OthersActorsInScene.Count - 1; }
        }
        target = Manager.OthersActorsInScene[CurrentTarget].Actor;
    }

    /// <summary>
    /// update camera movements
    /// </summary>
    void Orbit()
    {
        if (!canManipulate || cameraType != KillCameraType.OrbitTarget)
            return;

        float targetDistance = distance;
        var ray = new Ray(target.position, CachedTransform.position - target.position);
        if (Physics.SphereCast(ray, 0.2f, out var hit, distance, layers))
        {
            targetDistance = bl_MathUtility.Distance(target.position, hit.point) - 0.21f;
        }

        x += ((bl_GameInput.MouseX * this.xSpeed) * targetDistance) * 0.02f;
        y -= (bl_GameInput.MouseY * this.ySpeed) * 0.02f;
        y = bl_MathUtility.ClampAngle(this.y, this.yMinLimit, this.yMaxLimit);
        Quaternion quaternion = Quaternion.Euler(this.y, this.x, 0f);
        //this.distance = Mathf.Clamp(this.distance - (Input.GetAxis("Mouse ScrollWheel") * 5f), distanceMin, distanceMax);

        Vector3 vector = new Vector3(0f, 0f, -targetDistance);
        Vector3 vector2 = target.position;
        vector2.y = target.position.y + 1f;
        Vector3 vector3 = (quaternion * vector) + vector2;
        transform.rotation = quaternion;
        transform.position = vector3;
    }

    /// <summary>
    /// Set the kill cam focus/expectate target
    /// </summary>
    /// <param name="targetName"></param>
    public override bl_KillCamBase SetTarget(KillCamInfo info)
    {
        //if the player send the target
        if(info.Target != null && (bl_GameData.Instance.killCameraType == KillCameraType.ObserveDeath || string.IsNullOrEmpty(info.TargetName)))
        {
            target = info.Target;
            ReadyToShow(info);
            return this;
        }

        //if the player just send the name of the target
        var targetName = info.TargetName;
        if (string.IsNullOrEmpty(targetName)) return this;

        //try to find it by the name
        var targetInstance = GameObject.Find(targetName);

        if (targetName == LocalName)
        {
            if (info.FallbackTarget == null) return this;

            targetInstance = info.FallbackTarget.gameObject;
        }

        if (targetInstance == null)
        {
            var playerActor = bl_GameManager.Instance.FindActor(targetName);
            if (playerActor != null && playerActor.Actor != null)
            {
                targetInstance = playerActor.Actor.gameObject;
            }
        }

        if(targetInstance == null && bl_GameData.Instance.killCameraType == KillCameraType.OrbitTarget && info.FallbackTarget)
        {
            targetInstance = info.FallbackTarget.gameObject;
        }

        if (targetInstance == null)
        {
            Debug.LogWarning($"Couldn't found the kill cam target '{targetName}'.");
        }
        else
        {
            target = targetInstance.transform;
            ReadyToShow(info);
        }
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    private void ReadyToShow(KillCamInfo info)
    {
        canManipulate = true;
        bl_KillCamUIBase.Instance?.Show(info);
        if(bl_GameData.Instance.killCameraType == KillCameraType.ObserveDeath && target != null)
        {
            PositionedAndLookAt(target);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public override void SetActive(bool active)
    {
        gameObject.SetActive(active);
        if (!active) bl_KillCamUIBase.Instance?.Hide();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SpectPlayer(Transform player)
    {
        target = player;
        canManipulate = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void PositionedAndLookAt(Transform reference)
    {
        float distanceFromLocal = 2.5f;
        Vector3 position = reference.position + (Vector3.up * 1.5f);
        CachedTransform.position = position - (reference.forward * distanceFromLocal);
        RaycastHit rayHit;
        if (Physics.Raycast(reference.position, -reference.forward, out rayHit, distanceFromLocal, layers, QueryTriggerInteraction.Ignore))
        {
            CachedTransform.position = Vector3.Lerp(rayHit.point, reference.position, 0.05f);
        }

        var up = CachedTransform.position + (Vector3.up * distanceFromLocal);
        if (Physics.Raycast(up, Vector3.down, out rayHit, distanceFromLocal, layers, QueryTriggerInteraction.Ignore))
        {
            CachedTransform.position = Vector3.Lerp(up, rayHit.point, 0.4f);
        }

        CachedTransform.LookAt(position + Vector3.up * 0.25f);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator ZoomOut()
    {
        float d = 0;
        Vector3 next = target.position + transform.TransformDirection(new Vector3(0, 0, -3));
        Vector3 origin = target.position;
        transform.position = target.position;
        while (d < 1)
        {
            d += Time.deltaTime * 1.25f;
            transform.position = Vector3.Lerp(origin, next, d);
            transform.LookAt(target);
            yield return null;
        }
        x = CachedTransform.eulerAngles.y;
        y = CachedTransform.eulerAngles.x;
    }

    public enum KillCameraType
    {
        OrbitTarget,
        ObserveDeath,
    }
}