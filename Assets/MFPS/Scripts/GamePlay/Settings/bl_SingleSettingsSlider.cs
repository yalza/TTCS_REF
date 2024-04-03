using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFPS.Runtime.Settings
{
    public class bl_SingleSettingsSlider : MonoBehaviour
    {
        [Header("Settings")]
        public string SettingKeyName = "";
        public string valueFormat = "{0}";
        public float displayMultiplier = 1;

        [Header("References")]
        public Slider slider;
        public Text valueText;
        public TextMeshProUGUI valueTextTMP;

        public float currentValue { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        IEnumerator Start()
        {
            while (!bl_GameData.isDataCached) yield return null;
            Load();
        }

        /// <summary>
        /// 
        /// </summary>
        void Load()
        {
            currentValue = (float)bl_MFPS.Settings.GetSettingOf(SettingKeyName);
            slider.value = currentValue;
            ApplyCurrentValue();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ApplyCurrentValue()
        {
            if (valueText != null)
                valueText.text = string.Format(valueFormat, (slider.value * displayMultiplier));
            if(valueTextTMP != null) valueTextTMP.text = string.Format(valueFormat, (slider.value * displayMultiplier));
            ApplySetting();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ApplySetting()
        {
            //Set the changed setting value to the instance settings group.
            bl_MFPS.Settings.SetSettingOf(SettingKeyName, currentValue, bl_RuntimeSettings.Instance.autoSaveSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateValue(float value)
        {
            currentValue = value;
            ApplyCurrentValue();
        }

        /// <summary>
        /// 
        /// </summary>
        public void SaveValue()
        {
            bl_MFPS.Settings.SaveSettings();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Revert()
        {
            Load();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (slider == null) return;

            if (valueText != null)
                valueText.text = string.Format(valueFormat, (slider.value * displayMultiplier).ToString("0.0"));
            if (valueTextTMP != null)
                valueTextTMP.text = string.Format(valueFormat, (slider.value * displayMultiplier).ToString("0.0"));
        }
#endif
    }
}