using System.Collections.Generic;
using UnityEngine;
using MFPSEditor;

namespace MFPS.InputManager
{
    [CreateAssetMenu(fileName = "InputManager", menuName = "MFPS/Input/Manager")]
    public class bl_InputData : ScriptableObject
    {

        [SerializeField, ScriptableDrawer] private ButtonMapped Mapped = null;
        public string inputVersion = "1.0.0";
        [LovattoToogle] public bool useGamePadNavigation = false;
        [LovattoToogle] public bool runWithButton = false;

        [Header("References")]
        public GameObject GamePadInputModule;
        public GameObject GamePadPointerPrefab;

        [Header("Mapped Options")]
        public ButtonMapped[] mappedOptions;

        public ButtonMapped mappedInstance { get; set; }
        private Dictionary<string, ButtonData> cachedKeys = new Dictionary<string, ButtonData>();
        public const string KEYS = "mfps.input.bindings";

        public InputType inputType
        {
            get
            {
                if (Mapped != null) return Mapped.inputType;
                return InputType.Keyboard;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            cachedKeys.Clear();
            mappedInstance = null;
            LoadMapped();
        }

        /// <summary>
        /// 
        /// </summary>
        void LoadMapped()
        {
            if (PlayerPrefs.HasKey(MappedBindingKey))
            {
                string json = PlayerPrefs.GetString(MappedBindingKey);
                mappedInstance = Instantiate(Mapped);
                mappedInstance.mapped = JsonUtility.FromJson<Mapped>(json);
            }
            else
            {
                mappedInstance = Instantiate(Mapped);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangeMapped(int mappedID)
        {
            Mapped = mappedOptions[mappedID];
            Initialize();
            if(Mapped.inputType != InputType.Keyboard)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool GetButton(string key)
        {
            if (!IsCached(key)) return false;
            return cachedKeys[key].isButton();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool GetButtonDown(string key)
        {
            if (!IsCached(key)) return false;
            return cachedKeys[key].isButtonDown();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool GetButtonUp(string key)
        {
            if (!IsCached(key)) return false;
            return cachedKeys[key].isButtonUp();
        }

        /// <summary>
        /// Get the name of the primary button name
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetButtonName(string key)
        {
            if (!IsCached(key)) return "None";
            return cachedKeys[key].GetInputName();
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsCached(string key)
        {
            if(mappedInstance == null) { Initialize(); }
            if (!cachedKeys.ContainsKey(key))
            {
                int index = mappedInstance.ButtonMap.FindIndex(x => x.KeyName.Equals(key));
                if (index >= 0) { cachedKeys.Add(key, mappedInstance.ButtonMap[index]); }
                else
                {
                    Debug.Log($"Key <color=yellow>{key}</color> has not been mapped in the InputManager.");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SaveMappedInstance()
        {
            string json = JsonUtility.ToJson(mappedInstance.mapped);
            PlayerPrefs.SetString(MappedBindingKey, json);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RevertMappedInstance()
        {
            LoadMapped();
        }

        public string MappedBindingKey => Mapped == null ? KEYS : $"{KEYS}.{(short)Mapped.inputType}.{inputVersion}";
        public ButtonMapped DefaultMapped => Mapped;

        private static bl_InputData m_Data;
        public static bl_InputData Instance
        {
            get
            {
                if (m_Data == null)
                {
                    m_Data = Resources.Load("InputManager", typeof(bl_InputData)) as bl_InputData;
                }
                return m_Data;
            }
        }
    }
}