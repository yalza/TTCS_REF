using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFPS.Runtime.Settings
{
    public class bl_RuntimeSettings : MonoBehaviour
    {
        public int defaultWindow = 0;
        public WindowUI[] windows;

        [Header("Settings")]
        public bool autoSaveSettings = false;
        public bool applySettingsOnChange = false;

        [Header("References")]
        public TextMeshProUGUI titleText;

        private int currentWindow = -1;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            ChangeWindow(defaultWindow);
        }

        /// <summary>
        /// Change the settings UI window
        /// </summary>
        public void ChangeWindow(int id)
        {
            if (currentWindow == id) return;

            foreach (var item in windows)
            {
                item?.SetActive(false);
                item.button.interactable = true;
            }
            windows[id].SetActive(true);

            if(titleText != null)
            {
                titleText.text = windows[id].Window.name.Localized(windows[id].Window.name.ToLower()).ToUpper();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void RevertAll()
        {
            var all = transform.GetComponentsInChildren<bl_SingleSettingsBinding>(true);
            foreach (var item in all)
            {
                item.Revert();
            }
            var alls = transform.GetComponentsInChildren<bl_SingleSettingsSlider>(true);
            foreach (var item in alls)
            {
                item.Revert();
            }

            var im = transform.GetComponentInChildren<InputManager.bl_InputUI>();
            if(im != null)
            {
                im.RevertChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ApplySettings()
        {
            bl_MFPS.Settings.SaveSettings();
            bl_MFPS.Settings.ApplySettings(bl_RuntimeSettingsProfile.ResolutionApplication.ApplyUserSelected);

            var im = transform.GetComponentInChildren<InputManager.bl_InputUI>();
            if (im != null)
            {
                im.ApplyChanges();
            }
        }

        [System.Serializable]
        public class WindowUI
        {
            public GameObject Window;
            public Button button;

            public void SetActive(bool active)
            {
                Window.SetActive(active);
                button.interactable = !active;
            }
        }

        private static bl_RuntimeSettings _instance = null;
        public static bl_RuntimeSettings Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_RuntimeSettings>(); }
                return _instance;
            }
        }
    }
}