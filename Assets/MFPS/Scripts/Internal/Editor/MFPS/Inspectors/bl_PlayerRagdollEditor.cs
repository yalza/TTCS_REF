using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(bl_PlayerRagdoll))]
public class bl_PlayerRagdollEditor : Editor
{
    bl_PlayerRagdoll script;

    public override void OnInspectorGUI()
    {
        script = (bl_PlayerRagdoll)target;
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginVertical("box");
        EditorGUI.indentLevel++;
        base.OnInspectorGUI();
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        if (GUILayout.Button("Refresh"))
        {
            script.SetUpHitBoxes();
        }
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

}