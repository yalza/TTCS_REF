using UnityEngine;
using UnityEditor;
using MFPSEditor;
using Object = UnityEngine.Object;
using UnityEditor.SceneManagement;
using MFPS.Internal.Structures;

public class AddMapTutorial : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/map/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
        new NetworkImages{Name = "img-9.jpg", Image = null},
        new NetworkImages{Name = "img-10.jpg", Image = null},
        new NetworkImages{Name = "img-12.jpg", Image = null},
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Get Started", StepsLenght = 0 },
    new Steps { Name = "Set Up Scene", StepsLenght = 6 },
    new Steps { Name = "Tips", StepsLenght = 0 },
    new Steps { Name = "Maps Assets", StepsLenght = 0 },
    };
    //final required////////////////////////////////////////////////
    private Object m_SceneReference;
    string[] RequiredsPaths = new string[]
    {
        "Assets/MFPS/Content/Prefabs/Network/Managers/GameManager.prefab",
        "Assets/MFPS/Content/Prefabs/Network/Managers/AIManager.prefab",
        "Assets/MFPS/Content/Prefabs/Network/Managers/ItemManager.prefab",
        "Assets/MFPS/Content/Prefabs/GamePlay/GameModes/GameModes.prefab",
        "Assets/MFPS/Content/Prefabs/UI/UI.prefab",
    };
    bool[] RequiredInstanced = new bool[] { false, false, false, false, false, };
    string sceneName = "";
    Sprite scenePreview = null;
    AssetStoreAffiliate pcMapsAssets;
    AssetStoreAffiliate mobileMapsAssets;

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
        allowTextSuggestions = true;
    }

    public override void WindowArea(int window)
    {
        if (window == 0)
        {
            DrawStarted();
        }
        else if (window == 1) { DrawSetup(); }
        else if (window == 2) { DrawTips(); }
        else if (window == 3) { MapAssetsDoc(); }
    }

    void DrawStarted()
    {
        DrawNote("This tutorial will teach you how to add new maps to your MFPS game step by step.");
        DownArrow();
        DrawText("First, what you need is, of course, a map. By map, I mean a level environment design with all the art content models, prefabs, lights, sky, etc... placed in a way that looks like a battlefield.");
        DrawSuperText("There are some basic requirements for the map that all Unity users should know that applies for all games, not just MFPS:\n\n-<b>The map models/meshes must have colliders</b>, except for the models that are used only as decoration. All the models/meshes in your scene that the player is not supposed to go through must have a collider.\n\n-<b>Lighting</b>, lighting is a important part of map, but affects the performance of game in a big way. There a tons of tutorials out there on how to build a good scene lighting and bake your scene lightmap, e.g:\n<?link=https://learn.unity.com/tutorial/introduction-to-lighting-and-rendering#>https://learn.unity.com/tutorial/introduction-to-lighting-and-rendering#</link>\n\n-<b>Optimized performance over good looking graphics</b> We all love good graphics in a game, but a poorly optimized level could kill your game. MFPS is pretty well optimized in the code-side. but more important than the code, at least in this case, is the graphic optimization. There is a good official Unity post for graphic optimization, check it out:\n<?link=https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html>https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html</link>");
        DownArrow();
        DrawText("All right, if you have you map level design ready, lets continue");
    }

    void DrawSetup()
    {
        if (subStep == 0)
        {
            DrawText("As mentioned before you need to have your new map design ready, but not just as a prefab, you need to have it <b>placed in a Unity Scene which contains nothing else but your map environment</b>, if you don't have it, create a new Scene in (top editor menu ➔ <b>File ➔ New Scene</b>) > then in the new empty opened scene place your map environment or design it from scratch in that scene.\n                 \nOnce you have it, save the scene in your Unity Project <b>(File ➔ Save)</b> and then continue below.");
            DrawNote("<b>Make sure to delete all cameras in your map scene prior integrate with MFPS</b>. They are not needed as the required cameras will be created by MFPS.");
            DrawImage(GetServerImage(0));
            DownArrow();
            DrawText("Assign your Unity map scene <i>(.scene)</i> in the field below and <b>then click on the Continue</b> button to proceed with the scene validation.");
            Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Map Scene: ", GUILayout.Width(100));
            m_SceneReference = EditorGUILayout.ObjectField(m_SceneReference, typeof(SceneAsset), false) as SceneAsset;
            GUILayout.EndHorizontal();
            GUI.enabled = m_SceneReference != null;
            Space(5);
            if (GUILayout.Button("CONTINUE", MFPSEditorStyles.EditorSkin.customStyles[11]))
            {
                if (EditorSceneManager.GetActiveScene().name == m_SceneReference.name)
                {
                    NextStep();
                }
                else
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    string path = AssetDatabase.GetAssetPath(m_SceneReference);
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    NextStep();
                }
            }
            GUI.enabled = true;
        }
        else if (subStep == 1)
        {
            HideNextButton = isMissing();
            DrawText("Ok, now with the map scene open, let's drag the objects needed for the scene to work with MFPS, but first let's check which assets you have in the scene already, click on the button below to auto-detect.");
            Space();
            if (DrawButton("Check Scene"))
            {
                CheckScene();
            }
            if (sceneChecked)
            {
                EditorStyles.helpBox.richText = true;
                GUILayout.BeginVertical("box");
                GUILayout.Label(string.Format("Game Manager: {0}", RequiredInstanced[0] ? "<color=green>YES</color>" : "<color=red>NO</color>"), EditorStyles.helpBox);
                GUILayout.Label(string.Format("AI Manager: {0}", RequiredInstanced[1] ? "<color=green>YES</color>" : "<color=red>NO</color>"), EditorStyles.helpBox);
                GUILayout.Label(string.Format("Item Manager: {0}", RequiredInstanced[2] ? "<color=green>YES</color>" : "<color=red>NO</color>"), EditorStyles.helpBox);
                GUILayout.Label(string.Format("Game Mode Objects: {0}", RequiredInstanced[3] ? "<color=green>YES</color>" : "<color=red>NO</color>"), EditorStyles.helpBox);
                GUILayout.Label(string.Format("UI: {0}", RequiredInstanced[4] ? "<color=green>YES</color>" : "<color=red>NO</color>"), EditorStyles.helpBox);
                GUILayout.EndVertical();

                if (isMissing())
                {
                    DrawText("Your scene is not properly set up yet, click on the button below to automatically add the required components.");
                    Space();
                    if (DrawButton("Setup scene"))
                    {
                        for (int i = 0; i < RequiredsPaths.Length; i++)
                        {
                            Debug.Log(RequiredsPaths[i]);
                            if (RequiredInstanced[i] || RequiredsPaths[i] == string.Empty) continue;
                            GameObject prefab = AssetDatabase.LoadAssetAtPath(RequiredsPaths[i], typeof(GameObject)) as GameObject;
                            if (prefab != null)
                            {
                                PrefabUtility.InstantiatePrefab(prefab, EditorSceneManager.GetActiveScene());
                            }
                            else
                            {
                                Debug.LogWarning("Could not find the prefab at: " + RequiredsPaths[i]);
                            }
                        }
                        CheckScene();
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        Repaint();
                    }
                }
                else
                {
                    DrawText("All good, your scene have all require objects, continue with the next step.");
                }
            }
        }
        else if (subStep == 2)
        {
            DrawText("Ok, now after instanced the MFPS objects you will see in the Game View that there is a camera render from the top, that's the camera view that displays an aerial view when a player enters the room to select the team to join, so position it as you please in a space where the camera has a good view of the map.\n \nThe camera is located in <i>(Hierarchy) <b>GameManager ➔ Room Camera</b></i>");
            DrawImage(GetServerImage(1));
            DownArrow();
            DrawText("There are some objects that you need repositioned in your map, two of them are the Flags of CTF game mode, These are inside the object <b>GameModes -> CaptureOfFlag</b> in hierarchy," +
                " so select them and position them where you want them to be.");
            DrawImage(GetServerImage(2));
            DownArrow();
            DrawText("Do the same with the objects under <b>ItemManager</b> Position them around the map, but these are optional. These items (med kits and ammo) can be used as permanent items around the map " +
                "so player can use them during game, if you don't want to use them, simply delete them from the scene.");
            DrawImage(GetServerImage(3));
        }
        else if (subStep == 3)
        {
            DrawText("Now you need create some <b>Spawn Points</b> for each Team <i>(Team 1, Team 2 and For FFA)</i>. These spawn points are not specifically a static position, they are a <b>Spherical Area</b> where players can spawn, which means that the player will be instantiated in a random position inside of the radius of the area.\n \n<b><size=16>How create spawn points:</size></b>\n \nSimply create an empty game object in the scene and add the script <b>bl_SpawnPoint.cs</b> to it and assign the area and the team.\n \nBut to make it easier for you: Below you will have a button to create a spawn point, simply select the team and click on the button <b> Create Spawn Point.</b> ➔ A spawn point will be created and you can now select it in the scene view for position it on the map.");
            Space();
            GUILayout.BeginVertical();
            DrawText("<color=yellow>CREATE SPAWN POINTS</color>");
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Spawn Point For: ");
            SpawnTeam = (Team)EditorGUILayout.EnumPopup(SpawnTeam);
            if (GUILayout.Button("Create Spawn Point", EditorStyles.toolbarButton, GUILayout.Width(150)))
            {
                CreateSpawnPoint();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            DownArrow();
            DrawText("Once you create a spawn point and positioned it, you will have something similar to this:");
            DrawImage(GetServerImage(4));
            DrawText("This is the spawn point area. You can preview the available area to spawn a player inside with the semi-sphere and the real player size <i>(represented with the centered gizmo)</i>.\n \n You can increase or decrease the area in the bl_Spawnpoint " +
                "script attached to the object -> <b>Spawn Space</b>, Also, you can rotate it to set the default rotation (the direction spawned player will look).\n \nBe careful that the feet of the player gizmo and the sphere area are over the floor," +
                " otherwise the players will fall when spawned.");
            DrawText("\nCreate as many spawn points as you wish using the above button. Just make sure to create at least one spawn point for each team, once you finish, proceed with the next step.");
        }
        else if (subStep == 4)
        {
            DrawText("Now, in order for AI agents/bots to work on this map, you need to bake the <b>Navmesh</b>, In simple terms, a navmesh is an area where AI Agents can move, this area is calculated automatically by Unity when you bake it, but <b>it only takes into account meshes with a collider marked as static</b> in the scene, so you will have to make sure to mark your map meshes/terrain as static.\n \n For more info and a more detailed explanation of how to bake a Navmesh, check out this link:");
            if (DrawLinkText("https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html"))
            {
                Application.OpenURL("https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html");
            }
            DownArrow();
            DrawText("Once you bake your Navmesh you will have something like this:");
            DrawImage(GetServerImage(5));
            DrawText("The blue areas represent the walkable area for bots\n \nSo, now you need to position the <b>AI Cover Points.</b> These are empty game objects with a bl_AICoverPoint script attached that work" +
                " as reference positions for bots when they need to cover. By default, the AIManager object contains some of these points. You can find them in <b>AIManager -> *</b>");
            DrawImage(GetServerImage(6), TextAlignment.Center);
            DrawText("You can use as many as you want, if you don't need that many, simply delete some of them. if you want more, simply duplicate them.\n \nSelect each individually and position them in a strategic point on the map; you can preview the area enabling <b>Show Gizmos</b> in <i>AIManager ➔ bl_AIManager ➔ <b>Show Gizmos</b></i>");
            DrawImage(GetServerImage(7));
            DrawText("<i>For more information about the AI Cover points check this:</i>");
            if (Buttons.OutlineButton("AI Cover Points"))
            {
                var tut = GetWindow<TutorialBots>();
                tut.windowID = 1;
            }
        }
        else if (subStep == 5)
        {
            DrawText("Now you have your scene set up and ready!\n \nAll you have to do now is list it in the available scene list so that players can select your scene when creating a room. You can do it by manually adding a new field in the list <b>AllScenes</b> in GameData: <b><i>(Resources folder of MFPS) GameData ➔ AllScenes ➔ Add a new field</i></b> and fill in the required info\n\nor <b>you can do it automatically here</b>, Simply set a name for the map and a sprite preview below:");
            DrawImage(GetServerImage(8));
            DownArrow();
            GUILayout.BeginVertical("box");
            sceneName = EditorGUILayout.TextField("Map Custom Name", sceneName);
            scenePreview = EditorGUILayout.ObjectField("Map Preview", scenePreview, typeof(Sprite), false) as Sprite;
            GUI.enabled = m_SceneReference == null;
            m_SceneReference = EditorGUILayout.ObjectField("Scene", m_SceneReference, typeof(SceneAsset), false) as SceneAsset;
            GUI.enabled = !string.IsNullOrEmpty(sceneName) && m_SceneReference != null;
            if (DrawButton("List Map"))
            {
                if (!bl_GameData.Instance.AllScenes.Exists(x => x.ShowName == sceneName))
                {
                    var si = new MapInfo();
                    si.ShowName = sceneName;
                    si.m_Scene = m_SceneReference;
                    si.Preview = scenePreview;
                    bl_GameData.Instance.AllScenes.Add(si);
                    EditorUtility.SetDirty(bl_GameData.Instance);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    var original = EditorBuildSettings.scenes;
                    var newSettings = new EditorBuildSettingsScene[original.Length + 1];
                    System.Array.Copy(original, newSettings, original.Length);
                    string path = AssetDatabase.GetAssetPath(m_SceneReference);
                    var sceneToAdd = new EditorBuildSettingsScene(path, true);
                    newSettings[newSettings.Length - 1] = sceneToAdd;
                    EditorBuildSettings.scenes = newSettings;
                    sceneListed = true;
                }
                else
                {
                    Debug.LogWarning("A map with this name is already listed.");
                }
            }
            GUI.enabled = true;
            GUILayout.EndVertical();
            if (sceneListed)
            {
                DownArrow();
                DrawText("Nice! That's it, you have added a new map to your game!, now you can select the map in the Main Menu / Lobby -> Create Room.\n \nRead the next section for some tips.");
                DrawImage(GetServerImage(9));
            }
        }
    }

    void DrawTips()
    {
        DrawText("<b><size=16>Optimization</size></b>");
        DrawText("As I mentioned at the beginning, if you want to your game to perform well, with a good frame rate when there are more than 8 players in the same map, " +
                 "you'll need to optimize the graphic content of your map, Models, Textures, Shaders, etc... This is not as crucial in a single player game, but in multiplayer, the local client handles both the local info " +
            "and the info received from remote players. So, the optimization is really, really important.\nBelow are some links you may find useful for Unity Graphic Optimization");
        if (DrawLinkText("https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html"))
        {
            Application.OpenURL("https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html");
        }

        if (DrawLinkText("https://unity3d.com/es/learn/tutorials/temas/performance-optimization/optimizing-graphics-rendering-unity-games"))
        {
            Application.OpenURL("https://unity3d.com/es/learn/tutorials/temas/performance-optimization/optimizing-graphics-rendering-unity-games");
        }

        if (DrawLinkText("https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html"))
        {
            Application.OpenURL("https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html");
        }

        if (DrawLinkText("https://cgcookie.com/articles/maximizing-your-unity-games-performance"))
        {
            Application.OpenURL("https://cgcookie.com/articles/maximizing-your-unity-games-performance");
        }
        Space(20);
        DrawText("<b><size=16>Level Design.</size></b>");
        DrawText("Here you have some resources that you can use to learn or improve your level design skills from AAA or more experienced artists:");
        if (DrawLinkText("Practical Guide on First Person Level Design"))
        {
            Application.OpenURL("https://medium.com/ironequal/practical-guide-on-first-person-level-design-e187e45c744c");
        }
        if (DrawLinkText("Multiplayer Map Theory (Gears of War)"))
        {
            Application.OpenURL("https://docs.unrealengine.com/udk/Three/GearsMultiplayerMapTheory.html");
        }
        if (DrawLinkText("Level Design Guidelines"))
        {
            Application.OpenURL("http://www.mikebarclay.co.uk/my-level-design-guidelines/");
        }
    }

    void MapAssetsDoc()
    {
        DrawText("Here you will find two lists of assets available in the Asset Store picked by me that work for action-shooter games that you may find useful in case you are looking to add more maps to MFPS.\n \nThe two lists are separated into assets that work best for high-end platforms like <b>PC and Console</b> and the other list for <b>Mobile-Friendly</b> assets that you can use if you are looking for maps for your mobile game.");

        using (new GUILayout.HorizontalScope())
        {
            EditorGUILayout.BeginVertical();
            DrawTitleText("High-Quality Maps");
            if (pcMapsAssets == null)
            {
                pcMapsAssets = new AssetStoreAffiliate();
                pcMapsAssets.randomize = true;
                pcMapsAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673399302530/widget-medium");
                pcMapsAssets.FixedHeight = 360;
            }
            else
                pcMapsAssets.OnGUI();
            GUILayout.Space(20);
            DrawTitleText("Mobile-Friendly Maps");
            if (mobileMapsAssets == null)
            {
                mobileMapsAssets = new AssetStoreAffiliate();
                mobileMapsAssets.randomize = true;
                mobileMapsAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673399298719/widget-medium");
                mobileMapsAssets.FixedHeight = 360;
            }
            else
                mobileMapsAssets.OnGUI();
            EditorGUILayout.EndVertical();
            GUILayout.Space(60);
        }
    }

    Team SpawnTeam = Team.All;
    bool sceneChecked = false;
    bool sceneListed = false;
    void CheckScene()
    {
        RequiredInstanced[0] = FindObjectOfType<bl_GameManager>() != null;
        RequiredInstanced[1] = FindObjectOfType<bl_AIMananger>() != null;
        RequiredInstanced[2] = FindObjectOfType<bl_ItemManagerBase>() != null;
        RequiredInstanced[3] = GameObject.Find("GameModes") != null;
        RequiredInstanced[4] = FindObjectOfType<bl_UIReferences>() != null;
        sceneChecked = true;
    }

    void CreateSpawnPoint()
    {
        GameObject parent = GameObject.Find("SpawnPoints");
        if (parent == null)
        {
            parent = new GameObject("SpawnPoints");
            parent.transform.position = Vector3.zero;
        }
        if (SpawnTeam == Team.Team1)
        {
            GameObject t1p = GameObject.Find(string.Format("{0} Spawnpoints", bl_GameData.Instance.Team1Name));
            if (t1p == null)
            {
                t1p = new GameObject(string.Format("{0} Spawnpoints", bl_GameData.Instance.Team1Name));
                t1p.transform.parent = parent.transform;
                t1p.transform.localPosition = Vector3.zero;
            }
            GameObject spawn = new GameObject(string.Format("SpawnPoint [{0}]", bl_GameData.Instance.Team1Name));
            bl_SpawnPoint sp = spawn.AddComponent<bl_SpawnPoint>();
            sp.team = SpawnTeam;
            spawn.transform.parent = t1p.transform;
            Selection.activeObject = spawn;
            EditorGUIUtility.PingObject(spawn);
            var view = (SceneView)SceneView.sceneViews[0];
            spawn.transform.position = view.camera.transform.position + view.camera.transform.forward * 10;
        }
        else if (SpawnTeam == Team.Team2)
        {
            GameObject t1p = GameObject.Find(string.Format("{0} Spawnpoints", bl_GameData.Instance.Team2Name));
            if (t1p == null)
            {
                t1p = new GameObject(string.Format("{0} Spawnpoints", bl_GameData.Instance.Team2Name));
                t1p.transform.parent = parent.transform;
                t1p.transform.localPosition = Vector3.zero;
            }
            GameObject spawn = new GameObject(string.Format("SpawnPoint [{0}]", bl_GameData.Instance.Team2Name));
            bl_SpawnPoint sp = spawn.AddComponent<bl_SpawnPoint>();
            sp.team = SpawnTeam;
            spawn.transform.parent = t1p.transform;
            Selection.activeObject = spawn;
            EditorGUIUtility.PingObject(spawn);
            var view = (SceneView)SceneView.sceneViews[0];
            spawn.transform.position = view.camera.transform.position + view.camera.transform.forward * 10;
        }
        else
        {
            GameObject t1p = GameObject.Find(string.Format("{0} Spawnpoints", "ALL"));
            if (t1p == null)
            {
                t1p = new GameObject(string.Format("{0} Spawnpoints", "ALL"));
                t1p.transform.parent = parent.transform;
                t1p.transform.localPosition = Vector3.zero;
            }
            GameObject spawn = new GameObject(string.Format("SpawnPoint [{0}]", "ALL"));
            bl_SpawnPoint sp = spawn.AddComponent<bl_SpawnPoint>();
            sp.team = Team.All;
            spawn.transform.parent = t1p.transform;
            Selection.activeObject = spawn;
            EditorGUIUtility.PingObject(spawn);
            var view = (SceneView)SceneView.sceneViews[0];
            spawn.transform.position = view.camera.transform.position + view.camera.transform.forward * 10;
        }
    }

    private bool isMissing()
    {
        for (int i = 0; i < RequiredInstanced.Length; i++)
        {
            if (RequiredInstanced[i] == false) return true;
        }
        return false;
    }

    [MenuItem("MFPS/Tutorials/Add Map", false, 500)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AddMapTutorial));
    }
}