using System.Collections.Generic;
using UnityEngine;

public class bl_AICoverPoint : MonoBehaviour
{
    [Tooltip("Should the bot crouch when reach this cover point?")]
    [LovattoToogle] public bool Crouch = false;
    public float lastUseTime { get; set; } = 0;
    public List<bl_AICoverPoint> NeighbordPoints = new List<bl_AICoverPoint>();

    private bool m_positionCached = false;
    private Vector3 _position;
    public Vector3 Position
    {
        get
        {
            if (!m_positionCached)
            {
                _position = ThisTransform.position;
                m_positionCached = true;
            }
            return _position;
        }
    }

    private Transform _transform;
    public Transform ThisTransform
    {
        get
        {
            if (_transform == null) _transform = transform;
            return _transform;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        bl_AICovertPointManager.Register(this);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsAvailable(float timeSpan)
    {
        return (Time.time - lastUseTime) > timeSpan;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_AICoverPoint TryGetAvailableNeighbord()
    {
        if (NeighbordPoints == null || NeighbordPoints.Count <= 0) return null;

        for (int i = 0; i < NeighbordPoints.Count; i++)
        {
            if (NeighbordPoints[i] == null) continue;

            if (NeighbordPoints[i].IsAvailable(bl_AICovertPointManager.Instance.UsageTime))
                return NeighbordPoints[i];
        }
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool HasNeighbords() => NeighbordPoints != null && NeighbordPoints.Count > 0;

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmos()
    {
        if(bl_AICovertPointManager.Instance == null)
        {
            return;
        }
        if (!bl_AICovertPointManager.Instance.ShowGizmos)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1);
        Gizmos.DrawCube(transform.position, new Vector3(1, 0.1f, 1));

        Gizmos.color = Color.red;
        if (NeighbordPoints.Count > 0)
        {
            for (int i = 0; i < NeighbordPoints.Count; i++)
            {
                if (NeighbordPoints[i] == null) continue;
                Gizmos.DrawLine(transform.position, NeighbordPoints[i].transform.position);
            }
        }
    }
}