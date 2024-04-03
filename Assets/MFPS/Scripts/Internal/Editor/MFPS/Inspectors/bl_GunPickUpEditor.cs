using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(bl_GunPickUp)), DisallowMultipleComponent, CanEditMultipleObjects]
public class bl_GunPickUpEditor : Editor
{
    bl_GunPickUp script;
    bl_GunInfo info;
    bool isSetUp = true;
    SerializedProperty pickEvent;

    private void OnEnable()
    {
        script = (bl_GunPickUp)target;
        info = bl_GameData.Instance.GetWeapon(script.GunID);
        if (script.m_DetectMode == bl_GunPickUp.DetectMode.Trigger)
        {
            if (script.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
            {
                script.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }
        else
        {
            if (script.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
            {
                script.gameObject.layer = 0;
            }
            SphereCollider sc = script.GetComponent<SphereCollider>();
            Rigidbody rb = script.GetComponent<Rigidbody>();
            BoxCollider bc = script.GetComponent<BoxCollider>();

            if (sc != null && sc.radius < 0.5f)
            {
                sc.radius += 0.2f;
            }
            if (sc == null || rb == null || bc == null) { isSetUp = false; }
        }
        pickEvent = serializedObject.FindProperty("onPickUp");
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginVertical("box");
        if (!serializedObject.isEditingMultipleObjects)
            script.GunID = EditorGUILayout.Popup("Gun ID ", script.GunID, bl_GameData.Instance.AllWeaponStringList(), EditorStyles.toolbarDropDown);
        if (info != null && info.GunIcon != null)
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Texture2D t = info.GunIcon.texture;
            GUILayout.Label(t, GUILayout.MaxWidth(150), GUILayout.Height(75));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        if (info.Type != GunType.Knife && info.Type != GunType.Grenade)
        {
            EditorGUILayout.BeginHorizontal("box");
            float dw = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 90;
            script.Ammunition.Bullets = EditorGUILayout.IntField("Bullets", script.Ammunition.Bullets);
            script.Ammunition.Clips = EditorGUILayout.IntField("Clips", script.Ammunition.Clips);
            int bullets = script.Ammunition.Bullets * script.Ammunition.Clips;
            EditorGUILayout.LabelField(" = " + bullets + " Bullets");
            EditorGUIUtility.labelWidth = dw;
            EditorGUILayout.EndHorizontal();
        }
        else if (info.Type == GunType.Grenade)
        {

        }

        EditorGUILayout.BeginVertical("box");
        script.m_DetectMode = (bl_GunPickUp.DetectMode)EditorGUILayout.EnumPopup("Detect Mode", script.m_DetectMode, EditorStyles.toolbarDropDown);
        script.AutoDestroy = EditorGUILayout.ToggleLeft("Destroy After Time", script.AutoDestroy, EditorStyles.toolbarButton);
        if (script.AutoDestroy)
        {
            script.DestroyIn = EditorGUILayout.Slider("Destroy In", script.DestroyIn, 0.1f, 30);
        }
        EditorGUILayout.EndVertical();
        if (!isSetUp)
        {
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Add require components", EditorStyles.toolbarButton))
            {
                if (script.GetComponent<SphereCollider>() == null)
                {
                    SphereCollider sc = script.gameObject.AddComponent<SphereCollider>();
                    sc.radius = 0.62f;
                    sc.isTrigger = true;
                }
                if (script.GetComponent<BoxCollider>() == null)
                {
                    script.gameObject.AddComponent<BoxCollider>();
                }
                if (script.GetComponent<Rigidbody>() == null)
                {
                    script.gameObject.AddComponent<Rigidbody>();
                }

                isSetUp = false;
            }
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.PropertyField(pickEvent);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            info = bl_GameData.Instance.GetWeapon(script.GunID);
        }
    }
}