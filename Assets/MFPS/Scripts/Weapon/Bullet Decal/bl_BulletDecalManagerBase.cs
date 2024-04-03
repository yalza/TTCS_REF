using UnityEngine;

public abstract class bl_BulletDecalManagerBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public static void InstantiateDecal(RaycastHit raycastHit)
    {
        if (Instance == null) return;

        Instance.InstanceDecal(raycastHit);
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract void InstanceDecal(RaycastHit raycastHit);

    /// <summary>
    /// 
    /// </summary>
    private static bl_BulletDecalManagerBase _instance;
    public static bl_BulletDecalManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_BulletDecalManagerBase>(); }
            return _instance;
        }
    }
}