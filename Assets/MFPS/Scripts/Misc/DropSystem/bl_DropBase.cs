using UnityEngine;

/// <summary>
/// Base class for drop/packages in-game
/// Inherited your custom drop/package delivery scripts from this class
/// </summary>
public abstract class bl_DropBase : MonoBehaviour
{
    public struct DropData
    {
        public int KitID;
        public Vector3 DropPosition;
        public float DeliveryDuration;
        public GameObject DropPrefab;
    }

    /// <summary>
    /// Function that handle the instantiation of the drop/package
    /// </summary>
    public abstract void Dispatch(DropData dropData);
}