#if UNITY_EDITOR
#define MFPS
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using UnityEditor.PackageManager;
using System.Reflection;
using System.Linq;

namespace LovattoEditor
{
    public class MFPSImporter : EditorWindow
    {
        public static Dictionary<string, GUIStyle> cachedStyles = new Dictionary<string, GUIStyle>();
        public delegate void OnCreateStyleOp(ref GUIStyle style);

        public int currentWindow = 0;
        public Texture2D[] textures = new Texture2D[4];
        public GUISkin skin = null;
        public bool isLoading, installingPackage = false;
        float rotateAngle = 0;
        private const string IMPORT_FILES_PATH = "Assets/MFPS/Import/";
        private Color backgroundColor = new Color(0.09803922f, 0.09803922f, 0.09803922f, 1.00f);
        private Color headerColor = new Color(0.1568628f, 0.1568628f, 0.1568628f, 1.00f);
        private Color highlightColor = new Color(1f, 0.8628919f, 0f, 1.00f);
        private float alpha = 0;
        private float alphaTime = 1;
        private float deltaTime = 0.02f;
        private double lastFrameTime;
        private bool isFading, isFadeIn = false;
        public bool isWaitingForImportation, isInstallComplete = false;
        private Color overlayColor = new Color(0, 0, 0, 0.5f);
        public bool wasCompiling = false;
        private AnimationCurve fadeCurve = new AnimationCurve(new Keyframe() { time = 0, value = 0 },
            new Keyframe() { time = 0.4f, value = 1 }, new Keyframe() { time = 0.6f, value = 1 }, new Keyframe() { time = 1, value = 0 }
            );
        public Action onFadeFinish;
        public Action onFadeMid;
        private int fetchTrys = 0;
        public bool afterImportCallbackCalled = false;

#if MFPS
        [InitializeOnLoadMethod]
#else
        [MenuItem("Lovatto/MFPSEditor/MFPS Importer Test")]
#endif
        public static void Init()
        {
            string path = Application.dataPath + "/MFPS/Content/";
            if (Directory.Exists(path))
            {
                if (PlayerPrefs.GetInt("mfpsdev.lovatto", 0) == 1) return;

                if (HasOpenInstances<MFPSImporter>())
                {

                }
                else
                {
                    // MFPS is already imported in the project
                    Debug.Log("MFPS has been already imported in this project");

                }
            }
            else
            {
                // MFPS folder was not found
                // Lets do another check before assume it is not imported
                var gameData = Resources.Load("GameData");
                if (gameData == null)
                {
                    // No is confirmed that MFPS has not been imported yet
                    // Lets check if the MFPS .unitypackage is available

                    path = $"{Application.dataPath}/MFPS/mfps.unitypackage";
                    if (File.Exists(path))
                    {
                        // Importation can be execute, lets open the window
                        var cr = Screen.currentResolution;
                        GetWindowWithRect<MFPSImporter>(new Rect(cr.width * 0.5f - 360, cr.height * 0.5f - 200, 720, 400), true);
                    }
                    else
                    {
                        Debug.LogError("MFPS import package can't be found.");
                    }
                }
                else
                {
                    // MFPS exist but with a different root folder name
                    Debug.Log("MFPS exist but with a different root folder name.");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            titleContent = new GUIContent("MFPS Import");
            minSize = new Vector2(720, 400);
            maxSize = new Vector2(720, 400);
            AssetDatabase.importPackageCompleted += ImportCompleted;
            FetchUI();
            lastFrameTime = EditorApplication.timeSinceStartup;

            if (!installingPackage) dependencysState = 0;
        }

        private void FetchUI()
        {
            textures[0] = GetImportFile<Texture2D>("mfps-logo.png");
            textures[1] = (Texture2D)EditorGUIUtility.IconContent("ColorPicker-HueRing-Thumb-Fill").image;
            textures[2] = GetImportFile<Texture2D>("corner-overlay.png");
            textures[3] = GetImportFile<Texture2D>("check.png");
            if (skin == null) skin = GetImportFile<GUISkin>("import-skin.guiskin");
            if (skin == null) fetchTrys++;

            if (fetchTrys >= 10)
            {
                GetWindow<MFPSImporter>();
                isInstallComplete = true;
                Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            AssetDatabase.importPackageCompleted -= ImportCompleted;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDestroy()
        {
            if (!isInstallComplete && !installingPackage)
            {
#if MFPS
                EditorUtility.DisplayDialog("Installation Incomplete", "MFPS has not been installed yet,\nYou can open the installer window again in:\nEditor top menu > MFPS > Import.", "Got it!");
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        void ImportCompleted(string packageName)
        {
            dependencysState = 0;
            isLoading = false;

            if (packageName == "mfps" && isWaitingForImportation)
            {
                OnMFPSImported();
            }
        }

        #region Layout
        /// <summary>
        /// 
        /// </summary>
        private void OnGUI()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), backgroundColor);
            if (skin == null)
            {
                FetchUI();
                return;
            }

            Content();
            Footer();
            DrawLoadingScreen();
            Loading();
            DrawFade();

            deltaTime = (float)(EditorApplication.timeSinceStartup - lastFrameTime);
            lastFrameTime = EditorApplication.timeSinceStartup;
            Repaint();
        }

        /// <summary>
        /// 
        /// </summary>
        void Content()
        {
            switch (currentWindow)
            {
                case 0:
                    DrawWelcome();
                    break;
                case 1:
                    DrawImportDependencys();
                    break;
                case 2:
                    DrawSetupPhoton();
                    break;
                case 3:
                    DrawImportMFPS();
                    break;
                case 4:
                    DrawImportResume();
                    break;
            }

            var r = new Rect(position.width - 400, position.height - 270, 400, 270);
            GUI.DrawTexture(r, textures[2]);
        }

        /// <summary>
        /// 
        /// </summary>
        void Footer()
        {
            try
            {
                GUILayout.FlexibleSpace();
            }
            catch (ArgumentException e)
            {
                if (e.Message == "") { }
            }
            DrawDotsStepts();
        }

        /// <summary>
        /// 
        /// </summary>
        void Loading()
        {
            if (!isLoading && !EditorApplication.isCompiling) return;

            var matrix = GUI.matrix;
            var rect = new Rect(position.width - 25, position.height - 25, 18, 18);
            GUIUtility.RotateAroundPivot(rotateAngle, new Vector2(rect.x + 9, rect.y + 9));
            GUI.color = highlightColor;
            GUI.DrawTexture(rect, GetUnityIcon("Loading"), ScaleMode.ScaleToFit);
            GUI.color = Color.white;
            rotateAngle += 3 * Time.deltaTime;
            GUI.matrix = matrix;
        }

        /// <summary>
        /// 
        /// </summary>
        void DrawFade()
        {
            if (alphaTime < 1)
            {
                alphaTime += deltaTime * 2;
                if (alpha >= 0.5f && isFadeIn)
                {
                    onFadeMid?.Invoke();
                    isFadeIn = false;
                }
            }
            else
            {
                if (alpha <= 0)
                {
                    if (isFading)
                    {
                        onFadeFinish?.Invoke();
                        isFading = false;
                    }
                    return;
                }
            }

            alpha = fadeCurve.Evaluate(alphaTime);

            if (alpha <= 0) return;

            var color = new Color(0, 0, 0, alpha);
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), color);

        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        void DrawWelcome()
        {
            GUILayout.Space(50);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (textures[0] != null)
            {
                GUILayout.Label(textures[0]);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(50);
            DrawText("<size=22>Welcome to <b>MFPS 2.0</b> Installer</size>", GetUnityStyle("c-text", TextStyle, (ref GUIStyle s) =>
            {
                s.alignment = TextAnchor.MiddleCenter;
                s.wordWrap = true;
            }));
            DrawText("<color=#B7B7B7C2><size=11>To achieve a correct installation of MFPS, please follow the indications.</size></color>", GetUnityStyle("c-text", TextStyle));
            GUILayout.Space(50);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (DrawButton("<b><size=16>Install</size></b>", GUILayout.Height(40), GUILayout.Width(100)))
            {
                ChangeWindow(currentWindow + 1);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 
        /// </summary>
        private void DrawLoadingScreen()
        {
            if (!EditorApplication.isCompiling)
            {
                if (wasCompiling)
                {
                    OnAfterCompilation();
                    wasCompiling = false;
                }
                return;
            }

            if (!wasCompiling)
            {
                wasCompiling = true;
            }

            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), overlayColor);
        }

        /// <summary>
        /// 
        /// </summary>
        void DrawBackButton()
        {
            var r = new Rect(20, position.height - 40, 60, 20);
            if (DrawButton(r, "<b>BACK</b>"))
            {
                ChangeWindow(currentWindow - 1);
            }
        }

        #region Dependency installation
        /// <summary>
        /// 
        /// </summary>
        public int dependencysState = 0;
        public List<string> installedPackages;
        void DrawImportDependencys()
        {
            GUILayout.Space(50);
            if (dependencysState == 0)
            {
                EditorCoroutines.Execute(CheckPackages());
                dependencysState = 1;
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                using (new GUILayout.VerticalScope())
                {
                    DrawText($"<size=20><b>Install Dependencies</b></size>", GetUnityStyle("c-text", TextStyle, (ref GUIStyle s) =>
                    {
                        s.alignment = TextAnchor.MiddleCenter;
                        s.wordWrap = true;
                    }));
                    DrawText("<color=#B7B7B7C2><size=11>MFPS requires some Unity packages to work correctly\nthese package has to be installed before proceeding with the MFPS installation</size></color>", GetUnityStyle("c-text", TextStyle), GUILayout.Height(40), GUILayout.Width(600));
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(50);

            bool canContinue = dependencysState > 0;
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                using (new GUILayout.VerticalScope())
                {
#if !MFPS_MOBILE
                    if (!DrawPackageStatus("com.unity.postprocessing", "Post-Processing")) canContinue = false;
#endif
                    if (!DrawPackageStatus("com.unity.textmeshpro", "Text Mesh Pro")) canContinue = false;
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                using (new GUILayout.VerticalScope())
                {
                    if (!canContinue)
                    {
                        DrawText("<color=#B7B7B7C2><size=11>Install all the required packages before continue.</size></color>", GetUnityStyle("c-text", TextStyle), GUILayout.Width(300));
                    }

                    GUI.enabled = canContinue;
                    if (DrawButton("<b><size=16>Continue</size></b>", GUILayout.Height(40), GUILayout.Width(100)))
                    {
                        ChangeWindow(currentWindow + 1);
                    }
                    GUI.enabled = true;
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        private bool DrawPackageStatus(string packageID, string packageName)
        {
            bool v = false;
            var r = EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.DrawRect(r, headerColor);
                DrawText(packageName, GUILayout.Width(200));
                GUILayout.Space(10);
                if (installedPackages != null && installedPackages.Contains(packageID))
                {
                    DrawText("<b><color=#B0EC58FF>Installed</color></b>", GUILayout.Width(100));
                    v = true;
                }
                else
                {
                    if (dependencysState <= 0)
                    {
                        DrawText("<b><color=#E73340FF>Fetching info...</color></b>", GUILayout.Width(100));
                    }
                    else
                    {
                        DrawText("<b><color=#E73340FF>Not Installed</color></b>", GUILayout.Width(100));
                        GUILayout.Space(10);

                        if (!installingPackage && dependencysState != 1)
                        {
                            if (GUILayout.Button("Install", GUILayout.Width(50)))
                            {
                                EditorCoroutines.Execute(AddPack(packageID));
                            }
                        }
                        else
                        {
                            DrawText("<b>Waiting for package...</b>", GUILayout.Width(130));
                        }

                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            return v;
        }

        IEnumerator AddPack(string packageID)
        {
            isLoading = true;
            installingPackage = true;
            var request = Client.Add(packageID);
            while (!request.IsCompleted) yield return null;

            if (request.Status == StatusCode.Success)
            {

            }
            isLoading = false;
            installingPackage = false;
            dependencysState = 0; //recheck the installed packages

            Repaint();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator CheckPackages()
        {
            isLoading = true;
            installedPackages = new List<string>();

            var request = Client.List(true);
            while (!request.IsCompleted) yield return null;

            var projectPackages = request.Result;
            foreach (var item in projectPackages)
            {
                installedPackages.Add(item.name);
            }
            isLoading = false;
            dependencysState = 2;
        }
        #endregion

        public string appID = "";
        private void DrawSetupPhoton()
        {
            GUILayout.Space(50);

            DrawText("<size=20><b>Setup Photon PUN</b></size>", GetUnityStyle("c-text", TextStyle, (ref GUIStyle s) =>
            {
                s.alignment = TextAnchor.MiddleCenter;
                s.wordWrap = true;
            }));
            GUILayout.Space(15);
            using (new HorizontalCenterScope())
            {
                DrawText("<color=#B7B7B7C2><size=11>MFPS uses <b>Photon PUN 2</b> as the network framework, in order to setup Photon you only need to set your AppID below\n\nIf you don't have an AppID yet, you can get yours as follow:</size></color>", GetUnityStyle("c-text", TextStyle), GUILayout.Height(60), GUILayout.Width(500));
            }
            GUILayout.Space(30);
            var style = GetUnityStyle("list-text", TextStyle, (ref GUIStyle s) =>
            {
                s.alignment = TextAnchor.MiddleLeft;
                s.wordWrap = true;
                s.margin.left = 40;
                s.fontSize = 11;
                s.normal.textColor = new Color(1, 1, 1, 0.6f);
            });
            DrawText("■ Go to the <color=#007EFFFF>Photon Dashboard</color> and log in or create an account if you don't have one.", style);
            var r = GUILayoutUtility.GetLastRect();
            r.x += 62;
            r.width = 100;
            if (GUI.Button(r, GUIContent.none, GUIStyle.none))
            {
                Application.OpenURL("https://dashboard.photonengine.com/");
            }
            DrawText("■ Click on the button '<b>Create A New App</b>'", style);
            DrawText("■ In <b>Photon Type<color=#FF0B0BFF>*</color></b> set <b>Photon PUN</b>, set your game name in the <b>Name<color=#FF0B0BFF>*</color></b> field and Click on the <b>Create</b> button.", style);
            DrawText("■ In the new page, from the box with the title <b>PUN</b>, copy the text from the <b>App ID</b> field and paste it below and click <b>Continue</b>", style);
            GUILayout.Space(30);
            using (new HorizontalCenterScope())
            {
                DrawText("App ID", GUILayout.Width(45));
                GUILayout.Space(5);
                appID = EditorGUILayout.TextField(appID, GUILayout.Width(300));
                GUILayout.Space(5);
                GUI.enabled = !string.IsNullOrEmpty(appID);
                if (DrawButton("<b>Continue</b>", GUILayout.Width(80)))
                {
                    GUIUtility.keyboardControl = 0;
                    GUIUtility.hotControl = 0;
                    ChangeWindow(currentWindow + 1);
                }
                GUI.enabled = true;
            }
            GUILayout.Space(10);
            using (new HorizontalCenterScope())
            {
                GUI.enabled = string.IsNullOrEmpty(appID);
                if (DrawButton("Skip for now", GUILayout.Width(95)))
                {
                    GUIUtility.keyboardControl = 0;
                    GUIUtility.hotControl = 0;
                    ChangeWindow(currentWindow + 1);
                }
                GUI.enabled = true;
            }
        }

        #region MFPS Import
        /// <summary>
        /// 
        /// </summary>
        public MFPSImportStatus mFPSImportStatus = MFPSImportStatus.None;
        public bool[] mfpsImportOptions = new bool[] { true, true, true };
        void DrawImportMFPS()
        {
            if (mFPSImportStatus == MFPSImportStatus.None)
            {
                mFPSImportStatus = CheckMFPSImportStatus();
            }

            try
            {
                DrawBackButton();
                GUILayout.Space(50);

                if (mFPSImportStatus == MFPSImportStatus.NoImported)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(30);
                        var r = EditorGUILayout.BeginVertical(GUILayout.Width(300));
                        {
                            r.x = r.y = 0;
                            r.width = 360;
                            r.height = position.height;
                            EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.2f));
                            DrawText("<size=18><b>MFPS is ready to install.</b></size>");
                            GUILayout.Space(30);
                            mfpsImportOptions[0] = Toggle("<size=10>Setup Layers and Tags</size>", mfpsImportOptions[0]);
                            mfpsImportOptions[1] = Toggle("<size=10>Setup Unity Input Settings</size>", mfpsImportOptions[1]);
                            mfpsImportOptions[2] = Toggle("<size=10>Add MFPS scenes in Build Settings</size>", mfpsImportOptions[2]);
                            GUILayout.Space(50);
                            DrawText("<size=9><i><color=#7D7D7DFF>MFPS will override these settings in case they are marked,\nif this is not a new Unity project\nyour changes to the Tags and Input settings\nwill be replaced with the MFPS ones.</color></i></size>", GUILayout.Height(50));
                        }
                        EditorGUILayout.EndVertical();

                        GUILayout.Space(30);
                        EditorGUILayout.BeginVertical();

                        GUILayout.FlexibleSpace();
                        GUILayout.Space(50);

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        GUI.enabled = !isLoading;
                        if (DrawButton("<size=20><b><color=#191919>Install</color></b></size>", GUILayout.Width(150), GUILayout.Height(50)))
                        {
                            PlayerPrefs.SetInt("mfps.install.setlayers", mfpsImportOptions[0] ? 1 : 0);
                            PlayerPrefs.SetInt("mfps.install.setinput", mfpsImportOptions[1] ? 1 : 0);
                            PlayerPrefs.SetString("mfps.install.punappid", appID);
#if MFPS
                            isLoading = true;
                            isWaitingForImportation = true;
                            Repaint();
                            string path = $"{Application.dataPath}/MFPS/mfps.unitypackage";
                            AssetDatabase.ImportPackage(path, false);
#else
                            ChangeWindow(currentWindow + 1);
#endif
                        }
                        GUI.enabled = true;

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.Space(10);
                        DrawText("<size=9><i><color=#7D7D7DFF>After clicking the button\nwait until the package importation finish\nDepending on your hardware this may take a while.</color></i></size>", GetUnityStyle("c-text", TextStyle, (ref GUIStyle s) =>
                        {
                            s.alignment = TextAnchor.MiddleCenter;
                            s.wordWrap = true;
                        }), GUILayout.Height(50));
                        GUILayout.FlexibleSpace();

                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else if (mFPSImportStatus == MFPSImportStatus.Imported || mFPSImportStatus == MFPSImportStatus.ImportedWithFolderChanged)
                {
                    DrawMFPSAlreadyImported();
                }
            }
            catch (ArgumentException e)
            {
                if (e.Message == "") { }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void DrawMFPSAlreadyImported()
        {
            using (new HorizontalCenterScope())
            {
                DrawText("<b><b><size=14>MFPS is already installed in this project</size></b></b>\nInstall it again will override all the changes/modifications you have done\nso <b>do not re-install again unless you know what are you doing.</b>\n \n<b>DO NOT</b> import MFPS updates in projects where you have an older version of MFPS,\nsince this will also override your changes and potentially break your project.\n\nFor the guide on how to update MFPS,\nconsult the MFPS documentation under <i>MFPS ➔ Tutorials ➔ Documentation ➔ Update MFPS</i>.", GUILayout.Height(150), GUILayout.Width(600));
            }
            GUI.DrawTexture(new Rect(position.width - 75, 70, 30, 30), EditorGUIUtility.IconContent("console.warnicon").image);
            GUILayout.Space(30);
            using (new HorizontalCenterScope())
            {
                if (DrawButton("Yes, I know what I'm doing", GUILayout.Height(30)))
                {
                    mFPSImportStatus = MFPSImportStatus.NoImported;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void DrawImportResume()
        {
            try
            {
                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginVertical();
                var r = GUILayoutUtility.GetRect(96, 96);
                r.x = position.width * 0.5f - 48;
                r.width = r.height = 96;
                GUI.DrawTexture(r, textures[3]);

                GUILayout.Space(30);
                DrawText("<size=11><b><color=#DDDDDD>MFPS has been installed correctly!\nYou can start develop your game, good luck!</color></b></size>", GetUnityStyle("c-text", TextStyle, (ref GUIStyle s) =>
                {
                    s.alignment = TextAnchor.MiddleCenter;
                    s.wordWrap = true;
                }), GUILayout.Height(50), GUILayout.Width(300));
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(30);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (DrawButton("<size=20><b><color=#191919>Finish</color></b></size>", GUILayout.Width(150), GUILayout.Height(50)))
                {
                    EditorApplication.ExecuteMenuItem("MFPS/MFPS");

#if MFPS

                    try
                    {
                        var path = $"{Application.dataPath}/MFPS/Import";
                        if (Directory.Exists(path))
                        {
                            File.Delete(path + ".meta");
                            Directory.Delete(path, true);
                        }

                        path = $"{Application.dataPath}/MFPS/mfps.unitypackage";
                        if (File.Exists(path))
                        {
                            File.Delete(path + ".meta");
                            File.Delete(path);
                        }

                        path = $"{Application.dataPath}/MFPS/MFPSImporter.cs";
                        if (File.Exists(path))
                        {
                            File.Delete(path + ".meta");
                            File.Delete(path);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e.Message);
                    }

                    this.Close();
                    AssetDatabase.Refresh();
#else
                    this.Close();
#endif
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (!afterImportCallbackCalled)
                {
                    AfterImportCallback();
                    afterImportCallbackCalled = true;
                }
            }
            catch (ArgumentException e)
            {
                if (e.Message == "") { }
            }
        }

        void AfterImportCallback()
        {
            try
            {
                if (mfpsImportOptions[2] == true)
                {
                    var scenesArray = new EditorBuildSettingsScene[0];
                    if (EditorBuildSettings.scenes != null && EditorBuildSettings.scenes.Length > 0)
                    {
                        scenesArray = EditorBuildSettings.scenes;
                    }
                    var scenes = scenesArray.ToList();

                    const string MainMenuPath = "Assets/MFPS/Scenes/MainMenu.unity";
                    const string ExampleLevelPath = "Assets/MFPS/Scenes/ExampleLevel.unity";

                    if (!scenes.Exists(x => x.path == MainMenuPath))
                    {
                        scenes.Insert(0, new EditorBuildSettingsScene()
                        {
                            path = MainMenuPath,
                            enabled = true,
                        });
                    }

                    if (!scenes.Exists(x => x.path == ExampleLevelPath))
                    {
                        scenes.Add(new EditorBuildSettingsScene()
                        {
                            path = ExampleLevelPath,
                            enabled = true,
                        });
                    }

                    EditorBuildSettings.scenes = scenes.ToArray();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        MFPSImportStatus CheckMFPSImportStatus()
        {
            string path = Application.dataPath + "/MFPS/ContentTEST/";
            if (Directory.Exists(path))
            {
                isInstallComplete = true;
                // MFPS is already imported in the project
                return MFPSImportStatus.Imported;
            }
            else
            {
                // MFPS folder was not found
                // Lets do another check before assume it is not imported
                var gameData = Resources.Load("GameDataTEST");
                if (gameData == null)
                {
                    // No is confirmed that MFPS has not been imported yet
                    // Lets check if the MFPS .unitypackage is available

                    path = $"{Application.dataPath}/MFPS/mfps.unitypackage";
                    if (File.Exists(path))
                    {
                        // Importation can be execute, lets open the window
                        return MFPSImportStatus.NoImported;
                    }
                    else
                    {
                        return MFPSImportStatus.NoFoundImportFile;
                    }
                }
                else
                {
                    isInstallComplete = true;
                    // MFPS exist but with a different root folder name
                    return MFPSImportStatus.ImportedWithFolderChanged;
                }
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        void DrawDotsStepts()
        {
            try
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                for (int i = 0; i < 5; i++)
                {
                    GUI.color = i == currentWindow ? Color.white : new Color(1, 1, 1, 0.5f);
                    GUILayout.Label(textures[1], GUILayout.Width(10), GUILayout.Height(10));
                }
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            catch (ArgumentException e)
            {
                if (e.Message == "") { }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnMFPSImported()
        {
            isLoading = false;
            isInstallComplete = true;
            isWaitingForImportation = false;

            string appID = PlayerPrefs.GetString("mfps.install.punappid");
            if (!string.IsNullOrEmpty(appID))
            {
                var photonSettings = Resources.Load("PhotonServerSettings", typeof(ScriptableObject)) as ScriptableObject;
                if (photonSettings != null)
                {
                    var so = new SerializedObject(photonSettings);
                    var sso = so.FindProperty("AppSettings");
                    sso.FindPropertyRelative("AppIdRealtime").stringValue = appID;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(photonSettings);
                }
                else
                {
                    Debug.LogWarning("Photon Server Settings couldn't be found.");
                }
            }

            ChangeWindow(4, true);

            ClearConsole();
        }

        private void OnAfterCompilation()
        {

        }

        public void ChangeWindow(int newWindow, bool inmediate = false)
        {
            if (inmediate)
            {
                currentWindow = newWindow;
            }
            else
            {
                alphaTime = 0;
                isFading = true;
                isFadeIn = true;
                onFadeMid = () =>
                {
                    currentWindow = newWindow;
                };
            }
            Repaint();
        }

        #region Drawers
        void DrawText(string text, params GUILayoutOption[] options) => DrawText(text, TextStyle, options);
        void DrawText(string text, GUIStyle style, params GUILayoutOption[] options)
        {
            if (style == null) style = TextStyle;

            EditorGUILayout.LabelField(text, style, options);
        }

        private bool DrawButton(string text, params GUILayoutOption[] options)
        {
            bool v = false;
            GUIStyle style = EditorStyles.toolbarButton;
            if (skin != null) style = skin.GetStyle("btn");
            if (GUILayout.Button(text, style, options)) v = true;

            return v;
        }

        private bool DrawButton(Rect rect, string text)
        {
            bool v = false;
            GUIStyle style = EditorStyles.toolbarButton;
            if (skin != null) style = skin.GetStyle("btn");
            if (GUI.Button(rect, text, style)) v = true;

            return v;
        }

        public bool Toggle(string title, bool value)
        {
            var textStyle = GetUnityStyle("left-text", TextStyle, (ref GUIStyle style) =>
            {
                style.alignment = TextAnchor.MiddleLeft;
                style.padding.right = 16;
                style.normal.textColor = new Color(0.85f, 0.85f, 0.85f, 1);
            });

            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, textStyle, GUILayout.Width(EditorGUIUtility.labelWidth));
            GUILayout.Space(4);
            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Width(18), GUILayout.Width(18));
            rect.y += 2;
            EditorGUI.DrawRect(rect, Color.black);
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) value = !value;
            if (value)
            {
                EditorGUI.DrawRect(new Rect(rect.x + 4, rect.y + 4, rect.width - 8, rect.height - 8), highlightColor);
            }
            GUILayout.Space(4);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);

            return value;
        }

        private GUIStyle _textStyle = null;
        private GUIStyle TextStyle
        {
            get
            {
                if (_textStyle == null)
                {
                    _textStyle = GetUnityStyle("text", EditorStyles.label, (ref GUIStyle s) =>
                    {
                        s.normal.textColor = new Color(0.8679245f, 0.8679245f, 0.8679245f, 1.00f);
                        s.richText = true;
                    });
                }
                return _textStyle;
            }
        }
        #endregion

        #region Utility
        public static GUIStyle GetUnityStyle(string styleName, GUIStyle overlapStyle, OnCreateStyleOp onInitialize = null)
        {
            GUIStyle style;
            if (!cachedStyles.ContainsKey(styleName))
            {
                style = new GUIStyle(overlapStyle);
                style.richText = true;
                style.normal.textColor = Color.white;
                cachedStyles.Add(styleName, style);
                onInitialize?.Invoke(ref style);
            }
            else
            {
                style = cachedStyles[styleName];
            }
            return style;
        }
        public T GetImportFile<T>(string fileName)
        {
            var file = AssetDatabase.LoadAssetAtPath($"{IMPORT_FILES_PATH}{fileName}", typeof(T));
            return (T)Convert.ChangeType(file, typeof(T));
        }

        static void ClearConsole()
        {
            try
            {
                clearConsoleMethod.Invoke(new object(), null);
            }
            catch { }
        }

        static MethodInfo _clearConsoleMethod;
        static MethodInfo clearConsoleMethod
        {
            get
            {
                if (_clearConsoleMethod == null)
                {
                    Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                    Type logEntries = assembly.GetType("UnityEditor.LogEntries");
                    _clearConsoleMethod = logEntries.GetMethod("Clear");
                }
                return _clearConsoleMethod;
            }
        }

        private static Dictionary<string, Texture2D> cachedUnityIcons;
        public static Texture2D GetUnityIcon(string iconName)
        {
            if (cachedUnityIcons == null) cachedUnityIcons = new Dictionary<string, Texture2D>();
            if (!cachedUnityIcons.ContainsKey(iconName))
            {
                var icon = (Texture2D)EditorGUIUtility.IconContent(iconName).image;
                cachedUnityIcons.Add(iconName, icon);
            }
            return cachedUnityIcons[iconName];
        }

        [MenuItem("MFPS/Import")]
        static void Open()
        {
            GetWindow<MFPSImporter>(true);
        }
        #endregion

        public enum MFPSImportStatus
        {
            None,
            NoImported,
            Imported,
            ImportedWithFolderChanged,
            NoFoundImportFile
        }
    }

    #region EditorCoroutine
    public static class EditorCoroutines
    {
        public class Coroutine
        {
            public IEnumerator enumerator;
            public System.Action<bool> OnUpdate;
            public List<IEnumerator> history = new List<IEnumerator>();
        }

        static readonly List<Coroutine> coroutines = new List<Coroutine>();

        public static void Execute(IEnumerator enumerator, System.Action<bool> OnUpdate = null)
        {
            if (coroutines.Count == 0)
            {
                EditorApplication.update += Update;
            }
            var coroutine = new Coroutine { enumerator = enumerator, OnUpdate = OnUpdate };
            coroutines.Add(coroutine);
        }

        static void Update()
        {
            for (int i = 0; i < coroutines.Count; i++)
            {
                var coroutine = coroutines[i];
                bool done = !coroutine.enumerator.MoveNext();
                if (done)
                {
                    if (coroutine.history.Count == 0)
                    {
                        coroutines.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        done = false;
                        coroutine.enumerator = coroutine.history[coroutine.history.Count - 1];
                        coroutine.history.RemoveAt(coroutine.history.Count - 1);
                    }
                }
                else
                {
                    if (coroutine.enumerator.Current is IEnumerator)
                    {
                        coroutine.history.Add(coroutine.enumerator);
                        coroutine.enumerator = (IEnumerator)coroutine.enumerator.Current;
                    }
                }
                if (coroutine.OnUpdate != null) coroutine.OnUpdate(done);
            }
            if (coroutines.Count == 0) EditorApplication.update -= Update;
        }

        internal static void StopAll()
        {
            coroutines.Clear();
            EditorApplication.update -= Update;
        }

    }
    #endregion

    #region GUI Utility
    public class HorizontalCenterScope : GUI.Scope
    {
        public HorizontalCenterScope(params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(options);
            GUILayout.FlexibleSpace();
        }

        protected override void CloseScope()
        {
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    #endregion
}
#endif