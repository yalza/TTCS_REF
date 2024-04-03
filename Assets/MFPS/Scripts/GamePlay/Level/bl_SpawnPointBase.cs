using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Spawnpoint base class
/// You can inherited from this your own spawnpoint scripts.
/// </summary>
public abstract class bl_SpawnPointBase : MonoBehaviour
{
    [FormerlySerializedAs("m_Team")]
    public Team team = Team.All;

    public static implicit operator Transform(bl_SpawnPointBase d) => d.transform;

    /// <summary>
    /// Register this spawnpoint
    /// </summary>
    public virtual void Initialize()
    {
        bl_SpawnPointManager.AddSpawnPoint(this);
    }

    /// <summary>
    /// returns a spawn position confined to this spawnpoint's settings
    /// </summary>
    public abstract void GetSpawnPosition(out Vector3 position, out Quaternion Rotation);

}