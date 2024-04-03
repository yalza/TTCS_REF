using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;

public class TutorialBots : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/bots/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Replace Bot Model", StepsLenght = 3 },
     new Steps { Name = "Cover Points", StepsLenght = 0 },
     new Steps { Name = "bl_AICoverPointManager", StepsLenght = 0 },
     new Steps { Name = "Bots Names", StepsLenght = 0 },
    };
    //final required////////////////////////////////////////////////

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder);
        allowTextSuggestions = true;
        FetchWebTutorials("mfps2/tutorials/");
    }

    public override void WindowArea(int window)
    {
        if (window == 0)
        {
            DrawModel();
        }
        else if (window == 1) CoverPointDoc();
        else if (window == 2) AICoverPointManagerDoc();
        else if (window == 3) BotsNameDoc();
    }

    void CoverPointDoc()
    {
        DrawSuperText("The MFPS AI system comes with support for Cover Points, in essence, <b>are points strategically placed around the map which improves the AI navigation path maker</b>, based on some conditions in the battlefield, bots used these points to add some sort of randomness to their behavior when they are in battle, bots used these points to cover from enemies or as a random navigation target.\n \nThe usage of these points is recommended but not obligatory, more Cover Points you add to your map, more randomness, and realistic bot navigation you will get.\n \n<?title=18>ADD A NEW COVER POINT</title>\n \nAdd a new Cover Points is simply as duplicate one of the existing ones and manually placed in your map.\n \nIn order to easily preview all your Cover Points, you can turn on the gizmos for it in <i><b>(Your Map scene hierarchy) ➔ AIManager ➔ bl_AICoverPointManager ➔ Show Gizmos.</b></i>");
        DrawServerImage("img-5.png");
        DownArrow();
        DrawText("Each Cover Point reference must have attached the script <b>bl_AICoverPoint</b>, otherwise it wont work as a cover point, this scripts have a few public properties in the inspector:");
        DrawServerImage("img-6.png");
        DrawPropertieInfo("Crouch", "bool", "Tell is the bot should crouch (or stand up) while is using this cover point");
        DrawPropertieInfo("Neighbord Points", "List", "List with near by cover points that will be used as fallback in case this cover point is being used.");
    }

    void AICoverPointManagerDoc()
    {
        DrawText("The script <b>bl_AICoverPointManager.cs</b> is attached in each <i><b>map scene ➔ AIManager ➔ bl_AICoverPointManager</b></i>, this script handle the logic behind the cover point selection, when a bot request a cover point, this script is responsible for determine which cover point in the scene should be used based on the requester bot conditions.\n\nThis script has some public properties in the inspector that you can tweak:");
        DrawServerImage("img-7.png");
        DrawPropertieInfo("Max Distance", "float", "The max distance for which a cover point is consider a neighbor from another cover point.");
        DrawPropertieInfo("Usage Time", "float", "The 'cooldown' time that takes for the cover point to be used again after being used.");
        DrawPropertieInfo("Show Gizmos", "bool", "Show gizmos for each cover point in the map.");
        DrawPropertieInfo("Bake Neighbors Points", "Button", "Automatically calculate the neighbord cover points for each cover point in the scene, you should use this everytime you edit the cover points in your scene.");
        DrawPropertieInfo("Align point to floor", "Button", "Automatically vertical re-positione the cover point in the scene so it is right above the floor below the point (and not floating)");
    }

    GameObject ModelPrefab = null;
    void DrawModel()
    {
        if (subStep == 0)
        {
            DrawText("In order to replace the human model in one of the bots prefabs, you need a Humanoid rigged model.\n \n" +
                "your model has to be set up in <b>Humanoid</b> rigged in the model import settings:");
            Space(2);
            DrawImage(GetServerImage(0));
            DownArrow();
            DrawText("Then, drag the player model in the empty field below and click on <b>Create</b> button");
            Space(2);
            GUILayout.BeginVertical("box");
            ModelPrefab = EditorGUILayout.ObjectField("Human Model", ModelPrefab, typeof(GameObject), true) as GameObject;
            if (ModelPrefab != null)
            {
                Space(4);
                if (DrawButton("Create"))
                {
                    ReplaceBotModel();
                    NextStep();
                }
            }
            GUILayout.EndVertical();
        }else if(subStep == 1)
        {
            DrawText("Ok, now if all work correctly you should see a prefab in the scene hierarchy called <b>AISoldier [NEW]</b> with your human model integrated, that's the bot prefab," +
                "your model has been integrated and setup automatically, even though there is a fix that you have to do manually, the weapons model has been move to the new model right hand transform " +
                "but the position of these could be wrong, so you have to positioned it right.");
            DrawImage(GetServerImage(1));
            DrawText("Click the button below to select the weapon parent transform automatically.");
            Space(2);
            if(DrawButton("Select bot weapons parent"))
            {
                var asw = FindObjectOfType<bl_AIShooterAttack>();
                Transform wr = asw.aiWeapons[0].transform.parent;
                Selection.activeTransform = wr;
                EditorGUIUtility.PingObject(wr);
            }
            DownArrow();
            DrawText("Now positioned the weapons (moving the selected transform) to simulate that the human models is holding it:");
            DrawImage(GetServerImage(2));
        }else if(subStep == 2)
        {
            DrawText("Good, all is ready, now you have to create a prefab of this or replace one of the current bots prefabs," +
                "for it drag the <b>AISoldier [NEW]</b> from hierarchy to a <b>Resources</b> folder, for default you can drag it to <i>MFPS -> Resources</i>, in this folder you can create a prefab" +
                " or replace one of the default bots prefabs (AISoldier or AISoldier2), in case you create a new prefab you also have to assign this prefab in GameData -> BotTeam1 or BotTeam2.");
            DrawImage(GetServerImage(3));
            DrawText("That's :)");
        }
    }

    void BotsNameDoc()
    {
        DrawHyperlinkText("The bots are named randomly from a predefined list of names that you as the developer can easily modify.\n \nFirst, you can define the prefix that goes before the random name, this by default is <b>BOT</b>, you can change it to whatever you want in <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ <b>Bots Name Prefix</b>.\n \nTo modify the list of the random names, open the script <b>bl_GameTexts.cs</b> ➔ <b>RandomNames</b>, in this list, add, remove or edit any element of the list.");
        DrawServerImage("img-8.png");
    }

    void ReplaceBotModel()
    {
        if (ModelPrefab == null) return;
        GameObject model = ModelPrefab;
        if(PrefabUtility.IsPartOfAnyPrefab(ModelPrefab))
        {
            model = PrefabUtility.InstantiatePrefab(ModelPrefab) as GameObject;
#if UNITY_2018_3_OR_NEWER
        PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
        }
        model.name += " [NEW]";
        GameObject botPrefab = PrefabUtility.InstantiatePrefab(bl_GameData.Instance.BotTeam1.gameObject) as GameObject;
#if UNITY_2018_3_OR_NEWER
        PrefabUtility.UnpackPrefabInstance(botPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
        botPrefab.name = "AISoldier [NEW]";
        var oldModel = botPrefab.GetComponentInChildren<bl_AIAnimationBase>();
        oldModel.name += " [OLD]";
        Animator modelAnimator = model.GetComponent<Animator>();
        modelAnimator.applyRootMotion = false;
        modelAnimator.runtimeAnimatorController = oldModel.GetComponent<Animator>().runtimeAnimatorController;
        if (!AutoRagdoller.Build(modelAnimator))
        {
            Debug.LogError("Could not build a ragdoll for this model");
            return;
        }

        bl_AIShooterAgent aisa = botPrefab.GetComponent<bl_AIShooterAgent>();
        if(aisa != null)
        botPrefab.GetComponent<bl_AIShooterAgent>().aimTarget = modelAnimator.GetBoneTransform(HumanBodyBones.Spine);
        var botReferences = botPrefab.GetComponent<bl_AIShooterReferences>();

        model.transform.parent = oldModel.transform.parent;
        model.transform.localPosition = oldModel.transform.localPosition;
        model.transform.localRotation = oldModel.transform.localRotation;
        var aia = model.AddComponent<bl_AIAnimation>();
        botReferences.aiAnimation = aia;
        aia.mRigidBody.Clear();
        aia.mRigidBody.AddRange(model.transform.GetComponentsInChildren<Rigidbody>());
        Collider[] allColliders = model.transform.GetComponentsInChildren<Collider>();
        for (int i = 0; i < allColliders.Length; i++)
        {
            allColliders[i].gameObject.layer = LayerMask.NameToLayer("Player");
        }
        botReferences.hitBoxManager.SetupHitboxes(modelAnimator);
        botReferences.PlayerAnimator = modelAnimator;
        EditorUtility.SetDirty(botReferences.hitBoxManager);
        Transform weaponRoot = botPrefab.GetComponent<bl_AIShooterAttack>().aiWeapons[0].transform.parent;
        Vector3 wrp = weaponRoot.localPosition;
        Quaternion wrr = weaponRoot.localRotation;
        weaponRoot.parent = modelAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        weaponRoot.localRotation = wrr;
        weaponRoot.localPosition = wrp;
        DestroyImmediate(oldModel.gameObject);

        var view = (SceneView)SceneView.sceneViews[0];
        view.LookAt(botPrefab.transform.position);
        EditorGUIUtility.PingObject(botPrefab);
        Selection.activeTransform = botPrefab.transform;
    }

    [MenuItem("MFPS/Tutorials/ Change Bots", false, 501)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TutorialBots));
    }
}