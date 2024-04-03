using System.Collections.Generic;
using UnityEngine;
using MFPS.Runtime.AI;

[ExecuteInEditMode]
public class bl_AIAnimation : bl_AIAnimationBase
{
    #region Public members
    [Header("Head Look")]
    [SerializeField, Range(0, 1)] private float Weight = 0.8f;
    [SerializeField, Range(0, 1)] private float Body = 0.9f;
    [SerializeField, Range(0, 1)] private float Head = 1;

    [Header("Ragdoll")]
    public List<Rigidbody> mRigidBody = new List<Rigidbody>(); 
    #endregion

    #region Private members
    private Animator m_animator;
    private Vector3 localVelocity;
    private float vertical;
    private float horizontal;
    private float turnSpeed;
    private Vector3 velocity;
    private float lastYRotation;
    private float TurnLerp;
    private float movementSpeed;
    private Vector3 headTarget;
    private Dictionary<string, int> animatorHashes;
    private bl_AIShooterReferences references;
    private float timeSinceLastMove = 0;
    private Vector3 fixedLookAtPosition = Vector3.zero;
    private Transform headBone;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        references = CachedTransform.root.GetComponent<bl_AIShooterReferences>();
        m_animator = GetComponent<Animator>();
        headBone = m_animator.GetBoneTransform(HumanBodyBones.Head);
        SetKinecmatic();
        if (animatorHashes == null)
        {
            FetchHashes();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void FetchHashes()
    {
        //cache the hashes in a Array will be more appropriate but to be more readable for other users
        // I decide to cached them in a Dictionary with the key name indicating the parameter that contain
        animatorHashes = new Dictionary<string, int>();
        animatorHashes.Add("Vertical", Animator.StringToHash("Vertical"));
        animatorHashes.Add("Horizontal", Animator.StringToHash("Horizontal"));
        animatorHashes.Add("Speed", Animator.StringToHash("Speed"));
        animatorHashes.Add("Turn", Animator.StringToHash("Turn"));
        animatorHashes.Add("isGround", Animator.StringToHash("isGround"));
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        ControllerInfo();
        Animate();
    }

    /// <summary>
    /// 
    /// </summary>
    void ControllerInfo()
    {
        velocity = references.shooterNetwork.Velocity;
        float delta = Time.deltaTime;
        localVelocity = CachedTransform.InverseTransformDirection(velocity);
        localVelocity.y = 0;

        vertical = Mathf.Lerp(vertical, localVelocity.z, delta * 8);
        horizontal = Mathf.Lerp(horizontal, localVelocity.x, delta * 8);
        movementSpeed = velocity.magnitude;

        if (movementSpeed > 0.1f)
        {
            timeSinceLastMove = Time.time;
        }

        turnSpeed = Mathf.DeltaAngle(lastYRotation, CachedTransform.rotation.eulerAngles.y);
        TurnLerp = Mathf.Lerp(TurnLerp, turnSpeed, 7 * delta);

        if (Time.time - timeSinceLastMove < 1)
        {
            TurnLerp = 0;
        }

        if (Time.frameCount % 7 == 0) lastYRotation = lastYRotation = CachedTransform.rotation.eulerAngles.y;
    }

    /// <summary>
    /// 
    /// </summary>
    void Animate()
    {
        if (m_animator == null)
            return;

        m_animator.SetFloat(animatorHashes["Vertical"], vertical);
        m_animator.SetFloat(animatorHashes["Horizontal"], horizontal);
        m_animator.SetFloat(animatorHashes["Speed"], movementSpeed);
        m_animator.SetFloat(animatorHashes["Turn"], TurnLerp);
        m_animator.SetBool(animatorHashes["isGround"], true);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnAnimatorIK(int layerIndex)
    {
        if (layerIndex == 1)
        {
            fixedLookAtPosition = references.aiShooter.LookAtPosition;
            if (references.aiShooter.LookingAt != AILookAt.Target && bl_MathUtility.Distance(fixedLookAtPosition, headBone.position) < Mathf.PI)
            {
                fixedLookAtPosition = (CachedTransform.position + (CachedTransform.forward * 10));
                fixedLookAtPosition.y = headBone.position.y;
            }

            m_animator.SetLookAtWeight(Weight, Body, Head, 1, 0.4f);
            headTarget = Vector3.Slerp(headTarget, fixedLookAtPosition, Time.deltaTime * 3);
            m_animator.SetLookAtPosition(headTarget);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetKinecmatic()
    {
        for(int i = 0; i< mRigidBody.Count; i++)
        {
            if (mRigidBody[i] == null) continue;

            mRigidBody[i].isKinematic = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="from"></param>
    /// <param name="isExplosion"></param>
    public override void Ragdolled(Vector3 from, bool isExplosion = false)
    {
        m_animator.enabled = false;

        for (int i = 0; i < mRigidBody.Count; i++)
        {
            if (mRigidBody[i] == null) continue;

            mRigidBody[i].isKinematic = false;
            mRigidBody[i].detectCollisions = true;
            Vector3 rhs = transform.position - from;
            //Use the exact hitbox position instead of transform.position should result in a more realistic result.
            mRigidBody[i].AddForceAtPosition(rhs.normalized * 150, transform.position);
            if (isExplosion)
            {
                mRigidBody[i].AddExplosionForce(875, from, 7);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnGetHit()
    {
        int r = Random.Range(0, 2);
        string hit = (r == 1) ? "Right Hit" : "Left Hit";
        m_animator.Play(hit, 2, 0);
    }

#if UNITY_EDITOR
    [ContextMenu("Get RigidBodys")]
#endif
    public void GetRigidBodys()
    {
        Rigidbody[] R = this.transform.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in R)
        {
            if (!mRigidBody.Contains(rb))
            {
                mRigidBody.Add(rb);
            }
        }
    }
}