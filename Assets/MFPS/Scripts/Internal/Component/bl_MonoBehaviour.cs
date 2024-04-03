using MFPS.Internal;
using UnityEngine;

public class bl_MonoBehaviour : bl_PhotonHelper
{
    private bool isRegister = false;

    protected virtual void Awake()
    {
        if (!isRegister)
        {
            bl_UpdateManager.AddItem(this);
            isRegister = true;
        }
    }

    protected virtual void OnDisable()
    {
        if (isRegister)
        {
            bl_UpdateManager.RemoveSpecificItem(this);
            isRegister = false;
        }
    }

    protected virtual void OnDestroy()
    {
        if (isRegister)
        {
            bl_UpdateManager.RemoveSpecificItem(this);
            isRegister = false;
        }
    }


    protected virtual void OnEnable()
    {
        if (!isRegister)
        {
            bl_UpdateManager.AddItem(this);
            isRegister = true;
        }
    }

    public virtual void OnUpdate() { }

    public virtual void OnFixedUpdate() { }

    public virtual void OnLateUpdate() { }

    public virtual void OnSlowUpdate() { }

    private Transform m_cachedTransform = null;
    public Transform CachedTransform
    {
        get
        {
            if (m_cachedTransform == null) m_cachedTransform = transform;
            return m_cachedTransform;
        }
    }
}