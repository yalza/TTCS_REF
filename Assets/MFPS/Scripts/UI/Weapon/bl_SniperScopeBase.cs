using UnityEngine;
using UnityEngine.Serialization;

public abstract class bl_SniperScopeBase : bl_MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    [FormerlySerializedAs("Scope"), SerializeField]
    private Sprite scopeTexture;
    public Sprite ScopeTexture
    {
        get => scopeTexture;
        set => scopeTexture = value;
    }
}