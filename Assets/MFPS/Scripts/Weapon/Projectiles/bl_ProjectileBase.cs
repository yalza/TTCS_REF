/// <summary>
/// Base class for all the weapon projectiles (bullets, grenades, etc...)
/// </summary>
public abstract class bl_ProjectileBase : bl_MonoBehaviour
{
    /// <summary>
    /// Initialize the projectile
    /// </summary>
    public abstract void InitProjectile(BulletData data);
}