using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

[CustomEditor(typeof(bl_NamePlateDrawer))]
public class bl_NamePlateEditor : Editor
{
    bl_NamePlateDrawer script;
    public bool isSimulating = false;
    public int simulatedHealth = 100;
    public string simulatedName = "Lovatto";
    public bool editPresent = false;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        script = (bl_NamePlateDrawer)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        using (new EditorGUILayout.VerticalScope("box"))
        {
            DrawDefaultInspector();
        }

        EditorGUILayout.BeginVertical("box");
        GUI.enabled = script.StylePresent != null;
        if (GUILayout.Button("Edit Present"))
        {
            editPresent = !editPresent;
        }
        if (editPresent)
        {
            var e = Editor.CreateEditor(script.StylePresent);
            if (e != null)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    e.DrawDefaultInspector();
                }
            }
        }
        string isOn = isSimulating ? "ON" : "OFF";
        GUI.enabled = !Application.isPlaying && script.StylePresent != null;
        if (GUILayout.Button($"Simulate [{isOn}]"))
        {
            isSimulating = !isSimulating;
            ActiveEditorTracker.sharedTracker.isLocked = isSimulating;
        }
        if (isSimulating)
        {
            simulatedHealth = EditorGUILayout.IntSlider("Simulated Health", simulatedHealth, 0, 100);
            simulatedName = EditorGUILayout.TextField("Simulate Name", simulatedName);
        }
        GUI.enabled = true;
        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    private void OnSceneGUI()
    {
        if (!isSimulating) return;
        if (script.StylePresent == null) return;
        if (script == null) { script = (bl_NamePlateDrawer)target; }
        if (script.positionReference == null || SceneView.lastActiveSceneView == null) return;

        Camera sc = SceneView.lastActiveSceneView.camera;
        if (sc == null) return;

        float distance = bl_UtilityHelper.Distance(sc.transform.position, script.positionReference.position);
        Handles.BeginGUI();
        Vector3 vector = sc.WorldToScreenPoint(script.positionReference.position);
        if (vector.z > 0)
        {
            float distanceDifference = Mathf.Clamp(distance - 0.1f, 1, 12);
            vector.y += distanceDifference * (script.distanceModifier + 3.3f);

            int vertical = bl_GameData.Instance.ShowTeamMateHealthBar ? 15 : 10;
            if (distance < script.hideDistance)
            {
                GUI.Label(new Rect(vector.x - 5, (Screen.height - vector.y) - vertical, 10, 11), simulatedName, script.StylePresent.style);
                if (bl_GameData.Instance.ShowTeamMateHealthBar)
                {
                    float mh = 100;
                    float h = simulatedHealth;
                    GUI.color = script.StylePresent.HealthBackColor;
                    GUI.DrawTexture(new Rect(vector.x - (mh / 2), (Screen.height - vector.y), mh, script.StylePresent.HealthBarThickness), script.StylePresent.HealthBarTexture);
                    GUI.color = script.StylePresent.HealthBarColor;
                    GUI.DrawTexture(new Rect(vector.x - (mh / 2), (Screen.height - vector.y), h, script.StylePresent.HealthBarThickness), script.StylePresent.HealthBarTexture);
                    GUI.color = Color.white;
                }
            }
            else
            {
                float iconsSize = script.StylePresent.IndicatorIconSize;
                GUI.DrawTexture(new Rect(vector.x - (iconsSize / 2), (Screen.height - vector.y) - (iconsSize / 2), iconsSize, iconsSize), script.StylePresent.IndicatorIcon);
            }
        }
        Handles.EndGUI();
    }
}