using System;
using UnityEngine;

public abstract class bl_CameraRayBase : bl_MonoBehaviour
{
    public struct DetecableInfo
    {
        public string Name;
        public byte ID;
        public Action<bool> Callback;
    }

    /// <summary>
    /// 
    /// </summary>
    public float ExtraRayDistance { get; set; } = 0;

    /// <summary>
    /// Is currently the camera detecting colliders?
    /// </summary>
    public abstract bool IsCurrentlyDetecting { get; set; }

    /// <summary>
    /// If you wanna detect when an object is in front of the local player view
    /// register a callback in this function
    /// </summary>
    public abstract byte AddTrigger(DetecableInfo detecableInfo);

    /// <summary>
    /// Make sure of remove the trigger when you don't need to detect it anymore.
    /// </summary>
    public abstract void RemoveTrigger(DetecableInfo detecableInfo);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="add"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public abstract byte SetActiver(bool add, byte id);
}