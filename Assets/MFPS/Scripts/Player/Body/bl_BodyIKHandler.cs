using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class bl_BodyIKHandler
{
    public List<BodyBoneIK> allBones = new List<BodyBoneIK>();
    public Animator m_animator;
    private BodyBoneIK bodyBone;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="animator"></param>
    public void Initialize(Animator animator)
    {
        m_animator = animator;
    }

    /// <summary>
    /// 
    /// </summary>
    public BodyBoneIK AddBone(Transform target, AvatarIKGoal ikGoal, bool doPosition = true, bool doRotation = true, float weight = 1, float fadeWeight = 0)
    {
        var bbik = new BodyBoneIK()
        {
            Target = target,
            avatarIK = ikGoal,
            DoPosition = doPosition,
            DoRotation = doRotation,
            Weight = weight,
            WeightTarget = fadeWeight
        };
        if (bbik.WeightTarget > 0)
        {
            bbik.Weight = 0;
        }
        allBones.Add(bbik);
        return bbik;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnUpdate()
    {
        if (m_animator == null) return;

        for (int i = 0; i < allBones.Count; i++)
        {
            bodyBone = allBones[i];
            if (bodyBone == null) continue;

            if (bodyBone.WeightTarget > 0)
            {
                if (bodyBone.Weight < 1)
                    bodyBone.Weight += Time.deltaTime;
                else bodyBone.WeightTarget = 0;
            }
            if (bodyBone.DoPosition)
            {
                m_animator.SetIKPositionWeight(bodyBone.avatarIK, bodyBone.Weight);
                m_animator.SetIKPosition(bodyBone.avatarIK, bodyBone.GetPosition());
            }

            if (bodyBone.DoRotation)
            {
                m_animator.SetIKRotationWeight(bodyBone.avatarIK, bodyBone.Weight);
                m_animator.SetIKRotation(bodyBone.avatarIK, bodyBone.Target.rotation);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PrepareFade()
    {
        for (int i = 0; i < allBones.Count; i++)
        {
            allBones[i].WeightTarget = 1;
            allBones[i].Weight = 0;
        }
    }

    [Serializable]
    public class BodyBoneIK
    {
        public Transform Target;
        public AvatarIKGoal avatarIK;
        public float Weight = 1;

        public bool DoPosition = true;
        public bool DoRotation = true;

        public float WeightTarget = 0;

        private Vector3 PositionOffset = Vector3.zero;
        private int offsetType = 0;

        /// <summary>
        /// 
        /// </summary>
        public void SetPositionOffset(Vector3 offset)
        {
            PositionOffset = offset;
            offsetType = 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPosition()
        {
            if (offsetType == 2) return Target.position + (Target.right * PositionOffset.x);
            if (offsetType == 1) return Target.position + PositionOffset;
            if (offsetType == 3) return Target.position - (Target.right * PositionOffset.x);

            return Target.position;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetRightOffset(float space, bool negative)
        {
            offsetType = negative ? 3 : 2;
            PositionOffset.x = space;
        }
    }
}