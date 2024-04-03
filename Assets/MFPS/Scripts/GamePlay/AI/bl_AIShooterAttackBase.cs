using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class bl_AIShooterAttackBase : bl_PhotonHelper
{
    public enum FireReason
    {
        Normal,
        OnMove,
        Forced,
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract bool IsFiring
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract void Fire(FireReason fireReason = FireReason.Normal);

    /// <summary>
    /// 
    /// </summary>
    public abstract void OnDeath();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract Vector3 GetFirePosition();
}