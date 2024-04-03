using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace MFPS.InputManager
{
    [Serializable]
    public class Mapped
    {
        public List<ButtonData> ButtonMap = new List<ButtonData>();
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Mapped))]
    public class MappedDrawer : PropertyDrawer
    {
        private Dictionary<string, ReorderableList> _reorderableLists = new Dictionary<string, ReorderableList>();

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.IndentedRect(position);

            _reorderableLists[property.propertyPath].DoList(position);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty myDataList = property.FindPropertyRelative("ButtonMap");
            if (!_reorderableLists.ContainsKey(property.propertyPath) || _reorderableLists[property.propertyPath].index > _reorderableLists[property.propertyPath].count - 1)
            {
                _reorderableLists[property.propertyPath] = new ReorderableList(myDataList.serializedObject, myDataList, true, true, true, true);
                _reorderableLists[property.propertyPath].drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel++;
                    EditorGUI.PropertyField(rect, myDataList.GetArrayElementAtIndex(index), true);
                    EditorGUI.indentLevel = indent;
                };
                _reorderableLists[property.propertyPath].elementHeightCallback += (int index) =>
                {
                    var element = myDataList.GetArrayElementAtIndex(index);
                    if (element.isExpanded) return EditorGUI.GetPropertyHeight(element);
                    else return EditorGUIUtility.singleLineHeight;
                };
                _reorderableLists[property.propertyPath].drawHeaderCallback += (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Buttons Map");
                };
            }
            return _reorderableLists[property.propertyPath].GetHeight();
        }
    }
#endif
}