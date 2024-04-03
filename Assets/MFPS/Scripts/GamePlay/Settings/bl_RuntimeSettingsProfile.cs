using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPS.Runtime.Settings
{
    [CreateAssetMenu(fileName = "Settings Profile", menuName = "MFPS/Settings/Profile")]
    public class bl_RuntimeSettingsProfile : ScriptableObject
    {
        public enum ResolutionApplication
        {
            NoApply,
            ApplyCurrent,
            ApplyUserSelected
        }

        [Reorderable]
        public List<SettingValue> settingValues = new List<SettingValue>();
        public int[] RefreshRates = new int[] { 30, 60, 120, 144, 200, 260, 0 };

        /// <summary>
        /// Get the save value of an specific settings
        /// or if it doesn't exist, get the default value
        /// </summary>
        public object GetSettingOf(string settingName)
        {
            int index = settingValues.FindIndex(x => x.Name.ToLower() == settingName.ToLower());

            if (index == -1) { Debug.LogWarning($"The setting {settingName} has not been defined in the current setting profile in GameData."); return null; }

            return settingValues[index].GetValue();
        }

        /// <summary>
        /// Save the value of an specific setting
        /// </summary>
        public void SetSettingOf(string settingName, object value, bool autoSave = false)
        {
            int index = settingValues.FindIndex(x => x.Name.ToLower() == settingName.ToLower());
            if (index == -1) { Debug.LogWarning($"The setting {settingName} has not been defined in the current setting profile in GameData."); return; }

            settingValues[index].SetSettingValue(value, autoSave);
        }

        /// <summary>
        /// Have a setting defined in the <see cref="settingValues"/> list?
        /// This doesn't check if there is a setting value saved for settings.
        /// </summary>
        /// <param name="settingName"></param>
        public bool HasSettingDefinedFor(string settingName)
        {
            return settingValues.Exists(x => x.Name.ToLower() == settingName.ToLower());
        }

        /// <summary>
        /// Save the current Settings
        /// </summary>
        public void SaveSettings()
        {
            foreach (var item in settingValues)
            {
                item.SaveValue();
            }
        }

        /// <summary>
        /// Apply the settings of this profile
        /// </summary>
        public void ApplySettings(ResolutionApplication resolutionApplication, bool fireCallback = true)
        {
            QualitySettings.SetQualityLevel((int)GetSettingOf("Quality"), true);
            QualitySettings.anisotropicFiltering = (AnisotropicFiltering)(int)GetSettingOf("Anisotropic");
            int antiA = (int)GetSettingOf("Antialiasing");
            QualitySettings.antiAliasing = antiA;
            AudioListener.pause = !(bool)GetSettingOf("Audio");
            AudioListener.volume = (float)GetSettingOf("Volume");

            bool antialising = antiA == 0 ? false : true;
            bl_EventHandler.SetEffectChange((bool)GetSettingOf("CrAberration"), antialising, (bool)GetSettingOf("Bloom"),
                (bool)GetSettingOf("SSAO"), (bool)GetSettingOf("MotionBlur"));

            // in webgl the resolution has to be changed in the web canvas, that require an external call integration not supported by default.
#if !UNITY_WEBGL
            if (!bl_UtilityHelper.isMobile)
            {
                ApplyResolutionSettings(resolutionApplication);
            }
#endif

            if (fireCallback)
                bl_EventHandler.DispatchGameSettingsChange();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ApplyResolutionSettings(ResolutionApplication resolutionApplication)
        {
            if (resolutionApplication == ResolutionApplication.NoApply) return;

            if (resolutionApplication == ResolutionApplication.ApplyCurrent) bl_ResolutionSettings.FetchResolutions();

            int displayModeID = (int)GetSettingOf("Display Mode");
            if (displayModeID != -1) // if not the default value
            {
                var currentResolution = Screen.currentResolution;
                if (resolutionApplication == ResolutionApplication.ApplyUserSelected)
                {
                    currentResolution = bl_ResolutionSettings.ResolutionHandler.GetRelativeResolution();
                }
                else if (resolutionApplication == ResolutionApplication.ApplyCurrent)
                {
                    if (bl_ResolutionSettings.ResolutionHandler != null)
                    {
                        currentResolution = bl_ResolutionSettings.ResolutionHandler.GetResolution();
                    }
                }

                Screen.SetResolution(currentResolution.width, currentResolution.height, (FullScreenMode)displayModeID);
            }
        }

        [ContextMenu("Delete Saved")]
        public void DeleteSavedSettings()
        {
            foreach (var item in settingValues)
            {
                item.DeleteSavedValue();
            }
        }

        [System.Serializable]
        public class SettingValue
        {
            public string Name;
            public ValueType valueType = ValueType.Integer;
            public string Value;

            private bool hasBeenModified = false;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public object GetValue()
            {
                string v = PlayerPrefs.GetString(SettingKey, Value);
                switch (valueType)
                {
                    case ValueType.Bool:
                        return v == "1" ? true : false;
                    case ValueType.Float:
                        float f = 0f;
                        v = v.Replace(",", ".");
                        if (float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out f))
                        {
                            return f;
                        }
                        else
                        {
                            Debug.LogWarning($"Can't parse the settings '{Name}' with the value: {v}");
                            return f;
                        }
                    case ValueType.Integer:
                        int iv = 0;
                        if (int.TryParse(v, out iv))
                        {
                            return iv;
                        }
                        else
                        {
                            Debug.LogWarning($"Can't parse the settings '{Name}' with the value: {v}");
                            return iv;
                        }
                    case ValueType.String:
                    default:
                        return v;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetSettingValue(object value, bool autoSave = false)
            {
                string v = "";
                if (value is bool) { v = (bool)value == true ? "1" : "0"; }
                else { v = value.ToString(); }
                Value = v;

                hasBeenModified = true;

                if (autoSave)
                {
                    SaveValue();
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void SaveValue()
            {
                if (!hasBeenModified) return;

                PlayerPrefs.SetString(SettingKey, Value);
                hasBeenModified = false;
            }

            public void DeleteSavedValue() => PlayerPrefs.DeleteKey(SettingKey);

            public string SettingKey => $"{Application.productName}.msetting.{Name}";

            [System.Serializable]
            public enum ValueType
            {
                Float = 0,
                String = 1,
                Bool = 2,
                Integer = 3,
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(SettingValue))]
        public class SettingValueDrawer : PropertyDrawer
        {
            // Draw the property inside the given rect
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                // Using BeginProperty / EndProperty on the parent property means that
                // prefab override logic works on the entire property.
                EditorGUI.BeginProperty(position, label, property);
                Rect r = position;
                r.height = EditorGUIUtility.singleLineHeight;
                // Draw label
                property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, label);
                r.y += EditorGUIUtility.singleLineHeight + 2;

                if (property.isExpanded)
                {

                    // Draw fields - passs GUIContent.none to each so they are drawn without labels
                    EditorGUI.PropertyField(r, property.FindPropertyRelative("Name"), new GUIContent("Name"));
                    r.y += EditorGUIUtility.singleLineHeight + 2;
                    EditorGUI.PropertyField(r, property.FindPropertyRelative("valueType"), new GUIContent("Value Type"));
                    r.y += EditorGUIUtility.singleLineHeight + 2;
                    var value = property.FindPropertyRelative("Value");
                    if (property.FindPropertyRelative("valueType").intValue == 2)
                    {
                        bool bv = value.stringValue == "1" ? true : false;
                        bv = EditorGUI.ToggleLeft(r, "Value", bv, EditorStyles.toolbar);
                        value.stringValue = bv == true ? "1" : "0";
                    }
                    else
                        EditorGUI.PropertyField(r, value, new GUIContent("Value"));
                }

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                if (property.isExpanded)
                {
                    return EditorGUI.GetPropertyHeight(property);
                }
                else return EditorGUIUtility.singleLineHeight;
            }
        }
#endif
    }
}