using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This attribute can only be applied to fields because its
/// associated PropertyDrawer only operates on fields (either
/// public or tagged with the [SerializeField] attribute) in
/// the target MonoBehaviour.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Field)]
public class GunIDAttribute : PropertyAttribute
{
    public GunIDAttribute()
    {

    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(GunIDAttribute))]
public class InspectorButtonPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        prop.intValue = EditorGUI.Popup(position, prop.displayName, prop.intValue, bl_GameData.Instance.AllWeaponStringList());
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight + 2;
    }
}
#endif