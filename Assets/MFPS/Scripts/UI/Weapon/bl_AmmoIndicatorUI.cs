using UnityEngine;
using TMPro;

namespace MFPS.Runtime.UI
{
    /// <summary>
    /// Default MFPS Ammo Indicator handler
    /// The purpose of this script is to show a text that indicate of the current ammo state of the equipped weapon
    /// in order to let the player know if he need to reload, have low ammo remain or doesn't have any at all.
    /// This script is reference free, so if you want don't want to use or want to use your custom one, simply remove it from the UI instance.
    /// </summary>
    public class bl_AmmoIndicatorUI : MonoBehaviour
    {
        [LovattoToogle] public bool forceUpperCase = true;
        public TextMeshProUGUI indicatorText;

        private readonly Color m_warninColor = Color.yellow;
        private readonly Color m_alertColor = Color.red;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            SetActive(false);
            bl_EventHandler.onLocalPlayerAmmoUpdate += OnLocalAmmoChange;
            bl_EventHandler.onChangeWeapon += onWeaponChanged;
#if MFPS_VEHICLE
            bl_VehicleEvents.onLocalEnterInVehicle += OnEnterInVehicle;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.onLocalPlayerAmmoUpdate -= OnLocalAmmoChange;
            bl_EventHandler.onChangeWeapon -= onWeaponChanged;
#if MFPS_VEHICLE
            bl_VehicleEvents.onLocalEnterInVehicle -= OnEnterInVehicle;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalAmmoChange(int ammoCount)
        {
            var player = bl_MFPS.LocalPlayerReferences;
            if (player == null) return;

            var weapon = player.gunManager.GetCurrentWeapon();
            if (weapon == null) return;

            var type = weapon.Info.Type;
            if (type == GunType.Knife) { SetActive(false); return; }

            int bullets = weapon.bulletsLeft;
            if (bullets <= 0)
            {
                SetText(bl_GameTexts.OutOfAmmo);
                SetColor(m_alertColor);
                return;
            }

            //for these weapons type we only care to notify if there is no ammo
            if (type == GunType.Grenade || type == GunType.Launcher)
            {
                SetActive(false);
                return;
            }

            int max = weapon.bulletsPerClip;
            //if the magazine max bullets are is too little, don't bother to notify
            if (max <= 4) { SetActive(false); return; }

            int third = Mathf.FloorToInt(max / 3);
 

            //the weapon is just fine.
            if (bullets > third) { SetActive(false); return; }

            int five = Mathf.FloorToInt(max / 5);
            //the weapon have just 1/5 of the magazine capacity
            if (bullets <= third && bullets > five)
            {
                SetText(bl_GameTexts.Reload.ToUpper());
                SetColor(Color.white);
                return;
            }

            //only one possibility left = remaining bullets are 1/5 of the max ammo = low ammo
            SetText(bl_GameTexts.LowAmmo);
            SetColor(m_warninColor );
        }

        /// <summary>
        /// 
        /// </summary>
        void onWeaponChanged(int newWeapon)
        {
            OnLocalAmmoChange(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        private void SetText(string text)
        {
            if (indicatorText == null) return;
            if (forceUpperCase) text = text.ToUpper();
            indicatorText.text = text;
            SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        private void SetColor(Color color)
        {
            if (indicatorText == null) return;
            indicatorText.color = color;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            if (indicatorText == null) return;

            indicatorText.gameObject.SetActive(active);
        }

#if MFPS_VEHICLE
        /// <summary>
        /// 
        /// </summary>
        void OnEnterInVehicle(Vehicles.bl_VehicleManager vehicle)
        {
            SetActive(false);
        }
#endif
    }
}