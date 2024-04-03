using UnityEngine;
using MFPS.Runtime.Settings;

namespace MFPS.InputManager
{
    public class bl_InputDeviceSwitcher : MonoBehaviour
    {
        private bool init = false;
        private bl_SingleSettingsBinding settingsBinding;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            settingsBinding = GetComponent<bl_SingleSettingsBinding>();           
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            InitialSetup();
        }

        /// <summary>
        /// 
        /// </summary>
        void InitialSetup()
        {
            if (init) return;

            InputType currentType = bl_InputData.Instance.mappedInstance.inputType;
            int id = 0; // keyboard
            if(currentType == InputType.Xbox || currentType == InputType.Playstation)
            {
                id = 1;
            }

            settingsBinding.currentOption = id;
            settingsBinding.ApplyCurrentValue();
            init = true;
        }
    }
}