using UnityEngine;

public class bl_WeaponFX : bl_WeaponFXBase
{
    public ParticleSystem[] highFxParticles;
    public ParticleSystem[] lowFxParticles;

    private ParticleSystem[] targetParticles;

    /// <summary>
    /// Play the weapon fire FX effects
    /// </summary>
    public override void PlayFireFX()
    {
        if (targetParticles == null) ActiveDependOfTarget();

        foreach (var item in targetParticles)
        {
            if (item == null) continue;
            item.Play();
        }
    }

    /// <summary>
    /// Use the particles based on the platform target
    /// high for PC and console platforms
    /// low for mobile platforms
    /// </summary>
    public void ActiveDependOfTarget()
    {
        if (bl_UtilityHelper.isMobile)
        {
            SetActiveList(highFxParticles, false);
            SetActiveList(lowFxParticles, true);
            targetParticles = lowFxParticles;
        }
        else
        {
            SetActiveList(lowFxParticles, false);
            SetActiveList(highFxParticles, true);
            targetParticles = highFxParticles;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetActiveList(ParticleSystem[] list, bool active)
    {
        foreach (var item in list)
        {
            if (item == null) continue;
            item.gameObject.SetActive(active);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        bl_UtilityHelper.DrawWireArc(transform.position, 0.03f, 360, 10, Quaternion.LookRotation(transform.up));
    }

    [ContextMenu("Try FP Auto Place")]
    void AutoPlaceInFPWeapon()
    {
        var weapon = transform.GetComponentInParent<bl_Gun>();
        if (weapon == null) return;

        var firePoint = weapon.muzzlePoint;
        if (firePoint == null) return;

        Vector3 p = firePoint.position + (weapon.transform.forward * 0.05f);
        transform.position = p;
    }
}