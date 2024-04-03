using UnityEngine;

public abstract class bl_CountDownBase : MonoBehaviour
{
    /// <summary>
    /// Is currently counting down?
    /// </summary>
    public virtual bool IsCounting
    {
        get;
        set;
    } = false;

    /// <summary>
    /// 
    /// </summary>
    public abstract void StartCountDown(bool overrideIfStarted = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    public abstract void SetCount(int count);

    /// <summary>
    /// 
    /// </summary>
    private static bl_CountDownBase _instance;
    public static bl_CountDownBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_CountDownBase>(); }
            return _instance;
        }
    }
}