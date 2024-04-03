using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MFPS.InputManager
{
    [CustomEditor(typeof(ButtonMapped))]
    public class InputMappedEditor : Editor
    {
        ButtonMapped script;

        private void OnEnable()
        {
            script = (ButtonMapped)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(10);
            base.OnInspectorGUI();
            GUILayout.Space(10);
            string key = $"{bl_InputData.KEYS}.{(short)script.inputType}";
            if (PlayerPrefs.HasKey(key))
            {
                if(GUILayout.Button("Delete save input binding"))
                {
                    PlayerPrefs.DeleteKey(key);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);

                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                }
            }
        }
    }
}