using UnityEngine;

public abstract class bl_RoomCameraBase : bl_MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public enum CameraMode
    {
        MapPreview,
        Spectator
    }

    /// <summary>
    /// Don't move or rotate the camera by this script (or it's inherited script)
    /// </summary>
    public bool BlockSelfMovement
    {
        get;
        set;
    } = false;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract CameraMode GetCameraMode();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cameraMode"></param>
    public abstract void SetCameraMode(CameraMode cameraMode);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActive(bool active);

    /// <summary>
    /// 
    /// </summary>
    public abstract void ResetCamera();

    /// <summary>
    /// 
    /// </summary>
    private Camera _cameraComponent = null;
    public virtual Camera GetCamera()
    {
        if (_cameraComponent == null)
        {
            _cameraComponent = GetComponent<Camera>();
        }
        return _cameraComponent;
    }


    private static bl_RoomCameraBase _instance;
    public static bl_RoomCameraBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<bl_RoomCameraBase>();
            }
            return _instance;
        }
    }
}
