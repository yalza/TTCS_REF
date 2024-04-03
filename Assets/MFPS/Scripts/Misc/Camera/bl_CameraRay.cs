using System.Collections.Generic;
using UnityEngine;
using System;

public class bl_CameraRay : bl_CameraRayBase
{
    #region Public members
    public int CheckFrameRate = 5;
    public CastMethod castMethod = CastMethod.Box;
    [Range(0.1f, 10)] public float DistanceCheck = 2;
    public LayerMask DetectLayers;
    public Vector3 boxDimesion = new Vector3(0.15f, 0.15f, 0.1f);
    #endregion

    #region Public properties

    private bool isChecking = false;
    public override bool IsCurrentlyDetecting
    { 
        get => isChecking;
        set
        {
            isChecking = value;
            if (!isChecking && m_currentDetectedItem != null)
            {
                m_currentDetectedItem.OnUnDetectedByPlayer();
            }
        } 
    }
    #endregion

    #region Private members
    private int currentFrame = 0;
    private RaycastHit RayHit;
    private IRayDetectable m_currentDetectedItem = null;
    private List<byte> activers = new List<byte>();
    private Dictionary<string, Action<bool>> triggers = new Dictionary<string, Action<bool>>();
    bool hasDectected = false;
    private byte increaseCounter = 0;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!IsCurrentlyDetecting) { currentFrame = 0; return; }

        if(currentFrame == 0)
        {
            Fire();
        }
        currentFrame = (currentFrame + 1) % CheckFrameRate;
    }

    /// <summary>
    /// 
    /// </summary>
    void Fire()
    {
        bool detected = false;
        if (castMethod == CastMethod.Box || castMethod == CastMethod.Both)
        {
            detected = Physics.BoxCast(CachedTransform.position, boxDimesion, CachedTransform.forward,
                out RayHit, CachedTransform.rotation, RayDistance, DetectLayers, QueryTriggerInteraction.Ignore);
        }
        if((castMethod == CastMethod.Ray || castMethod == CastMethod.Both) && !detected)
        {
            Ray r = new Ray(CachedTransform.position, CachedTransform.forward);
            detected = Physics.Raycast(r, out RayHit, RayDistance, DetectLayers, QueryTriggerInteraction.Ignore);
        }

        if (detected)
        {
            hasDectected = true;
            OnHit();

            //check in each register trigger
            if (triggers.Count > 0)
            {
                foreach (var item in triggers.Keys)
                {
                    //if the object that is in front have the same name that the register trigger -> call their callback
                    if (RayHit.transform.name == item)
                    {
                        triggers[item].Invoke(true);
                    }
                }
            }
        }
        else
        {
            // if the player was focusing in a item before, but not anymore
            UnDetectCurrentItem();

            if (triggers.Count > 0 && hasDectected)
            {
                foreach (var item in triggers.Values)
                {
                    item.Invoke(false);
                }
            }
            hasDectected = false;
        }
    }

    /// <summary>
    /// When the player camera ray hit something in the map
    /// </summary>
    void OnHit()
    {
        var item = RayHit.transform.GetComponent<IRayDetectable>();
        if (item != null)
        {
            // if was focusing in other object before
            if (m_currentDetectedItem != null && m_currentDetectedItem != item)
            {
                // let that object know that is not being focused anymore.
                m_currentDetectedItem.OnUnDetectedByPlayer();
            }

            m_currentDetectedItem = item;
            m_currentDetectedItem.OnRayDetectedByPlayer();
        }
        else
        {
            UnDetectCurrentItem();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void UnDetectCurrentItem()
    {
        if (m_currentDetectedItem == null) return;

        m_currentDetectedItem.OnUnDetectedByPlayer();
        m_currentDetectedItem = null;
    }

    /// <summary>
    /// If you wanna detect when an object is in front of the local player view
    /// register a callback in this function
    /// </summary>
    public override byte AddTrigger(DetecableInfo info)
    {
        if (triggers.ContainsKey(info.Name)) { return 0; }

        triggers.Add(info.Name, info.Callback);
        return SetActiver(true, info.ID);
    }

    /// <summary>
    /// Make sure of remove the trigger when you don't need to detect it anymore.
    /// </summary>
    public override void RemoveTrigger(DetecableInfo info)
    {
        if (!triggers.ContainsKey(info.Name)) return;
        triggers.Remove(info.Name);
        SetActiver(false, info.ID);
    }

    /// <summary>
    /// 
    /// </summary>
    public override byte SetActiver(bool add, byte id)
    {
        if (add)
        {
            if (!activers.Contains(id))
            {
                activers.Add(id);
            }
            else
            {
                increaseCounter++;
                id = increaseCounter;
                activers.Add(id);
            }
            IsCurrentlyDetecting = true;
        }
        else
        {
            if (activers.Contains(id))
            {
                activers.Remove(id);
            }
            if (activers.Count <= 0)
            {
                IsCurrentlyDetecting = false;
            }
        }
        return id;
    }

    private float RayDistance => DistanceCheck + ExtraRayDistance;

    [Serializable]
    public enum CastMethod
    {
        Ray,
        Box,
        Both,
    }
}