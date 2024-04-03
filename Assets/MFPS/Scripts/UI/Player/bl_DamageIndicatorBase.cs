using UnityEngine;

public abstract class bl_DamageIndicatorBase : bl_MonoBehaviour
{
    /// <summary>
    ///
    /// </summary>
    public abstract void SetHit(Vector3 direction);

    private static bl_DamageIndicatorBase _instance;

    public static bl_DamageIndicatorBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<bl_DamageIndicatorBase>();
            }
            return _instance;
        }
    }
}