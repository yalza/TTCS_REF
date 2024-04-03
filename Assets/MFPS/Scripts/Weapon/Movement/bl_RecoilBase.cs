/// <summary>
/// Base class for the player weapon recoil movement
/// Inherited from this your custom recoil script.
/// </summary>
public abstract class bl_RecoilBase : bl_MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public struct RecoilData
    {
        public float Amount;
        public float Speed;

        public RecoilData(float amount)
        {
            Amount = amount;
            Speed = 2;
        }
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amount"></param>
    public abstract void SetRecoil(RecoilData data);

}