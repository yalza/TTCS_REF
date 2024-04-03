using UnityEngine;
using System.Text.RegularExpressions;
using TMPro;
using System.Collections.Generic;

namespace MFPS.InputManager
{
    public class bl_InputBindingUI : MonoBehaviour
    {
        public TextMeshProUGUI KeyNameText;
        public TextMeshProUGUI PrimaryKeyText;
        public TextMeshProUGUI AltKeyText;
        public GameObject PrimaryOverlay, AltOverlay;

        public ButtonData CachedData { get; set; }

        private bl_InputUI UIManager;
        private int waitingFor = 0;
        private Dictionary<KeyCode, string> overrideKeyNames = new Dictionary<KeyCode, string>()
        {
            { KeyCode.Mouse0, "Left Mouse" }, { KeyCode.Mouse1, "Right Mouse" }, { KeyCode.Mouse2, "Middle Mouse" }
        };

        /// <summary>
        /// 
        /// </summary>
        public void Set(ButtonData data, bl_InputUI uimanager)
        {
            CachedData = data;
            UIManager = uimanager;
            ApplyUI();
        }

        /// <summary>
        /// 
        /// </summary>
        void ApplyUI()
        {
#if LOCALIZATION
            string description = CachedData.KeyName;
            bl_Localization.Instance.TryGetText($"input_{CachedData.KeyName.ToLower()}", ref description);
            KeyNameText.text = description;
#else
            KeyNameText.text = CachedData.Description;
#endif

            if (!CachedData.PrimaryIsAxis)
            {
                string keyName = "None";
                if (overrideKeyNames.ContainsKey(CachedData.PrimaryKey))
                {
                    keyName = overrideKeyNames[CachedData.PrimaryKey];
                }
                else
                {
                    if (CachedData.PrimaryKey != KeyCode.None) keyName = Regex.Replace(CachedData.PrimaryKey.ToString(), "[A-Z]", " $0").Trim();
                }
                PrimaryKeyText.text = keyName;
            }
            else
                PrimaryKeyText.text = CachedData.PrimaryAxis;

            if (!CachedData.AlternativeIsAxis)
            {
                string keyName = "None";
                if (overrideKeyNames.ContainsKey(CachedData.AlternativeKey))
                {
                    keyName = overrideKeyNames[CachedData.AlternativeKey];
                }
                else
                {
                    if (CachedData.AlternativeKey != KeyCode.None) keyName = Regex.Replace(CachedData.AlternativeKey.ToString(), "[A-Z]", " $0").Trim();
                }

                AltKeyText.text = keyName;
            }
            else
                AltKeyText.text = CachedData.AlternativeAxis;


            PrimaryOverlay.SetActive(false);
            if(AltOverlay != null) AltOverlay.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnEdit(bool primary)
        {
            if (UIManager.ChangeKeyFor(this))
            {
                PrimaryOverlay.SetActive(primary);
                AltOverlay.SetActive(!primary);
                if (primary) { PrimaryKeyText.text = ""; }
                else { AltKeyText.text = ""; }
                waitingFor = primary ? 1 : 2;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnChanged(bl_InputUI.PendingButton button)
        {
            if (waitingFor == 1)
            {
                if (button.isAKey)
                {
                    CachedData.PrimaryIsAxis = false;
                    CachedData.PrimaryKey = button.Key;
                    CachedData.PrimaryAxis = "";
                }
                else
                {
                    CachedData.PrimaryIsAxis = true;
                    CachedData.PrimaryAxis = button.Axis;
                    CachedData.PrimaryKey = KeyCode.None;
                }
            }
            else if (waitingFor == 2)
            {
                if (button.isAKey)
                {
                    CachedData.AlternativeIsAxis = false;
                    CachedData.AlternativeKey = button.Key;
                    CachedData.AlternativeAxis = "";
                }
                else
                {
                    CachedData.AlternativeIsAxis = true;
                    CachedData.AlternativeAxis = button.Axis;
                    CachedData.AlternativeKey = KeyCode.None;
                }
            }

            ApplyUI();
            waitingFor = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CancelChange()
        {
            ApplyUI();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetKey(KeyCode key)
        {
            if(CachedData.PrimaryKey == key) { CachedData.PrimaryKey = KeyCode.None; }
            else if (CachedData.AlternativeKey == key) { CachedData.AlternativeKey = KeyCode.None; }
            ApplyUI();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetAxis(string axis)
        {
            if (string.IsNullOrEmpty(axis)) return;

            if (CachedData.PrimaryAxis == axis) { CachedData.PrimaryAxis = ""; }
            else if (CachedData.AlternativeAxis == axis) { CachedData.AlternativeAxis = ""; }
            ApplyUI();
        }
    }
}