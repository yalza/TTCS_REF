using UnityEngine;
using UnityEditor;

namespace UGUIMiniMap
{
    [CustomPropertyDrawer(typeof(CustomToggleAttribute))]
    public class CustomToggleDrawer : PropertyDrawer
    {
        CustomToggleAttribute att { get { return ((CustomToggleAttribute)attribute); } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.boolValue = EditorGUI.ToggleLeft(position, att.title, property.boolValue, EditorStyles.toolbarButton);
        }
    }
}