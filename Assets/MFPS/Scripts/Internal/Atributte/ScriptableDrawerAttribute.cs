using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPSEditor
{
    public class ScriptableDrawerAttribute : PropertyAttribute
    {
        public ScriptableDrawerAttribute()
        {
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ScriptableDrawerAttribute))]
    public class ScriptableDrawerAttributeDrawer : PropertyDrawer
    {
        private Editor cachedEditor;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();
            if (property.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(position, property);
            }
            else
            {
                var name = property.displayName;
                if (name.Contains("Element"))
                {
                    name = property.objectReferenceValue.name;
                }
                property.isExpanded = MFPSEditorStyles.ContainerHeaderFoldout(name, property.isExpanded);
                if (property.isExpanded)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(property);
                    DrawEditorOf(property);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (property.serializedObject != null)
                {
                    property.serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        void DrawEditorOf(SerializedProperty so)
        {
            if (so == null || so.objectReferenceValue == null) return;

            var editor = cachedEditor;
            if (editor == null)
            {
                editor = cachedEditor = Editor.CreateEditor(so.objectReferenceValue);
            }
            if (editor != null)
            {
                EditorGUILayout.BeginVertical("box");
                editor.OnInspectorGUI();
                EditorGUILayout.EndVertical();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null)
                return EditorGUIUtility.singleLineHeight;
            else return 0;
        }
    }
#endif
}