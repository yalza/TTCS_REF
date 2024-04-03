using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFPS.Runtime.UI
{
    public class bl_BarracksWindow : MonoBehaviour
    {
        [Header("Windows")]
        public string firstWindow = "profile";
        public List<MenuWindow> windows;

        [Header("References")]
        [SerializeField] GameObject content = null;
        [SerializeField] TextMeshProUGUI windowTitleText = null;

        public int CurrentWindow { get; set; } = -1;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            InitialSetup();
            OpenWindow(firstWindow);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitialSetup()
        {
            foreach (var window in windows)
            {
                if (!window.Active)
                {
                    if (window.OpenButton != null) window.OpenButton.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        public void OpenWindow(string windowName)
        {
            var id = windows.FindIndex(x => x.Name == windowName);
            if (id == -1)
            {
                Debug.LogWarning($"Pause window {windowName} doesn't exist.");
                return;
            }
            OpenWindow(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        public void OpenWindow(int windowIndex)
        {
            if (windowIndex == CurrentWindow) return;

            content.SetActive(true);
            CurrentWindow = windowIndex;

            windows.ForEach(x => x.SetActive(false));

            var window = windows[CurrentWindow];
            window.SetActive(true);

            if (windowTitleText != null && window.OpenButton != null)
            {
                windowTitleText.text = window.OpenButton.GetComponentInChildren<TextMeshProUGUI>().text.ToUpper();
            }
        }

        [Serializable]
        public class MenuWindow
        {
            public string Name;
            public GameObject Window;
            public Button OpenButton;
            [LovattoToogle] public bool Active = true;
            public bl_EventHandler.UEvent onOpen;

            public void SetActive(bool active)
            {
                if (Window != null) Window.SetActive(active);
                if (OpenButton != null) OpenButton.interactable = !active;
                onOpen?.Invoke();
            }
        }
    }
}