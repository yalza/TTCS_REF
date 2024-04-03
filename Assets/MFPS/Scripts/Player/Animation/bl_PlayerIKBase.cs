using UnityEngine;

public abstract class bl_PlayerIKBase : bl_MonoBehaviour
{
    /// <summary>
    /// When this is true, you shouldn't control the arms with IK.
    /// </summary>
    public bool ControlArmsWithIK
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Return the player IK head look transform reference.
    /// </summary>
    /// <returns></returns>
    public abstract Transform HeadLookTarget
    { 
        get; 
        set; 
    }

    /// <summary>
    /// When CustomArmsIKHandler is != null
    /// You should stop controlling the Arms with IK in your inherited script.
    /// </summary>
    public bl_BodyIKHandler CustomArmsIKHandler 
    { 
        get; 
        set; 
    } = null;

    /// <summary>
    /// Initialize the IK Solver
    /// </summary>
    public abstract void Init();
}