using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using UnityEditor.AnimatedValues;
using Photon.Pun;

[CustomEditor(typeof(bl_GunManager))]
public class bl_GunManagerEditor : Editor
{
    private AnimBool AssaultAnim;
    protected static bool ShowAssault;
    private AnimBool EnginnerAnim;
    protected static bool ShowEngi;
    private AnimBool ReconAnim;
    protected static bool ShowRecon;
    private AnimBool SupportAnim;
    protected static bool ShowSupport;

    private void OnEnable()
    {
        bl_GunManager script = (bl_GunManager)target;

        AssaultAnim = new AnimBool(ShowAssault);
        AssaultAnim.valueChanged.AddListener(Repaint);
        EnginnerAnim = new AnimBool(ShowEngi);
        EnginnerAnim.valueChanged.AddListener(Repaint);
        ReconAnim = new AnimBool(ShowRecon);
        ReconAnim.valueChanged.AddListener(Repaint);
        SupportAnim = new AnimBool(ShowSupport);
        SupportAnim.valueChanged.AddListener(Repaint);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        bl_GunManager script = (bl_GunManager)target;
        bool allowSceneObjects = !EditorUtility.IsPersistent(script);

        EditorGUILayout.BeginVertical("box");
        DrawNetworkGunsList(script);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        string[] weaponList = bl_GameData.Instance.AllWeaponStringList();

#if CLASS_CUSTOMIZER
        GUILayout.BeginHorizontal();
        GUILayout.Label("Class Customization is enabled, set default weapons here: ", EditorStyles.miniLabel);
        if (GUILayout.Button("ClassManager")) { Selection.activeObject = bl_ClassManager.Instance; EditorGUIUtility.PingObject(bl_ClassManager.Instance); }
        GUILayout.EndHorizontal();
        GUI.enabled = false;
#endif
        EditorGUILayout.BeginVertical("box");
        ShowAssault = PhotonGUI.ContainerHeaderFoldout("Assault Class", ShowAssault);
        AssaultAnim.target = ShowAssault;
        if (EditorGUILayout.BeginFadeGroup(AssaultAnim.faded))
        {
            var so = serializedObject.FindProperty("m_AssaultClass");
            DrawLoadoutField(so);
            DrawEditorOf(so);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        ShowEngi = PhotonGUI.ContainerHeaderFoldout("Engineer Class", ShowEngi);
        EnginnerAnim.target = ShowEngi;
        if (EditorGUILayout.BeginFadeGroup(EnginnerAnim.faded))
        {
            var so = serializedObject.FindProperty("m_EngineerClass");
            DrawLoadoutField(so);
            DrawEditorOf(so);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        ShowRecon = PhotonGUI.ContainerHeaderFoldout("Recon Class", ShowRecon);
        ReconAnim.target = ShowRecon;
        if (EditorGUILayout.BeginFadeGroup(ReconAnim.faded))
        {
            var so = serializedObject.FindProperty("m_ReconClass");
            DrawLoadoutField(so);
            DrawEditorOf(so);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        ShowSupport = PhotonGUI.ContainerHeaderFoldout("Support Class", ShowSupport);
        SupportAnim.target = ShowSupport;
        if (EditorGUILayout.BeginFadeGroup(SupportAnim.faded))
        {
            var so = serializedObject.FindProperty("m_SupportClass");
            DrawLoadoutField(so);
            DrawEditorOf(so);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();
        GUI.enabled = true;

        GUILayout.BeginVertical("box");
        if (script.changeWeaponStyle == bl_GunManager.ChangeWeaponStyle.HideAndDraw)
        {
            script.SwichTime = EditorGUILayout.Slider("Switch Time", script.SwichTime, 0.1f, 5);
        }
        script.PickUpTime = EditorGUILayout.Slider("Pick Up Time", script.PickUpTime, 0.1f, 5);
        script.changeWeaponStyle = (bl_GunManager.ChangeWeaponStyle)EditorGUILayout.EnumPopup("Change Weapon Style", script.changeWeaponStyle, EditorStyles.toolbarPopup);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        script.HeadAnimator = EditorGUILayout.ObjectField("Head Animator", script.HeadAnimator, typeof(Animator), allowSceneObjects) as Animator;
        script.SwitchFireAudioClip = EditorGUILayout.ObjectField("Switch Fire Mode Audio", script.SwitchFireAudioClip, typeof(AudioClip), allowSceneObjects) as AudioClip;
        GUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    void DrawEditorOf(SerializedProperty so)
    {
        if (so == null || so.objectReferenceValue == null) return;

        var editor = Editor.CreateEditor(so.objectReferenceValue);
        if (editor != null)
        {
            EditorGUILayout.BeginVertical("box");
            editor.DrawDefaultInspector();
            EditorGUILayout.EndVertical();
        }
    }

    void DrawLoadoutField(SerializedProperty so)
    {
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.PropertyField(so);
        if (so.objectReferenceValue == null)
        {
            if (GUILayout.Button("Create", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                string path = "Assets/MFPS/Content/Prefabs/Weapons/Loadouts";
                if (!AssetDatabase.IsValidFolder(path))
                {
                    path = EditorUtility.OpenFolderPanel("Save Folder", "Assets", "Assets");
                }
                path = bl_UtilityHelper.CreateAsset<bl_PlayerClassLoadout>(path, false, "Player Class Loadout");
                bl_PlayerClassLoadout pcl = AssetDatabase.LoadAssetAtPath(path, typeof(bl_PlayerClassLoadout)) as bl_PlayerClassLoadout;
                so.objectReferenceValue = pcl;
                EditorUtility.SetDirty(target);
            }
        }
        else
        {
            if (GUILayout.Button("NEW", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                bl_PlayerClassLoadout old = so.objectReferenceValue as bl_PlayerClassLoadout;
                string path = "Assets/MFPS/Content/Prefabs/Weapons/Loadouts";
                if (!AssetDatabase.IsValidFolder(path))
                {
                    path = EditorUtility.OpenFolderPanel("Save Folder", "Assets", "Assets");
                }
                path = bl_UtilityHelper.CreateAsset<bl_PlayerClassLoadout>(path, false, $"{so.objectReferenceValue.name} copy");
                bl_PlayerClassLoadout pcl = AssetDatabase.LoadAssetAtPath(path, typeof(bl_PlayerClassLoadout)) as bl_PlayerClassLoadout;
                so.objectReferenceValue = pcl;
                pcl.Primary = old.Primary;
                pcl.Secondary = old.Primary;
                pcl.Perks = old.Perks;
                pcl.Letal = old.Letal;
                EditorUtility.SetDirty(target);
                EditorUtility.SetDirty(pcl);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawNetworkGunsList(bl_GunManager script)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("WEAPON MANAGER", EditorStyles.toolbarButton);
        GUILayout.Space(5);
        if (GUILayout.Button(new GUIContent("IMPORT", EditorGUIUtility.IconContent("ol plus").image), EditorStyles.toolbarButton, GUILayout.Width(70)))
        {
            EditorWindow.GetWindow<bl_ImportExportWeapon>("Import", true).PrepareToImport(script.transform.root.GetComponent<bl_PlayerNetwork>(), null);
        }
        GUILayout.EndHorizontal();
        SerializedProperty listProperty = serializedObject.FindProperty("AllGuns");
        if (listProperty == null)
        {
            return;
        }

        float containerElementHeight = 22;
        float containerHeight = listProperty.arraySize * containerElementHeight;

        bool isOpen = PhotonGUI.ContainerHeaderFoldout("Gun List (" + script.AllGuns.Count + ")", serializedObject.FindProperty("ObservedComponentsFoldoutOpen").boolValue);
        serializedObject.FindProperty("ObservedComponentsFoldoutOpen").boolValue = isOpen;

        if (isOpen == false)
        {
            containerHeight = 0;
        }

        Rect containerRect = PhotonGUI.ContainerBody(containerHeight);
        if (isOpen == true)
        {
            for (int i = 0; i < listProperty.arraySize; ++i)
            {
                Rect elementRect = new Rect(containerRect.xMin, containerRect.yMin + containerElementHeight * i, containerRect.width, containerElementHeight);
                {
                    Rect texturePosition = new Rect(elementRect.xMin + 6, elementRect.yMin + elementRect.height / 2f - 1, 9, 5);              
                   // MFPSEditorUtils.DrawTexture(texturePosition, MFPSEditorUtils.texGrabHandle);
                    Rect propertyPosition = new Rect(elementRect.xMin + 20, elementRect.yMin + 3, elementRect.width - 45, 16);
                    EditorGUI.PropertyField(propertyPosition, listProperty.GetArrayElementAtIndex(i), new GUIContent());

                    Rect removeButtonRect = new Rect(elementRect.xMax - PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                                                        elementRect.yMin + 2,
                                                        PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                                                        PhotonGUI.DefaultRemoveButtonStyle.fixedHeight);

                    GUI.enabled = listProperty.arraySize > 1;
                    if (GUI.Button(removeButtonRect, new GUIContent(MFPSEditorUtils.texRemoveButton), PhotonGUI.DefaultRemoveButtonStyle))
                    {
                        listProperty.DeleteArrayElementAtIndex(i);
                    }
                    GUI.enabled = true;

                    if (i < listProperty.arraySize - 1)
                    {
                        texturePosition = new Rect(elementRect.xMin + 2, elementRect.yMax, elementRect.width - 4, 1);
                        PhotonGUI.DrawSplitter(texturePosition);
                    }
                }
            }
        }

        if (PhotonGUI.AddButton())
        {
            listProperty.InsertArrayElementAtIndex(Mathf.Max(0, listProperty.arraySize - 1));
        }

        serializedObject.ApplyModifiedProperties();
    }

    GUIStyle FoldOutStyle
    {
        get
        {
            GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.foldout);
            myFoldoutStyle.fontStyle = FontStyle.Bold;
            myFoldoutStyle.fontSize = 14;
            Color myStyleColor = Color.red;
            myFoldoutStyle.normal.textColor = myStyleColor;
            myFoldoutStyle.onNormal.textColor = myStyleColor;
            myFoldoutStyle.hover.textColor = myStyleColor;
            myFoldoutStyle.onHover.textColor = myStyleColor;
            myFoldoutStyle.focused.textColor = myStyleColor;
            myFoldoutStyle.onFocused.textColor = myStyleColor;
            myFoldoutStyle.active.textColor = myStyleColor;
            myFoldoutStyle.onActive.textColor = myStyleColor;
            return myFoldoutStyle;
        }
    }
}