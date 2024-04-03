using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;

namespace MFPS.Runtime.Level
{
    [CustomEditor(typeof(bl_Ammo))]
    public class bl_AmmoKitEditor : Editor
    {
        bl_Ammo script;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            script = (bl_Ammo)target;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Space(4);
                script.itemAuthority = (bl_NetworkItem.ItemAuthority)EditorGUILayout.EnumPopup("Item Authority", script.itemAuthority, EditorStyles.toolbarPopup);
                GUILayout.Space(4);
                Rect r = GUILayoutUtility.GetRect(Screen.width - 100, EditorGUIUtility.singleLineHeight);
                script.isSceneItem = MFPSEditorStyles.FeatureToogle(r, script.isSceneItem, "Is Scene Item");
                GUILayout.Space(10);
                r = GUILayoutUtility.GetRect(Screen.width - 100, EditorGUIUtility.singleLineHeight);
                script.isGlobal = MFPSEditorStyles.FeatureToogle(r, script.isGlobal, "Is Global Ammo");
                GUILayout.Space(2);
                r = GUILayoutUtility.GetRect(Screen.width - 100, EditorGUIUtility.singleLineHeight);
                script.autoRespawn = MFPSEditorStyles.FeatureToogle(r, script.autoRespawn, "Auto Respawn");
                GUILayout.Space(2);
                if (!script.isGlobal)
                {
                    script.ForGun = EditorGUILayout.Popup("For Gun", script.ForGun, bl_GameData.Instance.AllWeaponStringList(), EditorStyles.toolbarPopup);
                    GUILayout.Space(2);
                }
                script.Bullets = EditorGUILayout.IntField("Bullets", script.Bullets);
                script.Projectiles = EditorGUILayout.IntField("Projectiles", script.Projectiles);

                script.PickSound = EditorGUILayout.ObjectField("PickUp Sound", script.PickSound, typeof(AudioClip), false) as AudioClip;
            }
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                script.EditorValidateName();
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
    }
}