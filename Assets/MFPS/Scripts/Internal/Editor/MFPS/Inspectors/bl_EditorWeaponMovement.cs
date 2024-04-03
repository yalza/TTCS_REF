using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(bl_WeaponMovements))]
public class bl_EditorWeaponMovement : Editor
{
    private bool isRecording = false;
    Vector3 defaultPosition = Vector3.zero;
    Quaternion defaultRotation = Quaternion.identity;
    bl_WeaponMovements script;
    SerializedProperty previewProp;
    public bool isPreviwing = false;

    private void OnEnable()
    {
        previewProp = serializedObject.FindProperty("_previewWeight");
        previewProp.isExpanded = false;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        script = (bl_WeaponMovements)target;

        EditorGUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal("box");
        Color c = isRecording ? Color.red : Color.white;
        GUI.color = c;
        if (GUILayout.Button(new GUIContent(" Edit",EditorGUIUtility.IconContent("d_EditCollider").image), EditorStyles.toolbarButton))
        {
            isRecording = !isRecording;
            if (isRecording)
            {
                defaultPosition = script.transform.localPosition;
                defaultRotation = script.transform.localRotation;
                if (script.moveTo != Vector3.zero)
                {
                    script.transform.localPosition = script.moveTo;
                    script.transform.localRotation = Quaternion.Euler(script.rotateTo);
                }
                Tools.current = Tool.Transform;
                ActiveEditorTracker.sharedTracker.isLocked = true;
                isPreviwing = false;
            }
            else
            {
                script.transform.localPosition = defaultPosition;
                script.transform.localRotation = defaultRotation;
                ActiveEditorTracker.sharedTracker.isLocked = false;
                EditorUtility.SetDirty(target);
            }
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.Label("On Run weapon position", EditorStyles.helpBox);
        script.moveTo = EditorGUILayout.Vector3Field("Position", script.moveTo);
        script.rotateTo = EditorGUILayout.Vector3Field("Rotation", script.rotateTo);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Get Actual Position", EditorStyles.toolbarButton))
        {
            script.moveTo = script.transform.localPosition;
            script.rotateTo = script.transform.localEulerAngles;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("On Run and Reload weapon position", EditorStyles.helpBox);
        script.moveToReload = EditorGUILayout.Vector3Field("Position", script.moveToReload);
        script.rotateToReload = EditorGUILayout.Vector3Field("Rotation", script.rotateToReload);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Get Actual Position", EditorStyles.toolbarButton))
        {
            script.moveToReload = script.transform.localPosition;
            script.rotateToReload = script.transform.localRotation.eulerAngles;
        }
        if (GUILayout.Button("Copy", EditorStyles.toolbarButton))
        {
            script.moveToReload = script.moveTo;
            script.rotateToReload = script.rotateTo;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        script.InSpeed = EditorGUILayout.Slider("In Speed", script.InSpeed, 1, 25);
        script.OutSpeed = EditorGUILayout.Slider("Out Speed", script.OutSpeed, 1, 25);
        script.rotationSpeedMultiplier = EditorGUILayout.Slider("Rotation Speed Multiplier", script.rotationSpeedMultiplier, 0.1f, 3);
        script.accelerationMultiplier = EditorGUILayout.Slider("Acceleration Multiplier", script.accelerationMultiplier, 0.1f, 7);
        script.accelerationCurve = EditorGUILayout.CurveField("Acceleration Curve", script.accelerationCurve);
        GUILayout.EndVertical();

        if (!isRecording)
        {
            var l = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            GUILayout.BeginVertical("box");
            previewProp.isExpanded = EditorGUILayout.Foldout(previewProp.isExpanded, "Preview Movement");
            if(isPreviwing != previewProp.isExpanded)
            {
                defaultPosition = script.transform.localPosition;
                defaultRotation = script.transform.localRotation;
                script._previewWeight = 0;
                isPreviwing = previewProp.isExpanded;
            }
            if (previewProp.isExpanded)
            {
                EditorGUI.BeginChangeCheck();
                script._previewWeight = EditorGUILayout.Slider("Weight", script._previewWeight, 0, 1);
                if (EditorGUI.EndChangeCheck() && !Application.isPlaying)
                {
                    script.transform.localPosition = Vector3.Lerp(defaultPosition, script.moveTo, script._previewWeight);
                    script.transform.localRotation = Quaternion.Slerp(defaultRotation, Quaternion.Euler(script.rotateTo), script._previewWeight);
                }
            }
            GUILayout.EndVertical();
            EditorGUI.indentLevel = l;
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    Vector3 CalculateCenter()
    {
        var renderers = script.transform.GetComponentsInChildren<Renderer>();
        Bounds b = new Bounds(renderers[0].bounds.center, renderers[0].bounds.size);
        foreach (var r in renderers)
        {
            if (r.GetComponent<ParticleSystem>() != null) continue;
            if (b.extents == Vector3.zero)
                b = r.bounds;

            b.Encapsulate(r.bounds);
        }
       return  b.center;
    }
}