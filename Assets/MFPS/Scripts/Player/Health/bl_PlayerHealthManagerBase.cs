/// <summary>
/// Base script for the health manager script of players and bots
/// </summary>
public abstract class bl_PlayerHealthManagerBase : bl_MonoBehaviour
{

    /// <summary>
    /// 
    /// </summary>
    private bool damageEnable = true;
    public bool DamageEnabled
    {
        get => damageEnable;
        set => damageEnable = value;
    }

    /// <summary>
    /// Function that should handle the damage give from a local player
    /// it should sync the damage with all other players.
    /// </summary>
    /// <param name="damageData"></param>
    public abstract void DoDamage(DamageData damageData);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damageData"></param>
    public abstract void DoFallDamage(int damage);

    /// <summary>
    /// Make the player die instantaneously
    /// </summary>
    public abstract bool Suicide();

    /// <summary>
    /// Return the current health of the player
    /// </summary>
    /// <returns></returns>
    public abstract int GetHealth();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract int GetMaxHealth();

    /// <summary>
    /// Add or replace the current health value
    /// This should only be called by the owner of the network entity or the Master
    /// </summary>
    /// <param name="replace">If true the amount will replace the current health, otherwise, the amount will be added</param>
    public abstract void SetHealth(int amount, bool replace = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    public abstract void DoRepetingDamage(RepetingDamageInfo info);

    /// <summary>
    /// 
    /// </summary>
    public abstract void CancelRepetingDamage();

    /// <summary>
    /// 
    /// </summary>
    public abstract void DestroyEntity();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract bool IsDeath();

    public class RepetingDamageInfo
    {
        public int Damage;
        public float Rate;
        public DamageData DamageData;
    }
}