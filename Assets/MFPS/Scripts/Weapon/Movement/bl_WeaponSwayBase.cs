public abstract class bl_WeaponSwayBase : bl_MonoBehaviour
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="multiplier"></param>
    public abstract void SetMotionMultiplier(float multiplier);

    /// <summary>
    /// 
    /// </summary>
    public abstract void UseAimSettings();

    /// <summary>
    /// Reset all the settings to its original value
    /// </summary>
    public abstract void ResetSettings();
}