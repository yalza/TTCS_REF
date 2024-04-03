using UnityEngine;

public abstract class bl_PlayerRagdollBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public struct RagdollInfo
    {
        public Vector3 ForcePosition;
        public Vector3 Velocity;
        public bool IsFromExplosion;
        public bool AutoDestroy;
        public Transform RightHandChild;
    }

    /// <summary>
    /// Make a remote player a ragdoll
    /// </summary>
    public abstract void Ragdolled(RagdollInfo info);

    /// <summary>
    /// Make the local player ragdoll
    /// </summary>
    public abstract void SetLocalRagdoll(RagdollInfo info);

    /// <summary>
    /// Active/Deactivate all the colliders and Rigidbody's of the ragdoll
    /// </summary>
    public abstract void SetActiveRagdollPhysics(bool active);

    /// <summary>
    /// Make this ragdoll collider ignore or detect the given colliders
    /// </summary>
    public abstract void IgnoreColliders(Collider[] list, bool ignore);

    /// <summary>
    /// In this function you should make this player collider ignore the LOCAL player colliders.
    /// </summary>
    public abstract void IgnorePlayerCollider();
}