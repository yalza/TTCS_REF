/// <summary>
/// Base class for the Crosshair
/// Inherited from this your custom crosshair script.
/// </summary>
public abstract class bl_CrosshairBase : bl_MonoBehaviour
{
    /// <summary>
    /// If true = Freeze and block any action/call to the crosshair
    /// </summary>
    public bool Block
    {
        get;
        set;
    }

    /// <summary>
    /// Show/Hide the crosshair
    /// </summary>
    /// <param name="active"></param>
    public abstract void Show(bool active);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aiming"></param>
    public abstract void OnAim(bool aiming);

    /// <summary>
    /// Call when the local player hit a damageable object
    /// </summary>
    public abstract void OnHit();

    /// <summary>
    /// 
    /// </summary>
    public abstract void OnFire();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gun"></param>
    public abstract void SetupCrosshairForWeapon(GunType gunType);

    /// <summary>
    /// 
    /// </summary>
    public abstract void SetupCrosshairForWeapon(bl_Gun weapon);

    /// <summary>
    /// Allow the crosshair to fade in/out
    /// </summary>
    /// <param name="allow"></param>
    public abstract void AllowFade(bool allow);

    private static bl_CrosshairBase m_instance;
    public static bl_CrosshairBase Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<bl_CrosshairBase>();
            }
            return m_instance;
        }
    }
}