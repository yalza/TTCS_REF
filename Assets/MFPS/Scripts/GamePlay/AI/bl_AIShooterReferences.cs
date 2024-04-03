using UnityEngine.AI;
using UnityEngine;

public class bl_AIShooterReferences : bl_PlayerReferencesCommon
{
    public bl_AIShooter aiShooter;
    public bl_PlayerHealthManagerBase shooterHealth;
    public bl_AIShooterNetwork shooterNetwork;
    public bl_AIShooterAttackBase shooterWeapon;
    public bl_AIAnimationBase aiAnimation;
    public bl_HitBoxManager hitBoxManager;
    public bl_NamePlateBase namePlateDrawer;
    public NavMeshAgent Agent;
    [SerializeField] private Animator m_playerAnimator;
    public override Animator PlayerAnimator
    {
        get => m_playerAnimator;
        set => m_playerAnimator = value;
    }

    [SerializeField] private Transform m_botAimTarget = null;
    public override Transform BotAimTarget
    {
        get
        {
            if (m_botAimTarget == null) m_botAimTarget = transform;
            return m_botAimTarget;
        }
        set => m_botAimTarget = value;
    }


    private Collider[] m_allColliders;
    public override Collider[] AllColliders
    {
        get
        {
            if (m_allColliders == null || m_allColliders.Length <= 0)
            {
                m_allColliders = transform.GetComponentsInChildren<Collider>(true);
            }
            return m_allColliders;
        }
    }

    public override Team PlayerTeam => aiShooter.AITeam;

    /// <summary>
    /// 
    /// </summary>
    public override void IgnoreColliders(Collider[] list, bool ignore)
    {
        for (int e = 0; e < list.Length; e++)
        {
            for (int i = 0; i < AllColliders.Length; i++)
            {
                if (AllColliders[i] != null)
                {
                    Physics.IgnoreCollision(AllColliders[i], list[e], ignore);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsDeath()
    {
        return shooterHealth.IsDeath();
    }
}