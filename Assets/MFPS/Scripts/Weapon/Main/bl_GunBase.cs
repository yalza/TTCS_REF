#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

/// <summary>
/// Base class for the default MFPS weapons
/// bl_Gun.cs is a hard coded class not easy to extend, a modular weapon system is on the works
/// At the moment, bl_CustomGunBase.cs is the way to extended and create your own custom weapon script.
/// </summary>
public abstract class bl_GunBase : bl_MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public enum FireType
    {
        Undefined,
        Auto,
        Single,
        Semi,
    }

    private bl_GunInfo m_gunInfo = null;
    /// <summary>
    /// Weapon information
    /// </summary>
    public bl_GunInfo Info
    {
        get
        {
            if (m_gunInfo == null)
            {
                m_gunInfo = bl_GameData.Instance.GetWeapon(GunID);
            }
            return m_gunInfo;
        }
        set => m_gunInfo = value;
    }

    /// <summary>
    /// Weapon Index
    /// </summary>
    public int GunID;

#if !ACTK_IS_HERE
    protected int _bulletLeft;
#else
    protected ObscuredInt _bulletLeft;
#endif
    /// <summary>
    /// Current weapon bullet left in the clip
    /// </summary>
    public int bulletsLeft
    {
        get => _bulletLeft;
        set
        {
            _bulletLeft = value;
            bl_EventHandler.DispatchLocalPlayerAmmoUpdate(_bulletLeft);
        }
    }

    /// <summary>
    /// Is the player currently aiming?
    /// </summary>
    public bool isAiming
    {
        get;
        set;
    }

    /// <summary>
    /// Is the player currently firing with this weapon
    /// </summary>
    public bool isFiring
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract FireType GetFireType();
}