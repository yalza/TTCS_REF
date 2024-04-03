using UnityEngine;
#if UNITY_EDITOR
using MFPSEditor;
#endif

/// <summary>
/// Default spawnpoint implementation
/// If you wanna to use your own spawnpoint system simply inherited your script from bl_SpawnPointBase.cs
/// </summary>
public class bl_SpawnPoint : bl_SpawnPointBase
{

    [System.Serializable]
    public enum Shape
    {
        Cube,
        Dome
    }

    public Shape shape = Shape.Dome;
    public float SpawnSpace = 3f;
    [Tooltip("Detection ground limit")]
    public float groundSnapLimit = 1.5f;
    [Tooltip("Should the player spawn looking at the spawn direction or a random direction?")]
    [LovattoToogle] public bool randomRotation = false;

    RaycastHit hitInfo;
    private const float BASE_VERTICAL_THRESHOLD = 0.01f; // if your players fall of the map after spawn, try to increase this value.

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        // Make sure to add this spawnpoint to the list of available points
        // Otherwise your spawnpoint will never be used.
        Initialize();
    }

    /// <summary>
    /// returns a spawn position confined to this spawnpoint's settings
    /// </summary>
    public override void GetSpawnPosition(out Vector3 position, out Quaternion Rotation)
    {
        GetSpawnPosition(SpawnSpace, out position, out Rotation);
    }

    /// <summary>
    /// returns a spawn position with the given offset
    /// </summary>
    public void GetSpawnPosition(float radiusSpace, out Vector3 position, out Quaternion Rotation)
    {
        position = GetPositionInsideShape(radiusSpace);
        position = SnapPositionToGround(position);

        if (randomRotation) Rotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
        else Rotation = transform.rotation;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Vector3 GetPositionInsideShape(float space)
    {
        Vector3 s = transform.position;
        if (shape == Shape.Dome)
        {
            s = Random.insideUnitSphere * space;
            s = transform.position + new Vector3(s.x, BASE_VERTICAL_THRESHOLD, s.z);
        }
        else if (shape == Shape.Cube)
        {
            Vector3 localSize = transform.InverseTransformDirection(transform.localScale * 0.45f); // 0.05 of offset for the player collider radius
            s.x += Random.Range(-localSize.x, localSize.x);
            s.z += Random.Range(-localSize.z, localSize.z);
        }

        return s;
    }

    /// <summary>
    /// 
    /// </summary>
    public Vector3 SnapPositionToGround(Vector3 position)
    {
        if (Physics.SphereCast(new Ray(position + (Vector3.up * groundSnapLimit), Vector3.down), 0.1f, out hitInfo, groundSnapLimit * 2.0f, bl_GameData.TagsAndLayerSettings.EnvironmentOnly))
        {
            if (hitInfo.collider != null) position.y = hitInfo.point.y + BASE_VERTICAL_THRESHOLD;
        }

        return position;
    }

#if UNITY_EDITOR
    DomeGizmo _gizmo = null;
    void OnDrawGizmosSelected()
    {
        Draw();
    }

    private void OnDrawGizmos()
    {
        if (bl_SpawnPointManager.Instance == null || !bl_SpawnPointManager.Instance.drawSpawnPoints) return;
        Draw();
    }

    void Draw()
    {
        if (bl_SpawnPointManager.Instance == null || Application.isPlaying) return;

        if (shape == Shape.Dome)
        {
            float h = 180;
            if (_gizmo == null || _gizmo.horizon != h)
            {
                _gizmo = new DomeGizmo(h);
            }
        }

        Color c = (team == Team.Team2) ? bl_GameData.Instance.Team2Color : bl_GameData.Instance.Team1Color;
        if (team == Team.All) { c = Color.white; }
        Gizmos.color = c;

        if (shape == Shape.Dome)
        {
            _gizmo.Draw(transform, c, SpawnSpace);

        }
        else if (shape == Shape.Cube)
        {
            var matrix = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            c.a = 0.3f;
            Gizmos.color = c;
            Vector3 basePos = Vector3.zero;
            basePos.y += 1.1f;
            Gizmos.DrawCube(basePos, new Vector3(transform.localScale.x, 2.2f, transform.localScale.z));
            Gizmos.DrawWireCube(basePos, new Vector3(transform.localScale.x, 2.2f, transform.localScale.z));
            Gizmos.matrix = matrix;
        }

        if (bl_SpawnPointManager.Instance.SpawnPointPlayerGizmo != null)
        {
            Gizmos.DrawWireMesh(bl_SpawnPointManager.Instance.SpawnPointPlayerGizmo, transform.position, transform.rotation, Vector3.one * 2.75f);
        }
        if (!randomRotation)
            Gizmos.DrawLine(base.transform.position + ((base.transform.forward * this.SpawnSpace)), base.transform.position + (((base.transform.forward * 2f) * this.SpawnSpace)));
        Gizmos.color = Color.white;
    }
#endif
}