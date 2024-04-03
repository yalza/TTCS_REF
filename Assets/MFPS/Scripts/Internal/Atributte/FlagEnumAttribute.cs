using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPS.Internal
{
    public class FlagEnumAttribute : PropertyAttribute
    {
        public FlagEnumAttribute() { }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FlagEnumAttribute))]
    public class FlagEnumAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            _property.intValue = EditorGUI.MaskField(_position, _label, _property.intValue, _property.enumNames);
        }
    }
#endif
}