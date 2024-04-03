using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;

[CustomEditor(typeof(bl_AIAnimation))]
public class bl_AIAnimationEditor : Editor
{
    bl_AIAnimation script;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        script = (bl_AIAnimation)target;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        GUILayout.Space(10);
        if(GUILayout.Button("Refresh Rigidbody list"))
        {
            script.GetRigidBodys();
        }
        if (GUILayout.Button("Run Animator"))
        {
            var window = (AnimatorRunner)EditorWindow.GetWindow(typeof(AnimatorRunner));
            window.Show();
            Animator anim = script.gameObject.GetComponent<Animator>();
            window.SetAnim(anim, null, true);
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}