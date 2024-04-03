using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MFPSEditor
{
    public class bl_ImportExportWeapon : EditorWindow
    {
        private bl_Gun FPWeapon = null;
        private bl_PlayerNetwork Player;
        private bl_NetworkGun NetworkGun;
        private Texture goodTexture;
        private bool isExport = true;
        private bl_WeaponExported WeaponToImport = null;
        private bool isDone = false;
        Texture[] stateIcons = new Texture[3];
        readonly string LAST_PATH = "last-path-ls";

        [MenuItem("MFPS/Tools/Import Weapon")]
        static void OpenImport()
        {
            GetWindow<bl_ImportExportWeapon>("Import").PrepareToImport(null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            goodTexture = EditorGUIUtility.IconContent("d_SphereCollider Icon").image;
            stateIcons[0] = EditorGUIUtility.IconContent("CollabError").image;
            stateIcons[1] = goodTexture;
            stateIcons[2] = EditorGUIUtility.IconContent("CollabNew").image;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnGUI()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), MFPSEditorStyles.LovattoEditorPalette.GetBackgroundColor());

            if (isExport)
            {
                ExportUI();
            }
            else
            {
                ImportUI();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void ExportUI()
        {
            GUILayout.Space(10);
            if (FPWeapon == null)
            {
                EditorGUILayout.HelpBox("Can't export because the FPWeapon is null", MessageType.Error);
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("EXPORT WEAPON");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                GUILayout.Label("FPWeapon");
                GUILayout.Label(goodTexture, GUILayout.Height(15), GUILayout.Width(15));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            if (Player == null)
            {
                EditorGUILayout.HelpBox("Can't export because the Player is null", MessageType.Error);
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Player Info");
                GUILayout.Label(goodTexture, GUILayout.Height(15), GUILayout.Width(15));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            if (NetworkGun == null)
            {
                EditorGUILayout.HelpBox("Export will be incomplete because the TPWeapon was not found", MessageType.Warning);
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("TPWeapon");
                GUILayout.Label(goodTexture, GUILayout.Height(15), GUILayout.Width(15));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("EXPORT WEAPON", GUILayout.Height(25)))
            {
                Export();
            }
            GUILayout.Space(10);
        }

        /// <summary>
        /// 
        /// </summary>
        void ImportUI()
        {
            if (!isDone)
            {
                bool canImport = true;
                if (WeaponToImport == null)
                {
                    EditorGUILayout.HelpBox("Assign the weapon exported prefab to import.", MessageType.Info);
                    WeaponToImport = EditorGUILayout.ObjectField("Weapon To Import", WeaponToImport, typeof(bl_WeaponExported), true) as bl_WeaponExported;
                    canImport = false;
                }
                else
                {
                    GUILayout.Space(10);
                    GUI.enabled = false;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField("Weapon To Import", WeaponToImport, typeof(bl_WeaponExported), true);
                    GUILayout.Label(goodTexture, GUILayout.Width(15), GUILayout.Height(15));
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }
                if (Player == null)
                {
                    EditorGUILayout.HelpBox("Assign the player prefab where you wanna import this weapon.", MessageType.Info);
                    Player = EditorGUILayout.ObjectField("Player", Player, typeof(bl_PlayerNetwork), true) as bl_PlayerNetwork;
                    canImport = false;
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField("Player", Player, typeof(bl_PlayerNetwork), true);
                    if (Player.gameObject.scene.name == null) { GUILayout.Label(EditorGUIUtility.IconContent("d_ToolHandleLocal").image, GUILayout.Width(15), GUILayout.Height(15)); }
                    else { GUILayout.Label(EditorGUIUtility.IconContent("PrefabModel Icon").image, GUILayout.Width(15), GUILayout.Height(15)); }
                    GUILayout.Label(goodTexture, GUILayout.Width(15), GUILayout.Height(15));
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }
                if (WeaponToImport != null && WeaponToImport.WeaponInfo != null && WeaponToImport.WeaponInfo.GunIcon != null)
                {
                    GUILayout.Space(25);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    Texture2D icon = WeaponToImport.WeaponInfo.GunIcon.texture;
                    float aspet = 100f / (float)icon.width;
                    GUILayout.Space(50);
                    GUILayout.Label(icon, GUILayout.Width(100), GUILayout.Height(icon.height * aspet));
                    GUILayout.Space(10);
                    var tlcIcons = EditorGUIUtility.IconContent("d_Animation.Play");
                    if(tlcIcons != null)
                    GUILayout.Label(tlcIcons.image, GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight));

                    GUILayout.Space(10);
                    if(Player != null)GUILayout.Label(Player.gameObject.name, GUILayout.Width(150));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                GUI.enabled = canImport;
                GUILayout.FlexibleSpace();
                if (MFPSEditorStyles.ButtonOutline("IMPORT"))
                {
                    Import();
                }
                GUILayout.Space(5);
                GUI.enabled = true;
            }
            else
            {
                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("WEAPON IMPORTED!");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Weapon Info");
                Texture icon = stateIcons[results[0]];
                GUILayout.Label(icon, GUILayout.Width(20));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("FP Weapon");
                icon = stateIcons[results[1]];
                GUILayout.Label(icon, GUILayout.Width(20));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("TP Weapon");
                icon = stateIcons[results[2]];
                GUILayout.Label(icon, GUILayout.Width(20));
                GUILayout.EndHorizontal();

                GUILayout.Space(20);
                if (GUILayout.Button("DONE"))
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fpweapon"></param>
        /// <param name="player"></param>
        public void PrepareToExport(bl_Gun fpweapon, bl_PlayerNetwork player)
        {
            FPWeapon = fpweapon;
            Player = player;
            if (FPWeapon == null || Player == null) return;
            NetworkGun = Player.NetworkGuns.Find(x => x.LocalGun.GunID == FPWeapon.GunID);
            isExport = true;

            minSize = new Vector2(200, 150);
            maxSize = new Vector2(250, 200);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="exportedWeapon"></param>
        public void PrepareToImport(bl_PlayerNetwork player, bl_WeaponExported exportedWeapon)
        {
            Player = player;
            WeaponToImport = exportedWeapon;
            isExport = false;

            minSize = new Vector2(400, 200);
            maxSize = new Vector2(600, 350);
        }

        int[] results = new int[4];
        public void Import()
        {
            GameObject playerInstance = Player.gameObject;
            bool isPrefab = false;
            if (Player.gameObject.scene.name == null)
            {
                playerInstance = PrefabUtility.InstantiatePrefab(Player.gameObject, EditorSceneManager.GetActiveScene()) as GameObject;
#if UNITY_2018_3_OR_NEWER
        PrefabUtility.UnpackPrefabInstance(playerInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
                playerInstance.name = Player.gameObject.name;
                isPrefab = true;
            }
            bl_WeaponExported we = WeaponToImport;
            if (WeaponToImport.gameObject.scene.name == null)
            {
                GameObject weo = PrefabUtility.InstantiatePrefab(WeaponToImport.gameObject, EditorSceneManager.GetActiveScene()) as GameObject;
#if UNITY_2018_3_OR_NEWER
        PrefabUtility.UnpackPrefabInstance(weo, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
                we = weo.GetComponent<bl_WeaponExported>();
            }
            if (we.WeaponInfo != null && !string.IsNullOrEmpty(we.WeaponInfo.Name))
            {
                if (!bl_GameData.Instance.AllWeapons.Exists(x => x.Name == we.WeaponInfo.Name))
                {
                    bl_GameData.Instance.AllWeapons.Add(we.WeaponInfo);
                    we.FPWeapon.GunID = bl_GameData.Instance.AllWeapons.Count - 1;
                    EditorUtility.SetDirty(bl_GameData.Instance);
                    results[0] = 1;
                }
                else
                {
                    we.FPWeapon.GunID = bl_GameData.Instance.AllWeapons.FindIndex(x => x.Name == we.WeaponInfo.Name);
                    EditorUtility.SetDirty(bl_GameData.Instance);
                    results[0] = 2;
                }
            }
            else
            {
                results[0] = 0;
            }

            bl_GunManager gm = playerInstance.GetComponentInChildren<bl_GunManager>();
            if (gm != null)
            {
                we.FPWeapon.transform.parent = gm.transform;
                we.FPWeapon.transform.localPosition = we.FPWPosition;
                we.FPWeapon.transform.localRotation = we.FPWRotation;
                we.FPWeapon.name = we.FPWeapon.name.Replace("[FP]", "");
                gm.AllGuns.Add(we.FPWeapon);
                results[1] = 1;
            }
            else
            {
                results[1] = 0;
            }

            if (we.TPWeapon != null)
            {
                we.TPWeapon.LocalGun = we.FPWeapon;
                we.TPWeapon.transform.parent = playerInstance.GetComponent<bl_PlayerNetwork>().NetworkGuns[0].transform.parent;
                we.TPWeapon.transform.localPosition = we.TPWPosition;
                we.TPWeapon.transform.localRotation = we.TPWRotation;
                we.TPWeapon.name = we.TPWeapon.name.Replace("[TP]", "");
                playerInstance.GetComponent<bl_PlayerNetwork>().NetworkGuns.Add(we.TPWeapon);
                results[2] = 1;
            }
            else
            {
                results[2] = 0;
            }

            if (isPrefab)
            {

            }
            DestroyImmediate(we.gameObject);
            EditorUtility.SetDirty(playerInstance);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            isDone = true;
            Repaint();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Export()
        {
            if (FPWeapon == null || Player == null) return;

            bl_GunInfo info = bl_GameData.Instance.GetWeapon(FPWeapon.GunID);
            GameObject root = new GameObject(info.Name + " [Export]");
            root.transform.SetAsLastSibling();
            bl_WeaponExported export = root.AddComponent<bl_WeaponExported>();
            export.WeaponInfo = info;

            GameObject fpwCopy = Instantiate(FPWeapon.gameObject);
            fpwCopy.SetActive(true);
            fpwCopy.name = info.Name + " [FP]";
            fpwCopy.transform.parent = root.transform;
            fpwCopy.transform.localPosition = Vector3.zero;
            export.FPWeapon = fpwCopy.GetComponent<bl_Gun>();
            export.FPWPosition = FPWeapon.transform.localPosition;
            export.FPWRotation = FPWeapon.transform.localRotation;

            if (NetworkGun != null)
            {
                GameObject tpwCopy = Instantiate(NetworkGun.gameObject);
                tpwCopy.SetActive(true);
                tpwCopy.name = info.Name + " [TP]";
                tpwCopy.transform.parent = root.transform;
                tpwCopy.transform.localPosition = Vector3.zero;
                export.TPWeapon = tpwCopy.GetComponent<bl_NetworkGun>();
                export.TPWPosition = NetworkGun.transform.localPosition;
                export.TPWRotation = NetworkGun.transform.localRotation;
            }
            string lastPath = EditorPrefs.GetString(LAST_PATH, "");
            string path = EditorUtility.OpenFolderPanel("Select Folder to save prefab", lastPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                string subPath = path.Substring(Application.dataPath.Length);
                subPath = subPath.TrimStart();
                if (!subPath.StartsWith("/")) subPath = "/" + subPath;

                string relativepath = "Assets" + subPath;
                relativepath += "/" + root.name + ".prefab";
                Debug.Log(relativepath);
#if UNITY_2018_3_OR_NEWER
                GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(root, relativepath);
#else
                GameObject newPrefab = PrefabUtility.CreatePrefab(relativepath, root);
#endif
                Selection.activeGameObject = newPrefab;
                EditorGUIUtility.PingObject(newPrefab);
                DestroyImmediate(root);
                EditorPrefs.SetString(LAST_PATH, path);
            }
            else
            {
                Debug.LogWarning("Could not create a prefab automatically.");
                Selection.activeGameObject = root;
                EditorGUIUtility.PingObject(root);
            }
            Close();
        }
    }
}