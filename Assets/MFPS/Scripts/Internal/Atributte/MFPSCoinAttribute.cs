using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPSEditor
{
    public class MFPSCoinIDAttribute : PropertyAttribute
    {
        public MFPSCoinIDAttribute()
        {
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MFPSCoinIDAttribute))]
    public class MFPSCoinIDAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.hasMultipleDifferentValues)
            {
                EditorGUI.PropertyField(position, property);
            }
            else
            {
                var names = bl_GameData.Instance.gameCoins.Select(x =>
                {
                    if (x == null) return "Empty";

                    return x.CoinName;
                }).ToArray();
                property.intValue = EditorGUI.Popup(position, property.displayName, property.intValue, names);
            }
            EditorGUI.EndProperty();
        }
    }
#endif
}