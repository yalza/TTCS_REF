using UnityEngine;

public class bl_HitBox : bl_HitBoxBase
{   
    /// <summary>
    /// Use this for receive damage local and sync for all other
    /// </summary>
    public override void ReceiveDamage(DamageData damageData)
    {
        damageData.Damage = Mathf.FloorToInt(damageData.Damage * hitBoxInfo.DamageMultiplier);
        damageData.isHeadShot = hitBoxInfo.Bone == HumanBodyBones.Head;

        hitBoxManager?.OnHit(damageData, this);
    }
}