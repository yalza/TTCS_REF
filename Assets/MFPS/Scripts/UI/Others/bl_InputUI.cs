using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections;
using MFPS.Runtime.UI;

namespace MFPS.InputManager
{
    public class bl_InputUI : MonoBehaviour
    {
        [LovattoToogle] public bool autoHideOnMobile = true;
        public GameObject KeyBindingTemplate;
        public RectTransform ListPanel;
        public bl_ConfirmationWindow confirmationWindow;
        public GameObject RevertButton, ApplyButton;

        private bool m_isFetching = false;
        public bool isFetchingKey 
        {
            get => m_isFetching;
            set
            {
                m_isFetching = value;
                if(bl_RoomMenu.Instance != null)
                {
                    bl_RoomMenu.Instance.CurrentControlFocus = m_isFetching ? bl_RoomMenu.ControlFocus.InputBinding : bl_RoomMenu.ControlFocus.None;
                }
            }
        } 

        #region Private members
        private bool initialized = false;
        private bl_InputBindingUI currentFetchInput;
        private Array keyCodeList;
        private ButtonData pendingResetButton;
        private List<bl_InputBindingUI> cachedBindings = new List<bl_InputBindingUI>();
        private PendingButton pendingButton;
        private string confirmMessage; 
        #endregion

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            if (bl_UtilityHelper.isMobile && autoHideOnMobile)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator Start()
        {
            keyCodeList = Enum.GetValues(typeof(KeyCode));
            KeyBindingTemplate.SetActive(false);
            if (ApplyButton != null) ApplyButton.SetActive(false);
           if(RevertButton!= null) RevertButton.SetActive(false);

            while (!bl_GameData.isDataCached) { yield return null; }

            if (!initialized)
            {
                InstanceKeys();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            //if close the window without saving the changes
            if (ApplyButton != null && ApplyButton.activeSelf)
            {
                RevertChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void InstanceKeys()
        {
            bl_InputData.Instance.Initialize();
            if (bl_InputData.Instance.mappedInstance == null) { Debug.Log("Button Mapped has not been assigned in Input Manager."); return; }
            cachedBindings.ForEach(x => { Destroy(x.gameObject); });
            cachedBindings.Clear();
            List<ButtonData> buttons = bl_InputData.Instance.mappedInstance.ButtonMap;
            GameObject template;
            for (int i = 0; i < buttons.Count; i++)
            {
                template = Instantiate(KeyBindingTemplate) as GameObject;
                template.transform.SetParent(ListPanel, false);
                template.SetActive(true);
                bl_InputBindingUI script = template.GetComponent<bl_InputBindingUI>();
                script.Set(buttons[i], this);
                cachedBindings.Add(script);
            }
            initialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ChangeKeyFor(bl_InputBindingUI button)
        {
            if (isFetchingKey) return false;

            isFetchingKey = true;
            currentFetchInput = button;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            WaitForInput();
        }

        /// <summary>
        /// 
        /// </summary>
        void WaitForInput()
        {
            if (!isFetchingKey) return;

            //once when detect an input down
            if (Input.anyKeyDown)
            {
                foreach (KeyCode vKey in keyCodeList)
                {
                    if (Input.GetKeyDown(vKey))
                    {
                        if (vKey == KeyCode.Escape || vKey == KeyCode.JoystickButton6)//cancel
                        {
                            currentFetchInput.CancelChange();
                            isFetchingKey = false;
                            break;
                        }

                        pendingButton = new PendingButton()
                        {
                            isAKey = true,
                            Key = vKey,
                        };

                        if (isKeyUsed(vKey))//the key is already used, wait for change confirmation
                        {
                            confirmationWindow.AskConfirmation(confirmMessage, () => { ConfirmChange(true); }, () => { ConfirmChange(false); });
                            isFetchingKey = false;
                            break;
                        }

                        OnDetect();
                        break;
                    }
                }
            }

            ///Check for joystick axis
            for (int i = 0; i < GamePadButtonsNames.TriggerAxis.Length; i++)
            {
                if (Input.GetAxis(GamePadButtonsNames.TriggerAxis[i]) != 0)
                {
                    pendingButton = new PendingButton()
                    {
                        isAKey = false,
                        Axis = GamePadButtonsNames.TriggerAxis[i],
                    };
                    Debug.Log($"detected: {GamePadButtonsNames.TriggerAxis[i]} value: {Input.GetAxis(GamePadButtonsNames.TriggerAxis[i])}");
                    if (isAxisUsed(GamePadButtonsNames.TriggerAxis[i]))
                    {
                        confirmationWindow.AskConfirmation(confirmMessage, () => { ConfirmChange(true); }, () => { ConfirmChange(false); });
                        isFetchingKey = false;
                        break;
                    }

                    OnDetect();
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnDetect()
        {
            currentFetchInput.OnChanged(pendingButton);
            isFetchingKey = false;
            if (ApplyButton != null) ApplyButton.SetActive(true);
            if (RevertButton != null) RevertButton.SetActive(true);
        }

        /// <summary>
        /// Confirm change when the key is already defined in other key binding
        /// </summary>
        public void ConfirmChange(bool confirm)
        {
            if (confirm)
            {
                OnDetect();
                //reset the key where it was assigned before
                cachedBindings.ForEach(x =>
                {
                    if (x.CachedData.KeyName.Equals(pendingResetButton.KeyName))
                    {
                        if(pendingButton.isAKey)
                        x.ResetKey(pendingButton.Key);
                        else
                        x.ResetAxis(pendingButton.Axis);
                    }
                });
            }
            else
            {
                currentFetchInput.CancelChange();
                isFetchingKey = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ApplyChanges()
        {
            bl_InputData.Instance.SaveMappedInstance();
            if(ApplyButton != null)
            ApplyButton.SetActive(false);
            if (RevertButton != null) RevertButton.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RevertChanges()
        {
            bl_InputData.Instance.RevertMappedInstance();
            InstanceKeys();
            if (ApplyButton != null) ApplyButton.SetActive(false);
            if (RevertButton != null) RevertButton.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangeInputDevice(int id)
        {
            bl_InputData.Instance.ChangeMapped(id);
            InstanceKeys();
            SetActivePointer(bl_Input.isGamePad);
        }

        /// <summary>
        /// 
        /// </summary>
        void SetActivePointer(bool active)
        {
            if (active)
            {
                if(bl_GamePadPointer.Instance == null)
                {
                    bl_Input.CheckGamePadRequired();
                }
            }
            else
            {
                if (bl_GamePadPointer.Instance != null)
                {
                    Destroy(bl_GamePadPointer.Instance.gameObject);
                }
                if (bl_GamePadPointerModule.Instance != null)
                {
                    Destroy(bl_GamePadPointerModule.Instance.gameObject);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        bool isKeyUsed(KeyCode key)
        {
            foreach (var data in bl_InputData.Instance.mappedInstance.ButtonMap)
            {
                if (data.PrimaryKey == key || data.AlternativeKey == key)
                {
                    if (data.KeyName.Equals(currentFetchInput.CachedData.KeyName)) continue;//if this is the same button

                    string keyName = Regex.Replace(key.ToString(), "[A-Z]", " $0").Trim();
                    confirmMessage = $"The key <b>{keyName}</b> is already used for <b>{data.Description}</b>, you want to change it anyway?";
                    pendingResetButton = data;
                    return true;
                }
            }
            return false;
        }

        bool isAxisUsed(string axis)
        {
            foreach (var data in bl_InputData.Instance.mappedInstance.ButtonMap)
            {
                if (data.PrimaryAxis == axis)
                {
                    if (data.KeyName.Equals(currentFetchInput.CachedData.KeyName)) continue;//if this is the same button

                    confirmMessage = $"The key <b>{axis}</b> is already used for <b>{data.Description}</b>, you want to change it anyway?";
                    pendingResetButton = data;
                    return true;
                }
            }
            return false;
        }

        public class PendingButton
        {
            public string Axis;
            public KeyCode Key;
            public bool isPrimary = true;
            public bool isAKey = true;
        }
    }
}