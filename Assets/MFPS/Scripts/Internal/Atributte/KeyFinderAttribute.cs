using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using MFPSEditor;
#endif

public class KeyFinderAttribute : PropertyAttribute
{
    public KeyFinderAttribute()
    {    
    }

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(KeyFinderAttribute))]
public class KeyFinderAttributeDrawer : PropertyDrawer
{
    KeyFinderAttribute script { get { return ((KeyFinderAttribute)attribute); } }
    public string findValue;

    private string[] keyNames;
    private Vector2 scroll = Vector2.zero;
    public List<KeyMatches> matches = new List<KeyMatches>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect p = position;
        p.width -= 50;
        p.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(p, property);
     //   property.enumValueIndex = (int)(KeyCode)EditorGUI.EnumPopup(p, property.displayName, (KeyCode)property.enumValueIndex, EditorStyles.toolbarPopup);

        p.x += position.width - 50;
        p.width = 50;
        if (GUI.Button(p, "Find", EditorStyles.toolbarButton))
        {
            property.isExpanded = !property.isExpanded;
            findValue = "";
            matches.Clear();
        }
        if (property.isExpanded)
        {
            p = position;
            p.height = EditorGUIUtility.singleLineHeight;
            p.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.BeginChangeCheck();
            findValue = EditorGUI.TextField(p, "Search", findValue);
            if (EditorGUI.EndChangeCheck())
            {
                FindMatches();
            }
            if (!string.IsNullOrEmpty(findValue))
            {
                p = position;
                p.y += EditorGUIUtility.singleLineHeight + 2;
                p.height = EditorGUIUtility.singleLineHeight;
                p.x += EditorGUIUtility.labelWidth;
                p.width -= EditorGUIUtility.labelWidth;
                for (int i = 0; i < matches.Count; i++)
                {
                    p.y += EditorGUIUtility.singleLineHeight;
                    if (GUI.Button(p, matches[i].Name, EditorStyles.helpBox))
                    {
                        property.intValue = (int)(KeyCode)System.Enum.Parse(typeof(KeyCode), matches[i].Name);
                        property.isExpanded = false;
                        findValue = "";
                        matches.Clear();
                        GUI.FocusControl("");
                    }
                }
            }
        }
    }

    void FindMatches()
    {
        matches.Clear();
        if (keyNames == null)
        {
            keyNames = System.Enum.GetNames(typeof(KeyCode));
        }

        for (int i = 0; i < keyNames.Length; i++)
        {
            string k = keyNames[i];
            if (k.Length < findValue.Length) continue;
            if (!k.ToLower().StartsWith(findValue.ToLower())) continue;

            matches.Add(new KeyMatches()
            {
                Name = keyNames[i],
                ID = i
            });
        }
    }

    public class KeyMatches
    {
        public string Name;
        public int ID;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded) return (EditorGUIUtility.singleLineHeight * 2 + 2) + (matches.Count * (EditorGUIUtility.singleLineHeight));
        else return EditorGUIUtility.singleLineHeight + 2;
    }
}
#endif