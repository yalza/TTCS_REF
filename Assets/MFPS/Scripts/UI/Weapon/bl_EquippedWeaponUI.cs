using TMPro;
using UnityEngine;

public class bl_EquippedWeaponUI : bl_EquippedWeaponUIBase
{
    [SerializeField] private TextMeshProUGUI AmmoText;
    [SerializeField] private TextMeshProUGUI ClipText;
    [SerializeField] private TextMeshProUGUI FireTypeText;
    public Gradient AmmoTextColorGradient;

    /// <summary>
    /// 
    /// </summary>
    public override void SetAmmoOf(bl_Gun gun)
    {
        int bullets = gun.bulletsLeft;
        int clips = gun.RemainingClips;
        float per = (float)bullets / (float)gun.bulletsPerClip;
        Color c = AmmoTextColorGradient.Evaluate(per);

        if (gun.Info.Type != GunType.Knife)
        {
            AmmoText.text = bullets.ToString();
            if (gun.HaveInfinityAmmo)
                ClipText.text = "∞";
            else
                ClipText.text = ClipText.text = clips.ToString("F0");
            AmmoText.color = c;
            ClipText.color = c;
        }
        else
        {
            AmmoText.text = "--";
            ClipText.text = ClipText.text = "--";
            AmmoText.color = Color.white;
            ClipText.color = Color.white;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weapon"></param>
    public override void SetFireType(bl_GunBase.FireType fireType)
    {
        if (FireTypeText == null) return;

        string fireName = "--";
        switch (fireType)
        {
            case bl_GunBase.FireType.Auto:   fireName = bl_GameTexts.FireTypeAuto.Localized(45);   break;
            case bl_GunBase.FireType.Semi:   fireName = bl_GameTexts.FireTypeSemi.Localized(47);   break;
            case bl_GunBase.FireType.Single: fireName = bl_GameTexts.FireTypeSingle.Localized(46); break;
        }
        FireTypeText.text = fireName;
    }
}