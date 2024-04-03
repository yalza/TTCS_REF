using System;
using UnityEngine;

public abstract class bl_HitBoxBase : MonoBehaviour, IMFPSDamageable
{
    public HitBoxInfo hitBoxInfo;
    public bl_HitBoxManager hitBoxManager;

    /// <summary>
    /// Use this for receive damage local and sync for all other
    /// </summary>
    /// <param name="damageData"></param>
    public abstract void ReceiveDamage(DamageData damageData);
}

[Serializable]
public class HitBoxInfo
{
    public HumanBodyBones Bone;
    [Range(0.5f, 10)] public float DamageMultiplier = 1.0f;
    public Collider collider;
}