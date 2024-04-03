using UnityEngine;

public abstract class bl_ObjectPoolingBase : MonoBehaviour
{

    /// <summary>
    /// Get a pooled prefab instance.
    /// </summary>
    /// <param name="prefabName"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public abstract GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation);

    /// <summary>
    /// 
    /// </summary>
    private static bl_ObjectPoolingBase _instance;
    public static bl_ObjectPoolingBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_ObjectPoolingBase>(); }
            return _instance;
        }
    }
}