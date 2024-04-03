using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MFPSEditor;
#endif

namespace MFPSEditor.Addons
{
    [CreateAssetMenu(fileName = "MFPS Addon", menuName = "MFPS/Addon Info", order = 300)]
    public class MFPSAddon : ScriptableObject
    {
        public string Name;
        public string Version;
        public string MinMFPSVersion = "1.6";

        [TextArea(4, 10)]
        public string Instructions;
        public string TutorialScript = "";
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(MFPSAddon))]
    public class MFPSAddonsEditor : Editor
    {
        MFPSAddon script;
        private GUIStyle TextStyle = null;
        private GUIStyle TextStyleFlat = null;
        private bool editMode = false;
        public TutorialWizardText contentText;

        private void OnEnable()
        {
            script = (MFPSAddon)target;
            TextStyle = Resources.Load<GUISkin>("content/MFPSEditorSkin").customStyles[3];
            TextStyleFlat = Resources.Load<GUISkin>("content/MFPSEditorSkin").customStyles[1];
            contentText = new TutorialWizardText();

            if (MFPSAddonsData.Instance != null)
            {
                int i = MFPSAddonsData.Instance.Addons.FindIndex(x => x.NiceName == script.Name);
                if (i >= 0 && MFPSAddonsData.Instance.Addons[i].Info == null)
                {
                    MFPSAddonsData.Instance.Addons[i].Info = script;
                    EditorUtility.SetDirty(MFPSAddonsData.Instance);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    LovattoStats.SetStat($"aa-{script.Name}", 1);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            Rect rect;
            Rect r = EditorGUILayout.BeginVertical();
            {
                rect = r;
                TutorialWizard.Style.DrawGlowRect(r, MFPSEditorStyles.LovattoEditorPalette.GetMainColor(true), Color.white);
                if (!editMode && !string.IsNullOrEmpty(script.Name))
                {
                    r = EditorGUILayout.BeginVertical();
                    {
                        TutorialWizard.Style.DrawGlowRect(r, MFPSEditorStyles.LovattoEditorPalette.GetBackgroundColor(true), Color.white);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"<size=30>{script.Name.ToUpper()}</size>", TextStyle);
                        GUILayout.FlexibleSpace();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(string.Format("<size=14>VERSION: <b>{0}</b></size>", script.Version), TextStyleFlat);
                        GUILayout.Space(10);
                        EditorGUILayout.LabelField(string.Format("<size=14>MIN MFPS: <b>{0}</b></size>", script.MinMFPSVersion), TextStyleFlat);
                        EditorGUILayout.EndHorizontal();
                        if (!string.IsNullOrEmpty(script.TutorialScript))
                        {
                            GUILayout.Space(5);
                            if (MFPSEditorStyles.ButtonOutline("DOCUMENTATION", MFPSEditorStyles.LovattoEditorPalette.GetHighlightColor(true)))
                            {
                                EditorWindow.GetWindow(System.Type.GetType(string.Format("{0}, Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", script.TutorialScript)));
                            }
                        }
                    }
                    GUILayout.Space(10);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);
                    if(!string.IsNullOrEmpty(script.Instructions))
                    contentText.DrawText(script.Instructions, TextStyleFlat);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.EndHorizontal();
                    DrawDefaultInspector();
                }
                GUILayout.Space(25);
                if (TutorialWizard.Buttons.GlowButton("ADDONS MANAGER", MFPSEditorStyles.LovattoEditorPalette.GetBackgroundColor(true), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    EditorWindow.GetWindow<MFPSAddonsWindow>().OpenAddonPage(script.Name);
                }
                GUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();

            rect.x += rect.width - 15;
            rect.y += 5;
            rect.width = 10; rect.height = 25;

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) editMode = !editMode;
            rect.width = 1;
            EditorGUI.DrawRect(rect, Color.gray);
        }
    }
#endif
}