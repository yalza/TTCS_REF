using UnityEngine;
using System.Collections.Generic;

public class bl_PlayerRagdoll : bl_PlayerRagdollBase
{
    #region Public members
    [LovattoToogle] public bool ApplyVelocityToRagdoll = true;
    public bl_PlayerReferences playerReferences;
    public Transform RightHand;
    public Transform PelvisBone;
    public Collider[] playerColliders;
    public List<Rigidbody> rigidBodys = new List<Rigidbody>();
    #endregion

    private Collider[] allPlayerCollider;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (rigidBodys.Count > 0)
        {
            SetKinematic();
        }
    }

    /// <summary>
    /// Set ragdoll state for the local player
    /// </summary>
    public override void SetLocalRagdoll(RagdollInfo info)
    {
        if (RightHand != null && info.RightHandChild != null)
        {
            Vector3 RootPos = info.RightHandChild.localPosition;
            info.RightHandChild.parent = RightHand;
            info.RightHandChild.localPosition = RootPos;
        }
        Ragdolled(info);
    }

    /// <summary>
    /// Change the ragdoll kinematic state
    /// </summary>
    /// <param name="kinematic">is Kinematic?</param>
    public void SetKinematic(bool kinematic = true)
    {
        if (rigidBodys == null || rigidBodys.Count <= 0)
            return;

        foreach (Rigidbody r in rigidBodys)
        {
            if (r == null) continue;
            r.isKinematic = kinematic;
        }
    }

    /// <summary>
    /// Make the current player a ragdoll
    /// The player won't be controlled by the Character Controller anymore 
    /// and will be detached from the player root
    /// </summary>
    public override void Ragdolled(RagdollInfo info)
    {
        gameObject.SetActive(true);
        if (!info.AutoDestroy)
        {
            //apply the a frame to the animator with the current player state
            playerReferences.PlayerAnimator.speed = 5;
            playerReferences.playerAnimations.UpdateAnimatorParameters();
            //update multiple at once in order to play the right pose
            for (int i = 0; i < 4; i++)
                playerReferences.PlayerAnimator.Update(1);
        }
        Destroy(playerReferences.playerAnimations);
        this.transform.parent = null;

        playerReferences.PlayerAnimator.enabled = false;
        SetActiveRagdollPhysics(true);
        foreach (Rigidbody r in rigidBodys)
        {
            if (r == null) continue;

            r.isKinematic = false;
            r.useGravity = true;
            if (ApplyVelocityToRagdoll)
            {
                r.velocity = info.AutoDestroy ? playerReferences.PlayerAnimator.velocity : info.Velocity;
            }
            if (info.IsFromExplosion)
            {
                r.AddExplosionForce(875, info.ForcePosition, 7);
            }
        }

        if (info.AutoDestroy) Destroy(gameObject, bl_GameData.Instance.PlayerRespawnTime);
    }

    /// <summary>
    /// Make the local player ignore its Character Controller collider.
    /// </summary>
    public override void IgnorePlayerCollider()
    {
        GameObject p = bl_GameManager.Instance.LocalPlayer;
        if (p == null) return;

        Collider Player = p.GetComponent<Collider>();
        if (Player != null)
        {
            for (int i = 0; i < playerColliders.Length; i++)
            {
                var col = playerColliders[i];
                if (col == null) continue;

                Physics.IgnoreCollision(col, Player);
            }
        }
    }

    /// <summary>
    /// Active/Deactivate the player rigid bodies and colliders
    /// </summary>
    public override void SetActiveRagdollPhysics(bool active)
    {
        for (int i = 0; i < playerColliders.Length; i++)
        {
            if (playerColliders[i] == null) continue;

            playerColliders[i].enabled = active;
        }
        foreach (var item in rigidBodys)
        {
            if (item == null) continue;

            item.isKinematic = !active;
        }
    }

    /// <summary>
    /// Make this player colliders ignore the give colliders
    /// </summary>
    /// <param name="list"></param>
    /// <param name="ignore"></param>
    public override void IgnoreColliders(Collider[] list, bool ignore)
    {
        if (allPlayerCollider == null || allPlayerCollider.Length <= 0)
        {
            allPlayerCollider = transform.GetComponentsInChildren<Collider>();
        }
        for (int e = 0; e < list.Length; e++)
        {
            if (list[e] == null) continue;

            for (int i = 0; i < allPlayerCollider.Length; i++)
            {
                if (allPlayerCollider[i] != null)
                {
                    Physics.IgnoreCollision(allPlayerCollider[i], list[e], ignore);
                }
            }
        }
    }

    [ContextMenu("Setup")]
    public void SetUpHitBoxes()
    {   
        GetRigidBodys();
        GetRequireBones();
    }

    void GetRigidBodys()
    {
        rigidBodys.Clear();
        rigidBodys.AddRange(transform.GetComponentsInChildren<Rigidbody>());
        playerColliders = transform.GetComponentsInChildren<Collider>();
    }

    public void GetRequireBones()
    {
        RightHand = playerReferences.PlayerAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        PelvisBone = playerReferences.PlayerAnimator.GetBoneTransform(HumanBodyBones.Hips);
    }
}