using MFPS.Internal.Structures;
using UnityEngine;

public abstract class bl_KillFeedUIBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="feed"></param>
    public abstract void SetKillFeed(KillFeed feed);

    /// <summary>
    /// 
    /// </summary>
    private static bl_KillFeedUIBase _instance;
    public static bl_KillFeedUIBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_KillFeedUIBase>(); }
            return _instance;
        }
    }
}