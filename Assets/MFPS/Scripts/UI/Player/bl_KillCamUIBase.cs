using UnityEngine;

public abstract class bl_KillCamUIBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="gunID"></param>
    public abstract void Show(bl_KillCamBase.KillCamInfo killCamInfo);

    /// <summary>
    /// 
    /// </summary>
    public abstract void Hide();


    private static bl_KillCamUIBase _instance;
    public static bl_KillCamUIBase Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<bl_KillCamUIBase>();
            return _instance;
        }
    }
}