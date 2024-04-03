using UnityEngine;

public abstract class bl_RoundFinishScreenBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="winner"></param>
    public abstract void Show(string winner);

    /// <summary>
    /// 
    /// </summary>
    public abstract void Hide();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    public abstract void SetCountdown(int count);


    private static bl_RoundFinishScreenBase _instance;
    public static bl_RoundFinishScreenBase Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<bl_RoundFinishScreenBase>();
            return _instance;
        }
    }
}