using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class bl_AICovertPointManager : MonoBehaviour
{
    public float MaxDistance = 50;
    public float UsageTime = 10;
    public float maxNeighbordDistance = 25;
    [LovattoToogle] public bool onlyAllowOneBotPerCover = true;
    [LovattoToogle] public bool ShowGizmos = true;

    public static List<bl_AICoverPoint> AllCovers = new List<bl_AICoverPoint>();

    /// <summary>
    /// 
    /// </summary>
    private void OnDestroy()
    {
        // Since it's a static property, make sure to clean up after unload the scene.
        AllCovers.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Register(bl_AICoverPoint co)
    {
        AllCovers.Add(co);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bl_AICoverPoint GetCloseCover(Transform target)
    {
        if (AllCovers == null || AllCovers.Count <= 0)
        {
            Debug.LogWarning("There is no Cover Points for bots in this scene, bots behave will be limited.");
            return null;
        }

        bl_AICoverPoint cover = null;
        float d = MaxDistance;
        for (int i = 0; i < AllCovers.Count; i++)
        {
            float dis = bl_UtilityHelper.Distance(target.localPosition, AllCovers[i].Position);
            if (dis < MaxDistance && dis < d)
            {
                d = dis;
                cover = AllCovers[i];
            }
        }
        cover = CheckCoverUsage(cover);
        return cover;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public bl_AICoverPoint GetCoverOnRadius(Transform target, float radius)
    {
        if (AllCovers == null || AllCovers.Count <= 0)
        {
            Debug.LogWarning("There is no Cover Points for bots in this scene, bots behave will be limited.");
            return null;
        }

        List<bl_AICoverPoint> list = new List<bl_AICoverPoint>();
        for (int i = 0; i < AllCovers.Count; i++)
        {
            float dis = bl_UtilityHelper.Distance(target.localPosition, AllCovers[i].Position);
            if (dis <= radius)
            {
                list.Add(AllCovers[i]);
            }
        }
        bl_AICoverPoint cp = null;
        if (list.Count > 0)
        {
            cp = list[Random.Range(0, list.Count)];
        }
        if (cp == null) { cp = AllCovers[Random.Range(0, AllCovers.Count)]; }

        return cp;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bl_AICoverPoint GetCloseCoverForced(Transform target)
    {
        if (AllCovers == null || AllCovers.Count <= 0)
        {
            Debug.LogWarning("There is no Cover Points for bots in this scene, bots behave will be limited.");
            return null;
        }

        bl_AICoverPoint cover = null;
        float d = 100000;
        for (int i = 0; i < AllCovers.Count; i++)
        {
            float dis = bl_UtilityHelper.Distance(target.localPosition, AllCovers[i].Position);
            if (dis < d)
            {
                d = dis;
                cover = AllCovers[i];
            }
        }
        cover = CheckCoverUsage(cover);
        return cover;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="overrdidePoint"></param>
    /// <returns></returns>
    public bl_AICoverPoint GetCloseCover(Transform target, bl_AICoverPoint overrdidePoint)
    {
        bl_AICoverPoint cover = null;
        float d = MaxDistance;
        for (int i = 0; i < AllCovers.Count; i++)
        {
            float dis = bl_UtilityHelper.Distance(target.localPosition, AllCovers[i].Position);
            if (dis < MaxDistance && dis < d && AllCovers[i] != overrdidePoint)
            {
                d = dis;
                cover = AllCovers[i];
            }
        }
        cover = CheckCoverUsage(cover);
        return cover;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="coverSource"></param>
    /// <returns></returns>
    public bl_AICoverPoint CheckCoverUsage(bl_AICoverPoint coverSource)
    {
        if (coverSource == null) return null;

        // If this cover has been used recently, try to find another one
        if ((Time.time - coverSource.lastUseTime) <= UsageTime)
        {
            if (coverSource.HasNeighbords())
            {
                var neighbord = coverSource.TryGetAvailableNeighbord();
                if (neighbord == null)
                    neighbord = coverSource.NeighbordPoints[Random.Range(0, coverSource.NeighbordPoints.Count)];

                coverSource = neighbord;
            }
            else
            {
                if (onlyAllowOneBotPerCover) coverSource = null;
            }
        }

        if (coverSource == null) return null;

        coverSource.lastUseTime = Time.time;
        return coverSource;
    }

    private static bl_AICovertPointManager _instance;
    public static bl_AICovertPointManager Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_AICovertPointManager>(); }
            return _instance;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Fix Points")]
    public void FixedFloorPos()
    {
        bl_AICoverPoint[] sp = FindObjectsOfType<bl_AICoverPoint>();
        RaycastHit r;
        for (int i = 0; i < sp.Length; i++)
        {
            Transform t = sp[i].transform;
            Ray ray = new Ray(t.position + t.up, Vector3.down);
            if (Physics.Raycast(ray, out r, 100))
            {
                t.position = r.point;
            }
        }
    }

    [ContextMenu("Calculate Neighbors")]
    public void CalcuNeighbords()
    {
        bl_AICoverPoint[] allCovers = FindObjectsOfType<bl_AICoverPoint>();
        for (int i = 0; i < allCovers.Length; i++)
        {
            Transform t = allCovers[i].transform;
            allCovers[i].NeighbordPoints.Clear();
            for (int e = 0; e < allCovers.Length; e++)
            {
                if (allCovers[i] == allCovers[e]) continue;
                if (Vector3.Distance(allCovers[i].transform.position, allCovers[e].transform.position) <= maxNeighbordDistance)
                {
                    allCovers[i].NeighbordPoints.Add(allCovers[e]);
                }
            }
            UnityEditor.EditorUtility.SetDirty(allCovers[i]);
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(bl_AICovertPointManager))]
public class bl_AICovertPointManagerEditor : Editor
{
    bl_AICovertPointManager script;

    private void OnEnable()
    {
        script = (bl_AICovertPointManager)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10);

        if (GUILayout.Button("Bake Neighbors points"))
        {
            script.CalcuNeighbords();
        }
        if (GUILayout.Button("Align points to floors"))
        {
            script.FixedFloorPos();
        }
    }

}
#endif