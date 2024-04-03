using UnityEngine;
using UnityEditor;
using MFPSEditor;

public class AddPlayerTutorial : TutorialWizard
{

    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/player/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "dDHltGGDrAA", Image = null, Type = NetworkImages.ImageType.Youtube},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
        new NetworkImages{Name = "https://www.lovattostudio.com/en/wp-content/uploads/2017/03/player-selector-product-cover-925x484.png",Type = NetworkImages.ImageType.Custom},
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "3DModel", StepsLenght = 0, DrawFunctionName = nameof(DrawModelInfo) },
    new Steps { Name = "Ragdolled", StepsLenght = 3, DrawFunctionName = nameof(DrawRagdolled) },
    new Steps { Name = "Player Prefab", StepsLenght = 6, DrawFunctionName = nameof(DrawPlayerPrefab) },
    new Steps { Name = "Player Models Assets", StepsLenght = 1, DrawFunctionName = nameof(PlayerModelAssetsDoc) },
    };
    private readonly GifData[] AnimatedImages = new GifData[]
    {
        new GifData{ Path = "addpt3.gif" },
        new GifData{ Path = "addnewwindowfield.gif" },
    };
    //final required////////////////////////////////////////////////

    private GameObject PlayerInstantiated;
    private GameObject PlayerModel;
    private Animator PlayerAnimator;
    private Avatar PlayerModelAvatar;
    private string LogLine = "";
    private ModelImporter ModelInfo;
    Editor p1editor;
    AssetStoreAffiliate playerAssets;
    public TPWeaponOrientationMode weaponOrientationMode = TPWeaponOrientationMode.KeepSameLocation;
    public bool autoPoseAiming = true;

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
        if (playerAssets == null)
        {
            playerAssets = new AssetStoreAffiliate();
            playerAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/157287/widget-medium");
            playerAssets.FixedHeight = 420;
            playerAssets.randomize = true;
        }
        allowTextSuggestions = true;
    }

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }

    void DrawModelInfo()
    {
        DrawText("This tutorial will guide you step by step to replace the Player Model of the player prefabs, what you need is:");
        DrawHorizontalColumn("Player Model", "A Humanoid <b>Rigged</b> 3D Model with the standard rigged bones or any rigged that work with the unity re-targeting animator system.");
        DrawText("The Model Import <b>Rig</b> setting has to be set as <b>Humanoid</b> in order to work with retargeting animations, for it select the player model <i>(the model not a prefab)</i> and in the inspector window you will see a toolbar, go to the Rig tab and set the <b>Animation Type</b> as Humanoid, the settings should look like this:");
        DrawServerImage("img-0.png");
        DownArrow();
        DrawNote("<b>Important:</b> your model should have a correct <b>T-Pose skeleton</b> to work correctly with the re-targeting animations, if your character model have a wrong posed skeleton the animations will look weird in the player model, in order to fix the skeleton pose you can follow this video tutorial:");
        DrawYoutubeCover("Adjusting Avatar for correct animation retargeting", GetServerImage(1), "https://www.youtube.com/watch?v=dDHltGGDrAA");
    }

    void DrawRagdolled()
    {
        if (subStep == 0)
        {
            HideNextButton = true;
            DrawText("All right, with the model ready it's time to start setting it up.\n \nThe first thing that you need to do is make a ragdoll of your new player model. Normally in Unity, you make a ragdoll manually with GameObject ➔ 3D Object ➔ Ragdoll, and then assign every player bone in the wizard window manually, but this tool will make this automatically, you simply need to drag the player model below.");
            DownArrow();
            DrawText("Drag here your player model from the <b>Project View</b> window");
            PlayerModel = EditorGUILayout.ObjectField("Player Model", PlayerModel, typeof(GameObject), false) as GameObject;
            GUI.enabled = PlayerModel != null;
            if (DrawButton("Continue"))
            {
                AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(PlayerModel));
                if (importer != null)
                {
                    ModelInfo = importer as ModelImporter;
                    if (ModelInfo != null)
                    {
                        if (ModelInfo.animationType != ModelImporterAnimationType.Human)
                        {
                            ModelInfo.animationType = ModelImporterAnimationType.Human;
                            EditorUtility.SetDirty(ModelInfo);
                            ModelInfo.SaveAndReimport();
                        }
                        if (ModelInfo.animationType == ModelImporterAnimationType.Human)
                        {
                            PlayerInstantiated = PrefabUtility.InstantiatePrefab(PlayerModel) as GameObject;
                            UnPackPrefab(PlayerInstantiated);
                            PlayerInstantiated.transform.rotation = Quaternion.identity;
                            PlayerAnimator = PlayerInstantiated.GetComponent<Animator>();
                            PlayerModelAvatar = PlayerAnimator.avatar;
                            var view = (SceneView)SceneView.sceneViews[0];
                            view.camera.transform.position = PlayerInstantiated.transform.position + ((PlayerInstantiated.transform.forward * 10) + Vector3.up);
                            view.LookAt(PlayerInstantiated.transform.position);
                            EditorGUIUtility.PingObject(PlayerInstantiated);
                            Selection.activeTransform = PlayerInstantiated.transform;
                            subStep++;
                        }
                        else
                        {
                            LogLine = "Your models is not setup as a <b>Humanoid</b> rig, setup it:";
                        }
                    }
                    else
                    {
                        LogLine = "Please select the Model asset from the Project View not a prefab of the model.";
                    }
                }
                else { LogLine = "Please select the Model asset from the Project View not a prefab of the model."; }
            }
            GUI.enabled = true;
            if (!string.IsNullOrEmpty(LogLine))
            {
                GUILayout.Label(LogLine);
                if (LogLine.Contains("Humanoid"))
                {
                    DrawImage(GetServerImage(0));
                }
            }
        }
        else if (subStep == 1)
        {
            HideNextButton = false;
            GUI.enabled = false;
            GUILayout.BeginVertical("box");
            PlayerInstantiated = EditorGUILayout.ObjectField("Player Prefab", PlayerInstantiated, typeof(GameObject), false) as GameObject;
            PlayerModelAvatar = EditorGUILayout.ObjectField("Avatar", PlayerModelAvatar, typeof(Avatar), true) as Avatar;
            MeshSizeChecker meshChecker = null;
            if (PlayerInstantiated != null)
            {
                meshChecker = PlayerInstantiated.GetComponent<MeshSizeChecker>();
                if (meshChecker == null) meshChecker = PlayerInstantiated.AddComponent<MeshSizeChecker>();
                meshChecker.Check();

                GUILayout.Label(string.Format("Model Height: <b>{0}</b> | Expected Height: <b>2</b>", meshChecker.Height));
                if (ModelInfo != null) GUILayout.Label(string.Format("Model Rig: {0}", ModelInfo.animationType.ToString()));

                GUI.enabled = true;
                if (meshChecker.Height < 1.9f)
                {
                    GUILayout.Label("<color=yellow>the size of the model is too small</color>, you want try to resize it automatically?", EditorStyles.label);
                    if (DrawButton("Yes, Resize automatically"))
                    {
                        Vector3 v = PlayerInstantiated.transform.localScale;
                        float dif = 2f / meshChecker.Height;
                        v = v * dif;
                        PlayerInstantiated.transform.localScale = v;
                    }
                }
                else if (meshChecker.Height > 2.25f)
                {
                    GUILayout.Label("<color=yellow>the size of the model is too large</color>, you want resize it automatically?", EditorStyles.label);
                    if (DrawButton("Yes, Resize automatically"))
                    {
                        Vector3 v = PlayerInstantiated.transform.localScale;
                        float dif = meshChecker.Height / 2;
                        v = v / dif;
                        PlayerInstantiated.transform.localScale = v;
                    }
                }
            }

            GUILayout.EndVertical();
            GUI.enabled = true;
            if (PlayerModelAvatar != null && PlayerAnimator != null)
            {
                DownArrow();
                DrawText("Everything is ready to create the ragdoll, Click on the button below to build it.");
                if (DrawButton("Build Ragdoll"))
                {
                    if (AutoRagdoller.Build(PlayerAnimator))
                    {
                        if (meshChecker != null) DestroyImmediate(meshChecker);
                        else
                        {
                            meshChecker = PlayerInstantiated.GetComponent<MeshSizeChecker>();
                            if (meshChecker != null) DestroyImmediate(meshChecker);
                        }

                        var view = (SceneView)SceneView.sceneViews[0];
                        view.ShowNotification(new GUIContent("Ragdoll Created!"));
                        NextStep();
                    }
                }
            }
            else
            {
                GUILayout.Label("<color=yellow>Hmm... something is happening here, can't get the model avatar.</color>", EditorStyles.label);
            }
        }
        else if (subStep == 2)
        {
            DrawText("Right now your player model <i>(in the scene)</i> should look similar to this:");
            DrawImage(GetServerImage(2));
            DownArrow();
            DrawText("Now, these <b>Box</b> and <b>Capsule</b> Colliders are the player HitBoxes <i>(the colliders that detect when a bullet hit the player)</i>, in some models these colliders may not be place/oriented in the right axes causing a problem which will be that some parts of the player will not be hitteable in game.\n\nSo make sure all the colliders cover the player model by modifying the collider values if is necessary.\n\nIf all seems good, you are ready to go to the next step.");

        }
    }

    void DrawPlayerInstanceButton(GameObject player)
    {
        if (player == null) return;

        if (GUILayout.Button(player.name, GUILayout.Width(150)))
        {
            PlayerModel = PlayerInstantiated;
            PlayerInstantiated = PrefabUtility.InstantiatePrefab(player) as GameObject;
            UnPackPrefab(PlayerInstantiated);
            Selection.activeObject = PlayerInstantiated;
            EditorGUIUtility.PingObject(PlayerInstantiated);
            NextStep();
        }
        GUILayout.Space(5);
    }

    void DrawPlayerPrefab()
    {
        if (subStep == 0)
        {
            DrawText("Okay, now that we have the player model ragdolled, we can add it to a player prefab, for it we would open one of the existing player prefabs.\n\nBelow you will have a list of all your available player prefabs, click on the one that you want to use as reference to replace their model.");
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical();
                {
                    DrawPlayerInstanceButton(bl_GameData.Instance.Player1.gameObject);
                    DrawPlayerInstanceButton(bl_GameData.Instance.Player2.gameObject);
#if PSELECTOR
                    foreach (var p in bl_PlayerSelector.Data.AllPlayers)
                    {
                        if (p == null || p.Prefab == null) continue;
                        DrawPlayerInstanceButton(p.Prefab);
                    }
#endif
                }
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        else if (subStep == 1)
        {
            GUI.enabled = (PlayerInstantiated == null || PlayerModel == null);
            PlayerInstantiated = EditorGUILayout.ObjectField("Player Prefab", PlayerInstantiated, typeof(GameObject), true) as GameObject;
            if (PlayerModel == null)
            {
                GUILayout.Label("<color=yellow>Select the ragdolled player model (from hierarchy)</color>");
            }
            PlayerModel = EditorGUILayout.ObjectField("Player Model", PlayerModel, typeof(GameObject), true) as GameObject;
            GUI.enabled = true;
            if (PlayerModel != null && PlayerInstantiated != null)
            {
                DownArrow();
                DrawText("All good, click in the button below to setup the model in the player prefab.");
                GUILayout.Space(10);
                var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);
                autoPoseAiming = MFPSEditorStyles.FeatureToogle(r, autoPoseAiming, "Automatically Pose Aiming");
                GUILayout.Space(4);
                weaponOrientationMode = (TPWeaponOrientationMode)EditorGUILayout.EnumPopup("TPWeapon Reposition Method", weaponOrientationMode);
                GUILayout.Space(20);

                using (new CenteredScope())
                {
                    if (Buttons.GlowButton("<color=#1e1e1e>SETUP MODEL</color>", Style.highlightColor, GUILayout.Height(30), GUILayout.Width(200)))
                    {
                        SetUpModelInPrefab();
                        NextStep();
                    }
                }
            }
        }
        else if (subStep == 2)
        {
            string pin = PlayerInstantiated == null ? "MPlayer" : PlayerInstantiated.name;
            DrawText($"If all works as expected, you should see <b>just</b> a log in the console: <b><i>Player model integrated</i></b>.\n\nIf it's so, you also should see inside the player prefab instanced in the scene hierarchy: <b>{pin} -> RemotePlayer -></b> both models the old one <i>(marked with <b>(DELETE THIS)</b> at the end of the name) </i> and the new one.");
            DrawServerImage("img-3.png");
            DrawNote("The old model is not automatically deleted just in case you see a noticeable difference in the position, scale, or rotation between both models, if is this the case you can manually adjust the position, rotation, or scale of the new model using the old model as a reference, if that is not the case is everything seems correct, you can simply delete the old model.");
            DownArrow();
            DrawText("Ok, now there is one step that you need to do manually.\n \nThe TPWeapons <i>(The third person weapons)</i> has been moved from the old player model to the new one, but because the models pretty much always have a different local axis orientation, the TPWeapons will not be correctly located/oriented in the new player hands, so you need repositioned/re-oriented them manually, you can start by reorienting the <b>TPWeapon Root</b> which is the parent transform where all the TPWeapons are, it is the object called <b>RemoteWeapons</b>.\n \nThis is an example of how the weapons may look after replacing the player model <i>(more or less)</i>:");
            DrawNote("Since version 1.8 the player is automatically placed in an <i><b>Aiming Pose</b></i> that facilitates repositioning the weapons, so even if the player doesn't look like the image below, the logic is the same: <b>place the weapons simulating as if the player is holding it with their hands.</b>");
            DrawImage(GetServerImage(4));
            DownArrow();
            DrawText("In order to repositioned/re-oriented them, select the <b>RemoteWeapons</b> object which is inside of the player prefab <i>(inside of the right hand of the player model)</i>, or click in the button bellow to try to ping it automatically on the hierarchy window.\n");
            if (DrawButton("Ping RemoteWeapons"))
            {
                if (PlayerInstantiated != null)
                {
                    Transform t = PlayerInstantiated.GetComponent<bl_PlayerNetwork>().NetworkGuns[0].transform.parent;
                    Selection.activeTransform = t;
                    EditorGUIUtility.PingObject(t);
                    if (t != null)
                        NextStep();
                }
            }

        }
        else if (subStep == 3)
        {
            DrawText("Now the RemoteWeapons object should be selected and framed in the hierarchy window, to preview the position of the weapons if you don't have a weapon active/showing select one from inside of the object <i>(RemoteWeapons)</i> and make it visible by enabling the game object, or if you have more than one enabled, disable all of them is just leave one showing to make things more clear.\n\nThen select the <b>RemoteWeapons</b> <i>(not a weapon child)</i> parent again and rotate/move to positioned it simulating that the player is holding it in the right hand, something like this:");
            DrawNote("Since version 1.8 the player is automatically placed in an <i><b>Aiming Pose</b></i> that facilitates repositioning the weapons, so even if the player doesn't look like the image below, the logic is the same: <b>place the weapons simulating as if the player is holding it with their hands.</b>");
            DrawImage(GetServerImage(5));
            DrawNote("You can also check how each TPWeapons looks by activating the weapon in the hierarchy <i>(inside of the RemoteWeapons transform)</i>, you adjust each weapon to pose more accurately with the new player.");
            DownArrow();
            DrawSuperText("<?background=#CCCCCCFF>AIM POSITION</background>\n\nOnce you finish positioning the weapons, again, deactivate all of them <i>(the TPWeapons)</i> but one in order to preview the pose.\n \nThe arms aim position is controlled by IK and the aim position can be customized from the inspector, for it select the player model inside the player prefab the one marked with <b>(NEW)</b> inside the RemotePlayer object ➔ then go to the inspector window ➔ bl_PlayerIK ➔ at the bottom of the script inspector ➔ click on the button <b>Preview Aim Position</b> ➔ move the auto-selected pivot and you will see how the arms move with it ➔ positioned the pivot in the place that you want to be the Aim position ➔ once you got it, click on the <b>DONE</b> yellow button and that's.");
            DrawNote("Make sure the <b>Gismoz</b> is enabled in the Editor otherwise you won't be able to move the pivot.");
            DrawAnimatedImage(0);
            DownArrow();
            DrawText("When you are done, make sure that if you haven't deleted the old model yet, you should do it now:");
            DrawImage(GetServerImage(6));
        }
        else if (subStep == 4)
        {
            DrawText("Now you need to copy this prefab inside the <b>Resources</b> folder, by dragging it to: MFPS -> Resources. Rename it if you wish.");
            DrawImage(GetServerImage(7));
            DownArrow();
            DrawText("Now you need assign this new player prefab for use by one of the Teams (team 1 or team 2). To do this, go to GameData (in Resources folder too) -> Players section, and in the corresponding field (Team1 or Team2), " +
                "drag the new player prefab.");
            DrawImage(GetServerImage(8));
        }
        else if (subStep == 5)
        {
            DrawText("That's it! You have your new player model integrated!.\n\n Please note: Some models are not fully compatible with the default player animations re-targeting, causing " +
                "some of your animations to look awkward. Unfortunately, there is nothing we can do to fix it automatically. To fix it you have two options: Edit the animation or replace with another that you know" +
                " works in your model, check the documentation for more info of how replace animations.");
            GUILayout.Space(7);
            DrawText("Do you want to have multiple player options so a player has more players to choose from?, Check out <b>Player Selector</b> Addon, with which you can add as many player models as you want: ");
            GUILayout.Space(5);
            if (DrawButton("PLAYER SELECTOR"))
            {
                Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/player-selector/");
            }
            DrawImage(GetServerImage(9));
        }
    }

    void PlayerModelAssetsDoc()
    {
        DrawText("Here you have a list of Asset Store player model assets that you can use to integrate in MFPS");
        Space(10);
        playerAssets.OnGUI();
    }

    void UnPackPrefab(GameObject prefab)
    {
#if UNITY_2018_3_OR_NEWER
        if (PrefabUtility.GetPrefabInstanceStatus(prefab) == PrefabInstanceStatus.Connected)
        {
            PrefabUtility.UnpackPrefabInstance(prefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
#endif
    }

    void SetUpModelInPrefab()
    {
        UnPackPrefab(PlayerModel);
        UnPackPrefab(PlayerInstantiated);
        GameObject TempPlayerPrefab = PlayerInstantiated;
        GameObject TempPlayerModel = PlayerModel;

        //change name of prefabs to identify
        PlayerInstantiated.gameObject.name += " [NEW]";
        PlayerInstantiated.transform.SetAsLastSibling();
        PlayerModel.name += " [NEW]";

        // get the current player model
        GameObject RemoteChildPlayer = TempPlayerPrefab.GetComponentInChildren<bl_PlayerAnimationsBase>().gameObject;
        GameObject ActualModel = TempPlayerPrefab.GetComponentInChildren<bl_PlayerIKBase>().gameObject;
        Transform NetGunns = TempPlayerPrefab.GetComponent<bl_PlayerNetwork>().NetworkGuns[0].transform.parent;

        //set the new model to the same position as the current model
        TempPlayerModel.transform.parent = RemoteChildPlayer.transform;
        TempPlayerModel.transform.localPosition = ActualModel.transform.localPosition;
        TempPlayerModel.transform.localRotation = ActualModel.transform.localRotation;

        //add and copy components of actual player model
        bl_PlayerIK ahl = ActualModel.GetComponent<bl_PlayerIK>();
        if (TempPlayerModel.GetComponent<Animator>() == null) { TempPlayerModel.AddComponent<Animator>(); }
        Animator NewAnimator = TempPlayerModel.GetComponent<Animator>();

        if (ahl != null)
        {
            bl_PlayerIK newht = TempPlayerModel.AddComponent<bl_PlayerIK>();
            newht.Target = ahl.Target;
            newht.Body = ahl.Body;
            newht.Weight = ahl.Weight;
            newht.Head = ahl.Head;
            newht.Lerp = ahl.Lerp;
            newht.Eyes = ahl.Eyes;
            newht.Clamp = ahl.Clamp;
            newht.useFootPlacement = ahl.useFootPlacement;
            newht.FootHeight = ahl.FootHeight;
            newht.FootLayers = ahl.FootLayers;
            newht.AimSightPosition = ahl.AimSightPosition;
            newht.HandOffset = ahl.HandOffset;
            newht.TerrainOffset = ahl.TerrainOffset;
            newht.leftFeetRotationOffset = ahl.leftFeetRotationOffset;
            newht.rightFeetRotationOffset = ahl.rightFeetRotationOffset;
            newht.leftKneeTarget = ahl.leftKneeTarget;
            newht.rightKneeTarget = ahl.rightKneeTarget;

            Animator oldAnimator = ActualModel.GetComponent<Animator>();
            NewAnimator.runtimeAnimatorController = oldAnimator.runtimeAnimatorController;
            NewAnimator.applyRootMotion = oldAnimator.hasRootMotion;
            if (NewAnimator.avatar == null)
            {
                NewAnimator.avatar = oldAnimator.avatar;
                Debug.LogWarning("Your new model doesn't have a avatar, that can cause some problems with the animations, be sure to add it manually.");
            }
        }
        Transform RightHand = NewAnimator.GetBoneTransform(HumanBodyBones.RightHand);

        if (RightHand == null)
        {
            Debug.Log("Can't get right hand from new model, are u sure that is an humanoid rig?");
            return;
        }

        var tempPlayerReferences = TempPlayerPrefab.GetComponent<bl_PlayerReferences>();
        tempPlayerReferences.PlayerAnimator = NewAnimator;
        var pa = TempPlayerPrefab.transform.GetComponentInChildren<bl_PlayerAnimationsBase>();
        var tempRagdoll = TempPlayerPrefab.transform.GetComponentInChildren<bl_PlayerRagdoll>();
        pa.Animator = NewAnimator;
        ActualModel.SetActive(false);
        tempRagdoll.SetUpHitBoxes();
        tempPlayerReferences.hitBoxManager.SetupHitboxes(NewAnimator);
        tempPlayerReferences.playerSettings.carrierPoint = NewAnimator.GetBoneTransform(HumanBodyBones.UpperChest);

        if (tempPlayerReferences.gunManager != null)
        {
            // hide the FPWeapons so the TPWeapons can be seen clearly.
            foreach (var weapon in tempPlayerReferences.gunManager.AllGuns)
            {
                if (weapon == null) continue;
                weapon.gameObject.SetActive(false);
            }
        }

        EditorUtility.SetDirty(tempPlayerReferences);

        if (autoPoseAiming)
        {
            if (pa.Animator != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    pa.Animator.Update(0);
                }
            }
        }

        if (RightHand != null)
        {
            var npos = NetGunns.localPosition;
            var nrot = NetGunns.localRotation;

            NetGunns.parent = RightHand;
            if (weaponOrientationMode == TPWeaponOrientationMode.SameLocalAsOldModel)
            {
                NetGunns.localPosition = npos;
                NetGunns.localRotation = nrot;
            }
            else if (weaponOrientationMode == TPWeaponOrientationMode.ResetInNewModel)
            {
                NetGunns.localPosition = Vector3.zero;
                NetGunns.rotation = RightHand.rotation;
            }
            else
            {

            }
        }
        else
        {
            Debug.Log("Can't find right hand");
        }

        ActualModel.name += " (DELETE THIS)";
        ActualModel.SetActive(false);

        var view = (SceneView)SceneView.sceneViews[0];
        var pbounds = MFPSEditorUtils.GetTransformBounds(tempPlayerReferences.gameObject);
        pbounds.center += Vector3.up * 0.5f;
        view.LookAt(pbounds.center);
        //view.Frame(pbounds);

        view.ShowNotification(new GUIContent("Player Setup"));
        Debug.Log("Player model integrated.");
    }

    private Rigidbody[] GetRigidBodys(Transform t)
    {
        Rigidbody[] R = t.GetComponentsInChildren<Rigidbody>();
        return R;
    }

    private Collider[] GetCollider(Transform t)
    {
        Collider[] R = t.GetComponentsInChildren<Collider>();
        return R;
    }

    public enum TPWeaponOrientationMode
    {
        SameLocalAsOldModel,
        ResetInNewModel,
        KeepSameLocation
    }

    [MenuItem("MFPS/Tutorials/Add Player", false, 500)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AddPlayerTutorial));
    }
}