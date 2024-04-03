using UnityEngine;

public abstract class bl_DropCallerBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public struct DropData
    {
        public int KitID;
        public float Delay;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract void SetUp(DropData dropData);
}