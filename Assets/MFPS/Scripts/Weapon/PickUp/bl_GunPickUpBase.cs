using System;
using UnityEngine.Serialization;

public abstract class bl_GunPickUpBase : bl_MonoBehaviour
{
    /// <summary>
    /// Ammunition information
    /// </summary>
    [Serializable]
    public class AmmoInformation
    {
        public int Bullets = 0;
        public int Clips = 0;

        public int GetBullets
        {
            get
            {
                int b = Bullets;
                if (bl_GameData.Instance.AmmoType == AmmunitionType.Bullets)
                {
                    b = Bullets * Clips;
                }
                return b;
            }
        }
    }

    /// <summary>
    /// Identifier of the weapon
    /// </summary>
    [GunID] public int GunID = 0;

    /// <summary>
    /// Ammunition data of this dropped weapon
    /// </summary>
    [FormerlySerializedAs("Info")] 
    public AmmoInformation Ammunition;

    /// <summary>
    /// This weapon will be automatically be destroyed
    /// after certain time?
    /// </summary>
    public bool AutoDestroy = true;

    /// <summary>
    /// 
    /// </summary>
    public bl_GunInfo GunInfo
    {
        get
        {
            if (_gunInfo == null)
            {
                _gunInfo = bl_GameData.Instance.GetWeapon(GunID);
            }
            return _gunInfo;
        }
        private set => _gunInfo = value;
    }
    private bl_GunInfo _gunInfo;

    /// <summary>
    /// Pick up the weapon
    /// </summary>
    public abstract void PickUp();

    /// <summary>
    /// Destroy this weapon instance after a delay
    /// </summary>
    /// <param name="delay"></param>
    public abstract void DestroyAfter(float delay);
}