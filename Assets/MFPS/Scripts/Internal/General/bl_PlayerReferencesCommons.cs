using UnityEngine;

public abstract class bl_PlayerReferencesCommon : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public abstract Collider[] AllColliders 
    { 
        get; 
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract Animator PlayerAnimator 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract Transform BotAimTarget 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract Team PlayerTeam
    {
        get;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <param name="ignore"></param>
    public abstract void IgnoreColliders(Collider[] list, bool ignore);

    /// <summary>
    /// Determine if this player is death
    /// Since when the player dies it still exists in the game as a ragdoll for a short period of time
    /// </summary>
    /// <returns></returns>
    public abstract bool IsDeath();
}