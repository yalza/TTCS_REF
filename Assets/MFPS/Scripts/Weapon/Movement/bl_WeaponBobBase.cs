using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class bl_WeaponBobBase : bl_MonoBehaviour
{
    public float Intensitity { get; set; } = 1;

    /// <summary>
    /// Stop the walking bob movement
    /// </summary>
    public abstract void Stop();

    /// <summary>
    /// Update the walk animation elsewhere
    /// if useAnimation == false -> control the movement in the weapon bob script
    /// else -> controlled in the passed action 'callback' and do not update the weapon bob script.
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="useAnimation"></param>
    public abstract void AnimatedThis(Action<PlayerState> callback, bool useAnimation);
}