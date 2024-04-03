using UnityEngine;

public abstract class bl_AIAnimationBase : bl_MonoBehaviour
{

    private Animator _animator;
    public Animator m_Animator
    {
        get
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
            return _animator;
        }
    }

    /// <summary>
    /// Make the bot player a ragdoll
    /// </summary>
    /// <param name="from"></param>
    /// <param name="isExplosion"></param>
    public abstract void Ragdolled(Vector3 from, bool isExplosion = false);

    /// <summary>
    /// React to the bot being injured
    /// </summary>
    public abstract void OnGetHit();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bodyBone"></param>
    public virtual Transform GetHumanBone(HumanBodyBones bodyBone)
    {
        if (m_Animator == null) return null;

        return m_Animator.GetBoneTransform(bodyBone);
    }
}