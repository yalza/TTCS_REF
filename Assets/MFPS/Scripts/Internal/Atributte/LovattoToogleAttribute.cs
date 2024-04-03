using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MFPSEditor;
#endif

public class LovattoToogleAttribute : PropertyAttribute
{
    public readonly string title;
    public readonly float ExtraWidth = 0;

    public LovattoToogleAttribute(float extraWidth = 0)
    {
        title = string.Empty;
        ExtraWidth = extraWidth;
    }

    public LovattoToogleAttribute(string toggleTitle)
    {
        title = toggleTitle;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(LovattoToogleAttribute))]
public class LovattoToogleAttributteDrawer : PropertyDrawer
{
    LovattoToogleAttribute script { get { return ((LovattoToogleAttribute)attribute); } }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string t = script.title;
        if (string.IsNullOrEmpty(script.title)) { t = property.displayName; }
        float lw = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth += script.ExtraWidth;
        position.x += 15 * EditorGUI.indentLevel;
        property.boolValue = MFPSEditorStyles.FeatureToogle(position, property.boolValue, t);
        EditorGUIUtility.labelWidth = lw;
    }
}
#endif