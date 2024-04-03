using UnityEngine;
using UnityEditor;
using MFPSEditor;
//using Lovatto.DevTools;
using UnityEditor.Animations;
using System.Linq;
using System.IO;

public class MFPSGeneralDoc : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/general/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-0.jpg", Image = null},
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.png", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.png", Image = null},
        new NetworkImages{Name = "img-9.png", Image = null},
        new NetworkImages{Name = "img-10.png", Image = null},
        new NetworkImages{Name = "img-11.png", Image = null},
        new NetworkImages{Name = "img-12.png", Image = null},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_33.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_6.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_27.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_14.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_31.jpg", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_22.jpg", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_24.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_19.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_29.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://img.youtube.com/vi/ysYqI4w1vq4/0.jpg", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_23.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "img-13.jpg", Image = null},
        new NetworkImages{Name = "img-14.jpg", Image = null},
        new NetworkImages{Name = "img-15.png", Image = null},
        new NetworkImages{Name = "img-16.png", Image = null},
        new NetworkImages{Name = "img-17.png", Image = null},
        new NetworkImages{Name = "img-18.png", Image = null},
        new NetworkImages{Name = "img-19.png", Image = null},
        new NetworkImages{Name = "img-20.png", Image = null},//31
        new NetworkImages{Name = "img-21.png", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
    {
        new GifData{ Path = "createwindowobj.gif" },
        new GifData{ Path = "addnewwindowfield.gif" },
        new GifData{ Path = "createwindowbutton.gif" },
        new GifData{ Path = "addonintegrateprevw.gif"},
        new GifData{ Path = "mfps-urpcsps.gif"},
        new GifData{ Path = "playerai-1.gif"},
        new GifData{ Path = "cpaoc.gif"},
        new GifData{ Path = "adcwtp.gif"},
        new GifData{ Path = "adcwtp2.gif"},
    };
    private Steps[] AllSteps = new Steps[] {
    new Steps { Name = "Resume", StepsLenght = 0, DrawFunctionName = nameof(Resume) },
    new Steps { Name = "Terminology", StepsLenght = 0, DrawFunctionName = nameof(TerminologyDoc) },
    new Steps { Name = "How to Modify MFPS", StepsLenght = 3, DrawFunctionName = nameof(HowToEditMFPSDoc),
     SubStepsNames = new string[]{ "How to Modify", "Code Modifications", "Other Modifications" } },
    new Steps { Name = "GameData", StepsLenght = 0, DrawFunctionName = nameof(GameDataDoc) },
    new Steps { Name = "Photon PUN", StepsLenght = 3, DrawFunctionName = nameof(DrawPhotonPunDoc),
    SubStepsNames = new string[]{ "Photon PUN", "Photon Server", "What is CCU?"}},
    new Steps { Name = "Offline", StepsLenght = 0, DrawFunctionName = nameof(OfflineDoc) },
    new Steps { Name = "URP", StepsLenght = 4, DrawFunctionName = nameof(UniversalRPDoc) },
    new Steps { Name = "HDRP", StepsLenght = 5, DrawFunctionName = nameof(HDRPDoc) },
    new Steps { Name = "Kill Feed", StepsLenght = 2, DrawFunctionName = nameof(KillFeedDoc) },
    new Steps { Name = "Player Prefabs", StepsLenght = 0, DrawFunctionName = nameof(PlayerPrefabsDoc) },
    new Steps { Name = "Player Classes", StepsLenght = 0, DrawFunctionName = nameof(PlayerClassesDoc) },
    new Steps { Name = "Head Bob", StepsLenght = 0, DrawFunctionName = nameof(HeadBobDoc) },        
    new Steps { Name = "Game Audio", StepsLenght = 3, DrawFunctionName = nameof(AudioDoc),
    SubStepsNames = new string[]{ "Audio Reskin", "Audio Ranges", "Audio Assets"}},
    new Steps { Name = "Game Texts", StepsLenght = 0, DrawFunctionName = nameof(DrawGameTexts) },
    new Steps { Name = "Game Input", StepsLenght = 5, DrawFunctionName = nameof(GameInputDoc),
    SubStepsNames = new string[]{ "Game Input", "Default Mapped", "Add Input", "GampePad", "Add Mapped" }},
    new Steps { Name = "Game UI", StepsLenght = 2, DrawFunctionName = nameof(GameUIDoc),
    SubStepsNames = new string[]{ "UI Reskin", "UI Assets" }},
    new Steps { Name = "Teams", StepsLenght = 0, DrawFunctionName = nameof(DrawTeamsDoc) },
    new Steps { Name = "Coins", StepsLenght = 2 , DrawFunctionName = nameof(DrawCoins),
    SubStepsNames = new string[]{ "Properties", "Operations" } },
    new Steps { Name = "Game Modes", StepsLenght = 3, DrawFunctionName = nameof(GameModesDoc),
    SubStepsNames = new string[]{ "Game Modes", "Custom Mode", "Per Map Modes" } },
    new Steps { Name = "Player Animations", StepsLenght = 4, DrawFunctionName = nameof(DrawPlayerAnimationDoc),
    SubStepsNames = new string[]{ "Basic", "Advance", "Weapon Animations", "Animation Assets" } },
    new Steps { Name = "Name Plates", StepsLenght = 0, DrawFunctionName = nameof(NamePlatesDoc) },
    new Steps { Name = "Bullets", StepsLenght = 3 , DrawFunctionName = nameof(DrawBullets),
    SubStepsNames = new string[]{ "Bullet Prefab", "Bullet Decals", "Custom Bullet" } },
    new Steps { Name = "Kits", StepsLenght = 0, DrawFunctionName = nameof(DrawKitsSystem) },
    new Steps { Name = "Kill Zones", StepsLenght = 0, DrawFunctionName = nameof(DrawKillZones) },
    new Steps { Name = "Room Properties", StepsLenght = 0, DrawFunctionName = nameof(RoomPropertiesDoc) },
    new Steps { Name = "Game Settings", StepsLenght = 0, DrawFunctionName = nameof(DrawGameSettings) },
    new Steps { Name = "Mouse Look", StepsLenght = 0, DrawFunctionName = nameof(MouseLookDoc) },
    new Steps { Name = "Object Pooling", StepsLenght = 0, DrawFunctionName = nameof(DrawObjectPooling) },
    new Steps { Name = "Add New Menu", StepsLenght = 0, DrawFunctionName = nameof(AddNewMenu) },
    new Steps { Name = "Player Arms IK", StepsLenght = 0, DrawFunctionName = nameof(PlayerIKDoc) },
    new Steps { Name = "Crosshair", StepsLenght = 0, DrawFunctionName = nameof(CrosshairDoc) },
    new Steps { Name = "Mobile", StepsLenght = 0, DrawFunctionName = nameof(DrawMobileDoc) },
    new Steps { Name = "Particles & Decals", StepsLenght = 2, DrawFunctionName = nameof(ParticlesDecalsDoc) ,
    SubStepsNames = new string[]{ "Particles & Decals", "FX Assets" }},
    new Steps { Name = "Post Processing", StepsLenght = 3, DrawFunctionName = nameof(PostProcessingDoc),
        SubStepsNames = new string[]{ "Post Processing", "Custom Profile", "Error Handling" } },
    new Steps { Name = "Friend List", StepsLenght = 0, DrawFunctionName = nameof(DrawFriendListDoc) },
    new Steps { Name = "InGame Chat", StepsLenght = 0, DrawFunctionName = nameof(InGameChatDoc) },
    new Steps { Name = "FootStep", StepsLenght = 0, DrawFunctionName = nameof(FootStepsDoc) },
    new Steps { Name = "Doors", StepsLenght = 0, DrawFunctionName = nameof(DoorsDoc) },
    new Steps { Name = "Server Regions", StepsLenght = 0, DrawFunctionName = nameof(ServerRegionDoc) },
    new Steps { Name = "Local Notifications", StepsLenght = 0, DrawFunctionName = nameof(LocalNotificationsDoc) },
    new Steps { Name = "Kick Vote", StepsLenght = 0, DrawFunctionName = nameof(KickVotationDoc) },
    new Steps { Name = "Game Staff", StepsLenght = 0, DrawFunctionName = nameof(GameStaffDoc) },
    new Steps { Name = "Network Stats", StepsLenght = 0, DrawFunctionName = nameof(NetworkStats) },
    new Steps { Name = "Player Hitbox", StepsLenght = 2, DrawFunctionName = nameof(PlayerHitboxDoc),
    SubStepsNames = new string[]{ "Colliders", "Damage"}},
    new Steps { Name = "Deal Damage", StepsLenght = 2, DrawFunctionName = nameof(PlayerDamageDoc),
     SubStepsNames = new string[]{ "Player Damage", "Object Damage"}},
    new Steps { Name = "Ladder", StepsLenght = 0, DrawFunctionName = nameof(LadderDoc) },
    new Steps { Name = "Events", StepsLenght = 0, DrawFunctionName = nameof(MFPSEventsDoc) },
    new Steps { Name = "Addons", StepsLenght = 0, DrawFunctionName = nameof(DrawAddonsDoc) },
    new Steps { Name = "Editor Menus", StepsLenght = 0, DrawFunctionName = nameof(EditorMenusDoc) },
    new Steps { Name = "Update MFPS", StepsLenght = 0, DrawFunctionName = nameof(UpdateMFPSDoc) },
    new Steps { Name = "Anti-Cheat", StepsLenght = 0, DrawFunctionName = nameof(AntiCheatDoc) },
    new Steps { Name = "FP Arms Material", StepsLenght = 0, DrawFunctionName = nameof(FPArmsMaterial) },
    new Steps { Name = "AFK", StepsLenght = 0, DrawFunctionName = nameof(AfkDoc) },
    new Steps { Name = "Lobby Chat", StepsLenght = 0, DrawFunctionName = nameof(DrawLobbyChat) },
    new Steps { Name = "Common Q/A", StepsLenght = 0, DrawFunctionName = nameof(CommonQADoc) },
    new Steps { Name = "Known Issues", StepsLenght = 0, DrawFunctionName = nameof(KnownIssuesDoc) },
    };

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }
    //final required////////////////////////////////////////////////

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        FetchWebTutorials("mfps2/tutorials/");
        allowTextSuggestions = true;
    }

    void Resume()
    {
        DrawTitleText("MFPS 2.0");
        DrawText("Version: " + MFPSEditor.AssetData.Version);
        DrawYoutubeCover("MFPS Get Started Video", GetServerImage(22), "https://www.youtube.com/watch?v=ysYqI4w1vq4");
        DrawTitleText("Hot Tutorials");

        if(DrawLinkText("Add Maps"))
        {
            EditorApplication.ExecuteMenuItem("MFPS/Tutorials/Add Map");
        }
        if (DrawLinkText("Add Weapon"))
        {
            EditorApplication.ExecuteMenuItem("MFPS/Tutorials/Add Weapon");
        }
        if (DrawLinkText("Add Players"))
        {
            EditorApplication.ExecuteMenuItem("MFPS/Tutorials/Add Player");
        }
        if (DrawLinkText("Change Bots"))
        {
            EditorApplication.ExecuteMenuItem("MFPS/Tutorials/Change Bots");
        }

        DrawTitleText("Custom Integration Tutorials");
        if(DrawLinkText("Integrate Loading Screen to MFPS"))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/integrate-loading-screen-to-mfps-2-0/");
        }
        if (DrawLinkText("Integrate DestroyIt to MFPS"))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/integrate-destroyit-to-mfps-2-0/");
        }
    }

    void TerminologyDoc()
    {
        DrawText("There are some words and phrases that you will encounter a lot in this documentation that you may not be familiar with, here you find a short explanation of what they refer to.");
        DrawHorizontalSeparator();
        Space(10);
        DrawHorizontalColumn("GameData", "Is a <i>ScriptableObject</i> that contain many front-end settings of MFPS that you can easily tweak to fit your needs and reskin the game, the options go from simple toggles to show or hide blood in the game up to the Weapon and Game Mode information, it can be found in the <b>Resources</b> folder of MFPS.\n\nFor more information check the <b>GameData</b> section.");

        DrawHorizontalColumn("Player Prefab", "Are the unity prefabs or prefab that contain all the required scripts, objects, and structure that make up the MFPS player controller,\nby default the player prefabs are located in the <b>Resources</b> folder of MFPS.\n\nFor more information, check the <b>Player Prefab</b> section.");

        DrawHorizontalColumn("FPWeapon", "Stands for '<b>First Person Weapon</b>' is referring to the weapon that it seems for the local player camera, also referenced as the <b>view model</b>, What differentiates this weapon from the <i>TPWeapon</i> is that it contains the weapon and the arms/hands model and it only shows for the local player camera.\n \nThe FPWeapons can be found inside the <b>Local</b> child of each player prefab.");

        DrawHorizontalColumn("TPWeapon", "Stands for '<b>Third Person Weapon</b>' is referring to the weapon that it seems in the other player's soldier model, also referenced as the <b>world model</b>, What differentiates this weapon from the <i>FPWeapon</i> is that it is a single weapon model and is placed inside the player/soldier model hands.\n \nThe TPWeapons can be found inside the <b>Remote</b> child of each player prefab, specifically in the right-hand bone of the player model.");

        DrawHorizontalColumn("Unity Top Menu", "Is referencing the Menu Items located at the top of the Unity Editor, <i><b>File, Edit, Assets, GameObject, Components, MFPS, etc...</b></i>");
    }

    void HowToEditMFPSDoc()
    {
        if (subStep == 0)
        {
            DrawText("This is a question that could sound like you will have some sort of limitations for modifying MFPS, but that is definitely not the case, if you know what you want to do and you know how to do it, you practically have no limitations, MFPS includes the full source code and content of the game so everything can be modified as the developer please.\n \nThe purpose of this guide is to give a recommendation of the ideal way of modifying MFPS <b>if you want to make it a lot easier to merge future MFPS updates.</b>\n \nIf you have been using MFPS for quite a while, you already know that one of the biggest problems with MFPS has been the merge of Updates, since each major update require to be imported in a new Unity Project, this because import a new update in a project with an existing old version of MFPS, will cause to lose all the changes that you have done until that point in the game and most likely will break the game too, so the only possible way to apply the improves and fixes of new updates in old version projects have been to do a manual merge, checking script by script, comparing the prefabs, etc... definitely a nightmare.\n \nThis in part was caused by the way that MFPS was created, the only way to do modifications to the code was directly in the game default scripts, due to the hard-coded code and the \"spiderweb\" of references,\nalso at difference of tools or extensions assets, game templates are intended for the users/devs to modify the core content, which also contributes to making it harder to update to new versions.\nSince version 1.9 a redo of the game backend design has started, making most of the code inheritable allowing developer to make code changes without the need to modify the default scripts, instead, developers can create a new script ➔ inherited from the Base-Classes ➔ and implement their custom changes.\n \nOn the next page, you will have more information about this and how to work with that kind of code design.");
        }
        else if (subStep == 1)
        {
            DrawSuperText("<?background=#FFF>Code Modifications</background>\n\nStarting from MFPS 1.9, most of the default scripts are using code-inheritance design, <b>Inheritance allows you or your programmers to create classes that are built upon existing classes of the game, to specify a new implementation while maintaining the same behaviors, to reuse code, and to independently extend original code.</b>\n \n<b>As an example of how this design works and how it helps:</b>\n \nBefore MFPS 1.9, if you had wanted to make a modification to the weapon pick-up system, for example, you would have to make the changes to the script <b>bl_GunPickUp.cs</b> <i>(the default script)</i> which would cause you no longer be able to automatically update that script since you would lose your changes.\nInstead in MFPS 1.9 if you want to make the same modification, you won't need to make it directly in the default script, instead, you will create a new script and inherit it from the base class (in this case <b>bl_GunPickUpBase</b>) ➔ You will copy the code from the default script and pasted in your new one, and then make the wanted changes, you won't have to worry about the references to that script since all references are connected to the base class, so you only will need attach your new script in wherever game object the default script was attached (and you will remove/detach the default script).\n \n<?background=#FFF>How to inherit a script?</background>\n \nIf you are a medium-high experienced programmer, you may already be familiar with this programming design since is not only what is expected from a good code but also this type of design makes it a lot easier to maintain and scale the code and adds a polymorphism structure so you can implement new features/variants with less effort.\n \nIf you are not a programmer or you aren't familiarized with code inheritance, all you have to do is create a new script, using the same example of the gun pick up, let's say this script is for making modifications to the weapon pick up logic, so after you create that script you will have a simple structure like this:");
            DrawCodeText("using UnityEngine;\n \npublic class bl_GunPickUp2 : MonoBehaviour\n{\n    ...\n}");
            DrawText("What you have to do is change the base class <i>(the class name next to your class name)</i>, in this case, it's <color=#0E6148FF>MonoBeheaviour</color>, you have to change to the base class of <i>bl_GunPickUp</i> which is <color=#0E6148FF>bl_GunPickUpBase</color>, so you will have something like this:");
            DrawCodeText("using UnityEngine;\n \npublic class bl_GunPickUp2 : bl_GunPickUpBase\n{\n    ...\n}");
            DrawNote("Almost all the default scripts have this same name pattern of the base classes, they just end with the \"Base\" word at the end of the name, but you can always make sure which is the base of a script by opening the default script.");
            DrawText("Once you change the base class, you will have to override the base class functions/methods, if you don't know how to do this, simply copy the code of the original class <i>(in this case <b>bl_GunPickUp.cs</b>)</i> and paste it your script,\nThat is all you need, know you can make the changes that you want in your new script.");
            DrawHorizontalSeparator();
            DrawText("As said before, this is not a requirement to make code changes but is the recommended way, although not all code MFPS allow inheritance, as of version 1.9 more than half of the code has been redesigned to work that way but the works are still in progress, the target is that the whole code or at least most of it allows inheritance and be modular in future updates.");
        }
        else if (subStep == 2)
        {
            DrawText("The same idea of the <i>code modifications</i> applied to other game modifications like prefabs, menus, scenes, etc... <b>ideally you should not modify the defaults content, instead, you should create a duplicate</b> and use that duplicated, e.g:\nFor the MainMenu scene, you should create a duplicated of the scene and leave the default scene as-is for future reference.\nThat way if you want to merge a future update of MFPS you won't lose your changes on that scene if you import it, and you will have it as a reference to check the changes of the new version made in the scene.\n \nSame idea with the Player Prefabs, instead of using the default player prefabs <i>(MPlayer and MPlayer2)</i> you should create a duplicate of each and use these duplicates <i>(assigning them on GameData)</i>.\n \nYou get the idea, don't use the default content ➔ create and use a duplicate instead.");
            DrawNote("In the editor, you can duplicate almost anything by selecting it in the Project View window and pressing <b>Ctrl + D</b> on Windows or <b>Command + D</b> on Mac.");
        }
    }

    void GameDataDoc()
    {
        DrawTitleText("GameData");
        DrawText("With MFPS you will notice that <b>GameData</b> is mentioned a lot in the documentation, readMe.txt, and many other comments.\nIf you don't know what <b>GameData</b> is, how it works or where it's located, here is a brief explanation:\n\n<b>GameData</b> is a <i>ScriptableObject</i> that contain a lot of front-end settings that you can easily tweak to fit your needs and reskin the game, the options go from simple toggles to show or hide blood in the game up to the Weapon and Game Mode information.");
        DrawHyperlinkText("<link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> is located in the <b>Resources</b> folder of MFPS:");
        DrawServerImage(3);
        DrawNote("You can also open this quickly using the <b>MFPS Manager</b> window by clicking <b>Ctrl + M</b> in the editor using Windows or <b>Command + M</b> on Mac.");
    }

    void DrawPhotonPunDoc()
    {
        if (subStep == 0)
        {
            DrawText("<b>Photon Unity Networking</b> a.k.a <b>Photon PUN</b> is the network solution that MFPS use to handle all the network/server side stuff, it's one if it's not the most solid solution out there for Unity, it's fast, reliable, scalable as you can expect from a generic network solution, PUN offer multiple server locations worldwide which you can connect to in order to get least ping.\n\nIn Unity PUN comes as a third party plugin which you can download for free from the Unity Asset Store <i><b><size=8>(you probably already did by following the Get Started tutorial)</size></b></i>,\nNow there some common question about this network solution, first of course when we talk about server side things that of course have a cost, since PUN handle all the server stuff including the code, Hosting, operations and scaling services, server maintainement, etc... you don't have to worry about that things since Photon Team takes care of that, but due that there is a cost for this service, <b>PUN is a Pay service</b> but offer a Free Plan that you can use for the development process and upgrade when you are about to release your game.\n");

            DrawHyperlinkText("You can see all the available Plans in their website:\n<link=https://www.photonengine.com/en-US/PUN/pricing>Photon Pun Plans</link>\n");
            DrawHorizontalSeparator();
            DrawText("A common question that I receive is:\n\n<i><b><size=16>What about Authoritative Server?</size></b></i>\n\nIf you have previous experience with networks systems, you may already noticed that Photon use a Client <i>(named as Master Client)</i> instead of a server <i>(Master Server)</i> to authority the game, this open a gap for cheaters to easily mod the gameplay in their end and duplicated for others clients, since there's not an independent Master Server to compare and validate the logic with.\n");
            DrawHyperlinkText("Out of the box, Photon PUN doesn't offer a solid solution for fix this problem, instead they offer <link=https://www.photonengine.com/en-us/Server>Photon OnPremise</link> aka Photon Server with which you can host the server side code/sdk and make changes in the serve code and create your own authoritative server by modifying the server-side code but that also require some knowledge in the are, if you are interested in using Photon Server check the next section for more information regarding.");
        }
        else if (subStep == 1)
        {
            DrawText("As explained in the previous section, <b>Photon Server is an alternative to Photon PUN</b> which have some advantages but also some other things to take into consideration, some of the benefits of use Photon Server over Photon PUN are:\n \n- Self-Hosted servers and more control over the server-side code.\n- More affordable CCU plan prices with an unlimited plan option.\n- Allow hosting a server in a specific region not available in PUN.\n- Cheaper overall than using a Photon PUN plans.");
            DrawText("But these are things you have to take into consideration when using Photon Server:\n \n- You are in charge of keeping your server running and handling any server crash, shutdown, clean-up, etc...\n\n- Regions availability depends on where you host the server, at difference of Photon PUN which includes the option to easily change over 13 different regions around the globe, with Photon Server it will depend on where you host your server and that will be the unique region available unless you also add a server selection system in your game.\n\n- Scalability will depend on your type of hosting and plan, at the difference of Photon PUN where you can easily upgrade your plan if you require more CCUs, with Photon Server even if you have the unlimited CCU plan, if your hosting server is not prepared for auto-scalability you will have troubles growing your game, because of that is recommended that you use a resizable server hosting plan like EC2 of AWS.");
            DrawSuperText("For more detail information about Photon Server, check the website page here:\n<?link=https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-intro>https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-intro</link>");
            Space(18);
            DrawSuperText("<b><size=16>Use Photon Server with MFPS</size></b>\n \nUsing Photon Server with MFPS instead of Photon PUN doesn't require code changes in MFPS but you have to set up the server SDK manually and that may require some experience or knowledge in the area, fortunately, the official documentation explains the process clearly and step by step to set up and deploy your own server, you can found the guide here:\n<?link=https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-in-5min>https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-in-5min</link>");
            DrawSuperText("Once you have set up the Photon Server SDK and have it running, there're just a few things to do in your project to start using it:\n \n- In your MFPS Unity project, go to the <?link=asset:Assets/MFPS/Content/Required/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset>PhotonServerSettings (Click Here)</link> or located by default in <i>Assets ➔ MFPS ➔ Content ➔ Required ➔ Photon ➔ PhotonUnityNetworking ➔ Resources ➔ PhotonServerSettings</i>.\n\n- Foldout the <b>Server/Cloud Settings</b> > uncheck the <b>Use Name Server</b> toggle.\n\n- In the <b>Server</b> text field paste your server <i>(where you setup the Photon Server SDK)</i> public IP or domain name if apply.\n \n- In the <b>Port</b> input field set the port number that you open for your server or if you didn't change it set the default one which is <b>5055 for UDP</b> or <b>4530 for TCP</b>.\n \nThat's all, if everything was set up correctly you should not be able to play the game and it will be connecting to your server.");
            DrawServerImage("img-49.png", TextAlignment.Center);
        }
        else if (subStep == 2)
        {
            DrawText("<b><size=16>What is CCU?</size></b>\n \n<b>CCU</b> stands for <b>Concurrently Connected Users</b> and with Photon and other networking plugins it refers to the limit number of concurrent players that the server or plan allows or supports.\n \nWith Photon PUN each plan has a CCU limit and when that limit is reached it will refuse more connections causing new players not able to connect until a slot gets free, the default plan which is for development purposes counts with a 20 CCU limit which means only 20 players can be connected at the same time.");
            DrawNote("Do not confuse <b>CCU</b> with <b>DAU</b> <i>(Daily Active Users)</i> or <b>MAU</b> <i>(Monthly Active Users)</i>, CCU is only the players connected at the same time.");
            DrawText("<b><size=16>How to know which CCU limit I need?</size></b>\n \nSince the higher the CCU limit a Photon plan has higher the monthly cost is, it's important to select the right plan, for it you can use the formula based on the average of CCU with respect to the DAU and MAU so based on how many daily or monthly players play your game, you can get an idea of which CCU plan will be enough for you.");
            DrawNote("This data is based on official Photon stats collected across many titles and games working with Photon.");
            DrawText("CCU to DAU has a factor of 10 to 100 (depending on the game).\nDAU to MAU has a factor of 10.\n \nSo <b>100 CCU might work for as many as 100k monthly active users</b>.\nThese numbers are distorted for new games that get a lot of attention and playtime initially, so it's never really a fixed factor");
        }
    }

    void OfflineDoc()
    {
        DrawText("MFPS supports the Photon <b>Offline mode</b>, this allows you to test the map scene without the need to go to the lobby -> create a room -> load the map scene, instead you play the scene directly.\n\nThis feature is especially useful when, for example, you make changes to the player's prefab or a weapon and want to test them at runtime, save lot of time and improve the development work-flow.\n\nTo enable or disable this feature go to <b>GameData</b> -> Offline Mode\nAfter you enable it, simply open the map scene and Play.");
        DrawNote("The offline mode <b>is not</b> designed or intended to develop an offline game using MFPS, but rather to facilitate the development of the multiplayer gameplay.");
    }

    void UniversalRPDoc()
    {
        if (subStep == 0)
        {
            DrawText("By default MFPS use the legacy build-in render pipeline, MFPS will be using URP <i>(Universal Render Pipeline)</i> as the default Render Pipeline in the future when it's more standardized, but for the moment in order to use MFPS with URP or HDRP you have to manually convert the project, in this doc I'll teach you how you can do it:\n\n<b><size=20>Convert MFPS project to URP:</size></b>\n\n*<i>This tutorial takes for granted that you have an MFPS project working with the build-in render pipeline in Unity 2018.4 or later</i>*\n\nFirst of all, you have to remove the Post-Processing package, for this simply go to (Unity Top Menu) <b>MFPS -> Tools -> Delete Post-Processing</b> -> Wait until script compilation finish.\n\n<i>Continue in the next step.</i>");
        }
        else if (subStep == 1)
        {
            DrawText("<b><size=22>Installing URP</size></b>\n\n1. In Unity, open your Project.\n2. In the top navigation bar, select Window > Package Manager to open the Package Manager window.\n3. Select the All tab. This tab displays the list of available packages for the version of Unity that you are currently running.\n4. Select Universal RP from the list of packages.\n5. In the bottom right corner of the Package Manager window, select Install. Unity installs URP directly into your Project.\n\n<b><size=22>Configuring URP</size></b>\n\nBefore you can start using URP, you need to configure it. To do this, you need to create a Scriptable Render Pipeline Asset and adjust your Graphics settings.\n\nCreating the Universal Render Pipeline Asset\nThe Universal Render Pipeline Asset controls the global rendering and quality settings of your Project, and creates the rendering pipeline instance. The rendering pipeline instance contains intermediate resources and the render pipeline implementation.\n\n<b><size=16>To create a Universal Render Pipeline Asset:</size></b>\n\n1. In the Editor, go to the Project window.\n2. Right-click in the Project window, and select Create > Rendering: Universal Render    Pipeline: Pipeline Asset. Alternatively, navigate to the menu bar at the top, and select Assets: Create: Rendering: Universal Render Pipeline: Pipeline Asset.\n\nYou can either leave the default name for the new Universal Render Pipeline Asset, or type a new one.");
            DrawText("<b><size=16>Adding the Asset to your Graphics settings</size></b>\n\nTo use URP, you need to add the newly created Universal Render Pipeline Asset to your Graphics settings in Unity. If you don't, Unity still tries to use the Built-in render pipeline.\n\nTo add the Universal Render Pipeline Asset to your Graphics settings:\n\nNavigate to<b> Edit > Project Settings... > Graphics.</b>\nIn the <b>Scriptable Render Pipeline Settings</b> field, add the Universal Render Pipeline Asset you created earlier. When you add the Universal Render Pipeline Asset, the available Graphics settings immediately change. Your Project is now using URP.\n\n<b>Now you will see some (a lot) pink objects</b>, this is because the shaders from the build-in RP doesn't work on URP or HDRP, you have to upgrade the material shaders, in the next step, I'll show you how to convert them.");
        }
        else if (subStep == 2)
        {
            DrawText("<b><size=22>Upgrading your Shaders</size></b>\n\nIf your Project uses shaders from the built-in render pipeline, and you want to switch your Project to use the Universal Render Pipeline instead, you must convert those Shaders to the URP Shaders. This is because built-in Lit shaders are not compatible with URP Shaders. For an overview of the mapping between built-in shaders and URP Shaders, see Shader mappings.\n\nTo upgrade built-in Shaders:\n\n1. Open your Project in Unity, and go to Edit > Render Pipeline > Universal Render Pipeline.\n2. Select <b>Upgrade Project Materials to URP Materials</b>\n\n\n<b>Note:</b> These changes cannot be undone. Backup your Project before you upgrade it.\n\n<b>Tip:</b> If the Preview thumbnails in Project View are incorrect after you've upgraded, try right-clicking anywhere in the Project View window and selecting Reimport All.");
            Space(10);
            DrawText("After this, you still may see some pink objects, those objects were using a custom shader, so in order to fix them simply select them and change their material shader to a Universal RP Shader.\n");
            DrawServerImage(31);
            DrawText("There's one last thing you have to do, see the next step.");
        }
        else if (subStep == 3)
        {
            DrawText("Finally, there's one last thing that you have to set up.\nin URP and HDRP Camera's work different than the build-in RP, in URP/HDRP there's a 'Base' camera and if you want to render any other camera at the same time, you have to set up it as an '<b>Overlay Camera</b>' and add it on the <b>'Stack'</b> camera list of the '<b>Base Camera</b>'\n\nMFPS players use 2 cameras, one that draws only the FP Weapons and the other that draws everything else, so you have to configure the one that draws the FP Weapons as an 'Overlay Camera', <b>you have to do the following for each player prefab that you are using:</b>\n\n1. In the <b>Project Window</b>, select the player prefab <i>(the default MFPS player prefabs are located in the Resources folder of MFPS)</i> ➔ Click on <b>Open Prefab</b> button.");

            DrawNote("<color=#FFFC01FF>NOTE:</color> You may see a warning message when you select the player prefab, this is because there's a null component attached in the 'Weapon Camera' because the Post-Processing package was removed, to fix this simply remove the null component from the Weapon Camera.");
            DrawText("With the player prefab open do the following:");
            DrawAnimatedImage(4);
            DrawText("And that's, you can start using MFPS with URP, just remember that this last step has to be done in all the player prefabs that you are using.");
        }
    }

    void HDRPDoc()
    {
        if (subStep == 0)
        {
            DrawText("HDRP is one of the new Unity's render pipelines aiming for high-end platforms which allow access to cutting-edge real-time 3D rendering technology designed to deliver high-fidelity graphics and uncompromising GPU performance. By default, MFPS uses the built-in Render Pipeline, but you can manually convert the project to HDRP, in this guide you will learn how to do it.");
            DrawSuperText("<b><size=16>Convert MFPS to HDRP</size></b>\n \nThis tutorial takes for granted that you have an MFPS project using the built-in render pipeline in Unity 2020.1 or later, some options features mentioned in this tutorial may not be located or called the same in newest versions of the editor, for these cases you can refer to the official Unity guide for your specific Unity version here:\n<?link=https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Upgrading-To-HDRP.html>https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Upgrading-To-HDRP.html</link>");
            DrawText("Before start setting up the render pipeline, is necessary to remove the <b>Post Processing</b> package since this is not supported in HDRP (Which includes their own PP system), to automatically remove the package, go to <i>(Unity Top Menu)</i> MFPS ➔ Tools ➔ <b>Delete Post-Processing</b> ➔ Wait until script compilation finish and then continue with the next step.");
        }
        else if (subStep == 1)
        {
            DrawSuperText("<b><size=16>Setting up HDRP</size></b>\n\nFirstly, to install HDRP, add the High Definition RP package to your Unity Project:\n \n<?list=•>Open your Unity project.\nTo open the Package Manager window, go to Window > Package Manager.\nIn the Package Manager window, in the Packages: field, select <b>Unity Registry</b> from the menu.\nSelect <b>High Definition RP</b> from the list of packages.\nIn the bottom right corner of the Package Manager window, select <b>Install.</b></list>");
            DrawNote("<i><size=8><color=#76767694>Note:</color></size></i> When you install HDRP, Unity automatically attaches two HDRP-specific components to GameObjects in your Scene. It attaches the <b>HD Additional Light Data</b> component to Lights, and the <b>HD Additional Camera Data</b> component to Cameras. If you don't set your Project to use HDRP, and any HDRP component is present in your Scene, Unity throws errors. To fix these errors, see the following instructions on how to set up HDRP in your Project.");
            DrawSuperText("To set up HDRP in your project, use the <?link=https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Render-Pipeline-Wizard.html>HDRP Wizard.</link>\n \nTo open the <b>HD Render Pipeline Wizard</b> window, go to <b>Window > Rendering > HD Render Pipeline Wizard</b>.\nIn the <b>Configuration Checking</b> section, go to the <b>HDRP</b> tab and click <b>Fix All</b>. This fixes every HDRP configuration issue with your Project.\nYou have fixed your Project's HDRP configuration issues, but your Scene doesn't render correctly because GameObjects in the Scene still use Shaders made for the Built-in Render Pipeline. To find out how to upgrade Built-in Shaders to HDRP Shaders, see Upgrading Materials in the next step.");
        }else if(subStep == 2)
        {
            DrawText("<b><size=16>Upgrading Materials</size></b>\n\nTo upgrade the Materials in your Scene to HDRP-compatible Materials:\n \n1. Go to <b>Edit > Rendering > Materials</b>\n2. Choose one of the following options:\n \n ■ <b>Convert All Built-in Materials to HDRP</b>: Converts every compatible Material in your Project to an HDRP Material.\n\n ■ <b>Convert Selected Built-in Materials to HDRP</b>: Converts every compatible Material currently selected in the Project window to an HDRP Material.\n\n ■ <b>Convert Scene Terrains to HDRP Terrains</b>: Replaces the built-in default standard terrain Material in every Terrain in the scene with HDRP default Terrain Material.");
            DrawText("<b><size=16>Limitations</size></b>\n\nThe automatic upgrade options described above can't upgrade all Materials to HDRP correctly:\n \nYou can't automatically upgrade custom Materials or Shaders to HDRP. You must convert custom Materials and Shaders manually, in MFPS the water shader is a custom shader that can't be automatically upgrade so you will have to either replace it with a HDRP water sahder or remove the water from your scene.\n\nHeight mapped Materials might look incorrect. This is because HDRP supports more height map displacement techniques and decompression options than the Built-in Render Pipeline. To upgrade a Material that uses a heightmap, modify the Material's Amplitude and Base properties until the result more closely matches the Built-in Render Pipeline version.\n\n<b>You can't upgrade particle shaders</b>. HDRP doesn't support particle shaders, but it does provide Shader Graphs that are compatible with the Built-in Particle System. These Shader Graphs work in a similar way to the built-in particle shaders. To use these Shader Graphs, import the Particle System Shader Samples sample:\n \n 1. Open the <b>Package Manager</b> window (menu: <b>Window > Package Manager</b>).\n 2. Find and click the <b>High Definition RP</b> entry.\n 3. In the package information for <b>High Definition RP</b>, go to the <b>Samples</b> section and click the <b>Import into Project</b> button next to <b>Particle System Shader Samples</b>.");
        }else if(subStep == 3)
        {
            DrawSuperText("<b><size=18>Adjusting lighting</size></b>\n\nHDRP uses <?link=https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Physical-Light-Units.html>physical Light un</link>its to control the intensity of Lights. These units don't match the arbitrary units that the Built-in render pipeline uses.\n \nFor light intensity units, Directional Lights use Lux and all other Light types can use Lumen, Candela, EV, or simulate Lux at a certain distance.\n \nTo set up lighting in your HDRP Project:\n \n1. To add the default sky Volume to your Scene and set up ambient lighting go to <b>GameObject > Volume > Sky and Fog Global Volume.</b>\n\n2. Set the <?link=https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Environment-Lighting.html>Environment Lighting</link> to use this new sky:\n \n - Open the Lighting window (menu: <b>Window > Rendering > Lighting Settings</b>).\n - In the <b>Environment</b> tab, set the <b>Profile</b> property to the same Volume Profile that the Sky and Fog Global Volume uses.\n - Set the <b>Static Lighting Sky</b> property to <b>PhysicallyBasedSky</b>.\n - Optionally, if you don't want Unity to re-bake the Scene's lighting when you make the rest of the changes in this section, you can disable the <b>Auto Generate</b> checkbox at the bottom of the window.");
            DrawText("3. Currently, the shadows are low quality. To increase the shadow quality:\n \nCreate a new <b>Global Volume</b> GameObject (menu: <b>GameObject > Volume > Global Volume</b>) and name it <b>Global Settings.</b>\nTo create a new Volume Profile for this Global Volume:\n \n - Open the Global Volume's Inspector window and go to the Volume component.\n - Go to <b>Profile</b> and select <b>New</b>.\n - To add a Shadows override:\n \n  - Go to Add <b>Override > Shadowing > Shadows</b>.\n  - Enable Max Distance.\n  - Set Max Distance to 50.\n\n4.Configure your Sun Light GameObject.\n \n - Select your Light GameObject that represents the Sun in your Scene to view it in the Inspector.\n - Go to <b>Emmision</b> and set the Intensity to 100000.\n - Set <b>Light Appearance</b> to <b>Color</b>.\n - Set <b>Color</b> to white.\n - To see the sun in the sky, go to <b>Shape</b> and set <b>Angular Diameter</b> to 3.\n\n5. The Scene is now over-exposed. To fix this:\n \n - Select the <b>Global Settings</b> GameObject you created in step 3.\n - Add an <b>Exposure</b> override to its Volume component (menu: Add <b>Override > Exposure</b>).\n - Enable <b>Mode</b> and set it to <b>Automatic</b>.\n - To refresh the exposure, go to the Scene view and enable <b>Always Refresh</b>.");
            DrawServerImage("img-50.png");
        }else if(subStep == 4)
        {
            DrawText("Finally, there's a manual setup you have to do for all the MFPS player prefabs to stack the player cameras:\n \n1. Open the player prefab by opening it in the prefab editor or dragging it into a scene hierarchy.\n \n2. Inside the Player Prefab hierarchy, select the <b>WeaponCamera</b> located in <b>Local > Mouse > Animations > Main Camera > WeaponCamera</b>.\n \n3. In the inspector window of the <b>Camera</b> component > unfold the <b>Output</b> tab and set the <b>Depth</b> value to 2.\n \nApply/Save the player prefab changes and repeat this process with all the remaining player prefabs in your game; once you finish with this, you have complete the basic project conversion to HDRP.");
            DrawServerImage("img-51.png");
        }
    }

    void KillFeedDoc()
    {
        if (subStep == 0)
        {
            DrawTitleText("KILL FEED");
            DrawText("<i>Kill Feed</i> or <i>Who Kill Who</i> is the UI text notification panel where displays events of the match like player's kills and other in-game eliminations, showing to the players who get eliminated and who eliminated him.\n \nNormally, this panel is placed in a corner of the screen to not interfere with the gameplay but easily visible.\n  \nHere I'll show you a few options that you have to customize this system without requiring to touch the code and how you can display your own events on it.");
            DownArrow();
            DrawText("• MFPS comes with two modes to display kills events on the kill feed, in a kill event there are 3 parts of event information to display: the name of the player who was eliminated, the name of the player who did the elimination, and the weapon or cause of the elimination.\n \nYou have two ways of shows the cause of the eliminations:\n \n<b>Weapon Name:</b>");
            DrawServerImage(0);
            DrawText("<b>Weapon Icon:</b>");
            DrawServerImage(1);
            DrawText("By default <b>Weapon Icon</b> is the default option, you can change that in <b>GameData</b> -> KillFeedWeaponShowMode.\n \nAnother option to customize is the color to highlight the local player name when this " +
                "appear in the kill feed, for the context player names in kill feed are represented by the color of his Team but in order to the local player easily knows when an event that include him appear in the kill feed, his name " +
                "should be highlight with a different color, <b>to choose that color</b> go to GameData -> <b>HighLightColor.</b>\n \nOkay, that are the front-end customize options, if you want to customize the way that the UI looks" +
                " you have to do the in the UI prefab which is located in: <i>Assets -> MFPS -> Content -> Prefabs -> UI -> Instances -> <b>KillFeed</b></i>, drag this prefab inside the kill feed panel in canvas which is located by default in: ");
            DrawServerImage(2);
            DrawText("Right, these are all customize options that you have in front end, if you wanna create your own events to display, check the next step.");
        }
        else if (subStep == 1)
        {
            DrawTitleText("CREATE KILLFEED EVENTS");
            DrawText("The kill feed system various type of events to display, use the one that fits your event:\n \n<b>Kill Event:</b>\n \n• This is should use when of course a kill event happen, but a kill that include two actors " +
                "the killer and the killed, to show that you have to call this:");
            DrawCodeText("bl_KillFeed.Instance.SendKillMessageEvent(string killer, string killed, int gunID, Team killerTeam, bool byHeadshot);");
            DrawText("<b>Message:</b>\n \n• If you want to show a simple text of an event in concrete that doesn't include a player in specific, use:");
            DrawCodeText("bl_KillFeed.Instance.SendMessageEvent(string message);");
            DrawText("<b>Team Highlight:</b>\n \n• If you want to show a text of an event in concrete that as subject have a team in specific and you wanna highlight a part of the text with the tam color, use:");
            DrawCodeText("bl_KillFeed.Instance.SendTeamHighlightMessage(string teamHighlightMessage, string normalMessage, Team playerTeam);");
        }
    }

    void PlayerClassesDoc()
    {
        DrawText("MFPS use \"<b>Classes</b>\" system to diversify the weapon loadout, these classes are: <b>Recon, Support, Assault, and Engineer</b> each with distinct weapons.\n\nYou can set up the weapon loadout of each class per player prefab in the <b>bl_GunManager</b> script attached in the <b>WeaponsManager</b> object inside of the player prefab.\n\nEach class require 4 weapons <i>(Primary, Secondary, Perk and Letal)</i>, for set up the default weapons for each class you have two options: Create a new Present/ScritableObject or Edit the default one, basically you only have to create a new Scriptable if you want to keep a backup of the current class setup or if you want use a different setup in a player prefab, if you don't need that, simply edit the default instance.\n\nindependent if you want to create or just edit open the Player Prefabs or the specif player prefab that you want to edit the player class for, then go to the <b>WeaponsManager</b> object -> <b>bl_GunManager</b> inspector -> foldout the target Player Class -> set the weapons for each slot.\n\n(if you want to create a new present before edit, simply click in the <b>New</b> button");
        DrawServerImage(8);
        DrawText("You also can edit the default class loadouts from the Project Window in the folder: <i>Assets->MFPS->Content->Prefabs->Weapons->Loadouts</i>\n");
    }

    void HeadBobDoc()
    {
        DrawText("<b>Head Bob</b> is the camera movement that simulates the reaction of the player head when walk or run, in MFPS this movement is procedurally generated by code and you can adjust the value to obtain the desired result.\n\nIn order to obtain a more realistic result in MFPS we have sync the weapon bob and the head bob movement, so the settings will apply to both movements.\n\nYou can modify the values in the bl_WeaponBob.cs<i> (Attached in WeaponsManager object inside the players prefabs')</i>, you can edit in runtime to preview the movement as you edit it.\n");
        DrawServerImage(9);
        DownArrow();
        DrawText("If you want to use different movements per player or just want to have a backup of the current movement settings you can create a new \"Present\" of the settings and assing it in the script instead of the current one.\n\nFor create a new present simple select the folder where you wanna create it <i>(In Project View)</i> -> Right Click -> MFPS -> Weapons -> Bob -> Settings -> Drag the created profile in the bl_WeaponBob -> Settings -> Them make the changes that you want.\n");
    }

    void AfkDoc()
    {
        DrawTitleText("AFK");
        DrawText("AFK is an abbreviation for <i>away from keyboard</i>, players are called AFK when they have not interact with the game in a extended period of time.  in multi player games AFK players could be a problem," +
            "like for example in MFPS where players play in teams, an AFK player represent free points for the enemy team, or in different context AFK player are used to leveling up, so that is way some games count with a system " +
            "to detect these AFK players and kick out of the server/room after a certain period of time begin AFK.  MFPS include this system but <b>is disable by default</b>.\n \n" +
            "In order to enable AFK detection, go to GameData -> turn on <b>Detect AFK</b>, -> set the seconds before kick out players after detected as AFK in <b>AFK Time Limit</b>");
    }

    void KickVotationDoc()
    {
        DrawTitleText("KICK VOTATION");
        DrawText("In order to give an option to players to get rip of toxic, hackers, non-rules players in a democratic way where a player put the option on the table and the majority of the players in room " +
            "decide to kick out or cancel the petition, MFPS include a voting system.\n \nTo start a vote in game, players have to open the menu -> in the scoreboard click / touch over the player that they want to request the vote -> " +
            "in the PopUp menu that will appear -> Click on <b>Request Kick</b> button.\n \nBy default the keys to vote are F1 for Yes and F2 for No, you can change these keys in bl_KickVotation.cs which is attached in <b>GameManager</b> " +
            "in maps scenes.");
        DownArrow();
        DrawText("If you want to implement your own way to start a voting request, you can do it by calling:");
        DrawCodeText("bl_KickVotation.Instance.RequestKick(Photon.Realtime.Player playerToKick);");
    }

    private AssetStoreAffiliate soundsAssets;
    void AudioDoc()
    {
        if (subStep == 0)
        {
            DrawText("<b><size=22>BACKGROUND AUDIO</size></b>\n \nBy default MFPS only use <b>background audio in the Lobby/MainMenu</b> scene, in order to change that soundtrack or remove it, you can do it by:\n \n■ In the <b>MainMenu</b> scene -> Lobby -> Scene -> AudioController -> <i>(Inspector window)</i> bl_AudioController -> assign/remove the audio clip in the field '<b>Background Clip</b>'");
            Space(20);
            DrawText("<b><size=22>BULLET HITS</size></b>\n \nThere's a bullet hit sound effects that's played when the bullet hit particle is instanced, by design that sound volume is really low since it can be annoying for some persons, in order to change the audio sound effect or volume of these bullet impacts <b>you have to open the Bullet Impact particle prefab</b> which by default are located in the MFPS folder at <i>Assets -> MFPS -> Content -> Prefabs -> Level -> Particles -> WeaponEffects -> Prefabs->*</i>\n \nOpen the prefab of the impact you want to modify -> in the <b>Audio Source</b> component attached in the prefab, assign/replace the Audio Clip and adjust the volume as you please.");
            Space(20);
            DrawText("<b><size=22>PLAYER HIT</size></b>\n \nAnother hit sound that is played is when the local player gets hit, there are two different sounds played, one when is hit by a bullet and the other when is any other kinda injure.\n \nYou can change these sounds per player prefab, simply select the <b>Player Prefab</b> where you wanna change the sounds -> bl_PlayerHealthManager -> in the lists:\n \n<b>Hits Sounds:</b> for the bullet hit sound effect\n<b>Injure Sounds:</b> for... well, the injured sound effects :D");
        }
        else if (subStep == 1)
        {
            DrawText("You may encounter a scenario where in your custom map you can hear players/bots shooting, footsteps, explosions, etc... sounds far away as if it were near to you, this is because the default audio ranges are not ideal for your map size, but this can be easily modified.");
            DrawText("<b><size=16>Adjust Audio Ranges</size></b>\n \nTo adjust the audio ranges in a specific map > open your map scene in the editor > in the hierarchy window go to <b>GameManager</b> > <b>Audio Manager</b> > in the inspector window of this object > <b>bl_AudioController</b> > you will see some slider parameters that you can use to adjust the range of certain sound types, the ranges are defined in meters which means setting up a range to 50 means the audio will be heard only when the player is 50 meter or close to the audio source.\n \nthe audio volume will gradually fade out based on the origin and the max distance.");
            DrawServerImage("img-47.png");
        }
        else if (subStep == 2)
        {
            DrawText("In case you are looking for sounds to replace the default ones in the game or maybe you need new sounds for your weapon or players, below you will find a hand-picked collection of assets that you can acquire from the Asset Store");
            Space(20);
            if (soundsAssets == null)
            {
                soundsAssets = new AssetStoreAffiliate();
                soundsAssets.randomize = true;
                soundsAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555517340/widget-medium");
                soundsAssets.FixedHeight = 400;
            }
            else
            {
                soundsAssets.OnGUI();
            }
        }
    }

    void AntiCheatDoc()
    {
        DrawSuperText("Hacking is one of the obstacles that soon or later you will face as a game developer, but especially in competitive multiplayer games, you will have to deal with those modders/hackers trying to exploit your game to get some advantage over regular players, because that, you have to take some measures since the beginning of your project to protect your game.\n \nBy default, MFPS doesn't include any Anti-Cheat system because these are big and complex systems that by they selves worth more than the MFPS core price, but since version 1.9.2, <b>MFPS comes with the basic integration of a third-party anti-cheat asset</b> that solves most of the common hacking methods used nowadays, this asset is the\n<?link=https://assetstore.unity.com/packages/tools/utilities/anti-cheat-toolkit-2021-202695?aid=1101lJFi>Anti-Cheat Toolkit (ACTk)</link>\nfrom the Asset Store, with it you can prevent the players to be able to cheat in your game e.g modifying the player health, ammo, drops, coins, etc...\n \nBy default, the MFPS core just includes the basic integration that allows prevents the players to modify these values, but there's an addon that extends the features <b>allowing automatically banning players that use Speed hacks, memory injection, code injection, or walk hacks</b>, it is the <?link=https://www.lovattostudio.com/en/shop/addons/anti-cheat-and-reporting/>MFPS Anti Cheat addon</link>.\n \nAlthough this will drive away most of the wanna-be hackers, this would not be enough for more experienced/hardened modders, actually, nothing is unhackable, you can only make it harder so they give up trying, with that in mind here you have other implementations and changes that you can do to make your game more secure in any platform:");
        Space(20);
        DrawSuperText("<b><size=14>1. Use IL2CPP</size></b>\n \nUnity supports 2 scripting backend at the moment: <b>Mono</b> and <b>IL2CPP</b> where this last one is newer and more secure, IL2CPP does produce raw binary code with metadata instead of Mono’s IL bytecode making all IL reversing tools useless and making it much, much harder to get good decompilation of your game code.\n \nThis is built-in Unity and you simply have to change the scripting backend option in the <b>Project Settings > Player > Scripting Backend</b>.\n \n\n<b><size=14>2. Code Obfuscation</size></b>\n \nCode obfuscation is the process of randomly generating names for your script parameters, functions, properties, etc... in the build process in order to make it pretty much unreadable for anyone that decompiles your binaries, the code will become a mess of useless names making reconstructed IL assembly very hard to reverse-engineer and analyze a complete nightmare.\n \nUnfortunately, Unity doesn't come with such a feature included, and third-party systems are required, the most popular and easy to use obfuscator for Unity is available on the Asset Store:\n<?link=https://assetstore.unity.com/packages/tools/utilities/obfuscator-48919?aid=1101lJFi>Obfuscator</link>");
        DrawHorizontalSeparator();
        DrawText("<b><size=16>Enable Anti-Cheat</size></b>\n \nIn order to enable the anti-cheat integration, if you are using the addon for the advance integration > follow the addon documentation, if you are not using the addon but you have installed the Anti-Cheat Toolking assets and want to enable the basic integration included in the core package > go to the editor top navigation menu > Tools > Code Stage > Anti-Cheat Toolking > Settings... > Conditional Compilation Symbols > check the <b>ACTK_IS_HERE</b> check box and that's.");
    }

    void FPArmsMaterial()
    {
        DrawText("Normally you will use the same hand model for all of your weapons model, using a different material and textures for each team, so you may encounter with the inconvenient of change the hand texture for each weapon in the player prefabs is a little bit annoying, well MFPS handle this.\nYou don't have two manually change the arms, sleeves, gloves, etc.. materials, you only" +
            "have to create a prefab and list all the arms materials along with the different textures per team.");
        DownArrow();
        DrawText("Let's start by creating a new \"Arms Material\" asset, in the <b>Project Window</b> select a folder where save the asset and do <b>Right Mouse Click</b> -> MFPS -> Player -> <b>Arm Material</b>");
        DrawServerImage(4);
        DownArrow();
        DrawText("Then select the created material and in the Inspector window you'll see a List, in this list you have to add all the materials your Arms model <b> that change of texture depending of the player team</b>, for example the default MFPS arms model have 3 materials: Sleeve, Skin and Gloves, but only the Sleeves and Gloves material change of texture, the skin is the same, so only those two materials are include in the list.\nProbably you only have to add the Gloves Material, so add a new field on the list and assign the material and add the different Textures depending of the Team.\n \nWith that the materials will automatically change of textures in runtime depending on which team the player spawm.");
        DrawServerImage(5);
    }

    void RoomPropertiesDoc()
    {
        DrawHyperlinkText("There're some properties that are different per room/match besides the game mode, that you can tweak, e.g: the max players options, max rounds time limits, game goals, etc... these properties options can be different per game mode and you can modify them in the game mode info like this:\n\n► Go to <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ Game Modes ➔ <i>*Open a game mode*</i> ➔ there you will see the list and\noptions to modify these properties.");

        DrawHorizontalColumn("Max Players", "the maximum number of players options that can join in the room for the game mode, for two-team game modes, the max number of the player for each team is half of the max players allowed.");
        DrawHorizontalColumn("Game Goals Options:", "the score, point, or kills goals options of this game mode.");
        DrawHorizontalColumn("Time Limits:", "the round time limit options for this game mode (in seconds).");
        DrawServerImage("img-45.png");
    }

    void DrawTeamsDoc()
    {
        DrawText("On MFPS there are various game modes that use Team systems like CTF (Capture the Flag) or TDM (Team Death Match), for default these teams are named as \"Delta\" and \"Recon\", you can modify these team names and representative color, for it go to <b>Game Data</b> in find the \"Team\" section:\n");
        DrawServerImage(13);
    }

    void DrawCoins()
    {
        if(subStep == 0)
        {
            DrawHyperlinkText("MFPS Integrate a virtual coin/currency system with two different coins, one that can be earned by playing the game <i>(with XP)</i> and another that should only be acquired by purchasing it with real money <i>(using the <link=https://www.lovattostudio.com/en/shop/network/shop/>Shop addon</link> or your custom IAP)</i>.\n \nYou can customize many aspects of these coins without touching any code, like <b>the names of the coins, color, icon, and value</b>.\n \nTo modify these properties, go to <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ Game Coins ➔ unfold the coin you want to modify, most of the properties names are self-describing, but here is the explanation of the ones that could confuse you:");
            DrawHorizontalSeparator();
            DrawHorizontalColumn("Acronym", "An abbreviation of the coin name.");
            DrawHorizontalColumn("Coin Value", "The value of the coin with respect to 1, <b>e.g</b>: when you set the price for an in-game item let say 100 for a weapon if the coin value is equal to 0.25 it means that the player will need 400 of that coin to purchase that weapon <i>(0.25 = 1/4 ➔ 400/4 = 100)</i>, or if the coin value is equal to 2, the player just will need 50 of that coin.\n \nThe idea is that you only need to set a price for the in-game items, and the price for each coin is calculated automatically in-game.");
            DrawHorizontalColumn("Initial Coins", "The amount of that coins that new player will receive the first time that play the game.");
            Space(10);
            DrawServerImage("img-38.png", TextAlignment.Center);
            DownArrow();
            DrawSuperText("<?title=20>How to use the coins?</title>\n \nBy default <i>(in the core packages)</i> MFPS does not use the coins for anything, is up to you for what you wanna use them, either to buy game items like weapons, special skins, operators, etc... or implement a lootbox system or anything you can imagine.\n \nThere are some addons if you want to implement a shop system and allow players to buy weapons and coins packs:\n \n<?link=https://www.lovattostudio.com/en/shop/network/shop/>Shop System Addon.</link>\n \nAnd there are addons to integrate a payment system to allow purchases with real money/currencies:\n \n<?link=https://www.lovattostudio.com/en/shop/addons/unity-iap-for-shop/>Unity IAP Addon.</link>\n<?link=https://www.lovattostudio.com/en/shop/network/paypal-for-shop/>Paypal Addon.</link>");
        }else if(subStep == 1)
        {
            DrawSuperText("If you want to do basic coin operations like <b>add</b> or <b>deduct</b> a specific amount of coins to the player wallet you can do so with a single line of code:\n \n<?title=#20>ADD COINS</title>");
            DrawCodeText("bl_MFPS.Coins.GetCoinData(0).Add(100);");
            DrawSuperText("where 0 = the index of the coin in GameData ➔ GameCoins list,\nby default 0 = XP Coin, 1 = Gold Coin.\n \n<?title=20>DEDUCT COINS</title>");
            DrawCodeText("bl_MFPS.Coins.GetCoinData(0).Deduct(100);");
            DrawText("If you are using ULogin Pro addon the coin operation will be executed in the server and saved in the database, if you are not using it, the coins will be stored locally using <b>PlayerPrefs which is not secured</b>, that is why is recommended to save the coins externally in a dedicated database as ULogin Pro does.");
        }
    }

    void GameModesDoc()
    {
        if(subStep == 0)
        {
            DrawText("MFPS comes with 3 different game modes: <b>Team Deathmatch, Capture Of Flag, and Free For All</b>, each of these modes has its respective logic in separate scripts which can be found at: <i>Assets ➔ MFPS ➔ Scripts ➔ GamePlay ➔ GameModes➔*</i>\n \nEach game mode has some common properties that you can customize in the inspector in GameData ➔ Game Modes ➔ *,\nif you foldout one of the modes, you will find all the customizable properties");
            DrawServerImage("img-36.png");
            DrawText("Here what they are for:");
            DrawPropertieInfo("Mode Name", "string", "The name represent this mode that will be displayed in-game.");
            DrawPropertieInfo("Game Mode", "enum", "The internal id of this mode, it's use in code to identify a specific game mode, you can only have a mode in the Game Modes list with the same id.");
            DrawPropertieInfo("Is Enable", "bool", "Will this mode be available in your game?");
            DrawPropertieInfo("Support Bots", "bool", "Can games be created for this mode and filled them with bots? <b>NOTE:</b> that by default, bots are only supported in TDM and FFA modes.");
            DrawPropertieInfo("Auto Team Selection", "bool", "Force auto team selection to keep teams balanced?");
            DrawPropertieInfo("Required Players To Start", "int", "The minimum number of players that has to be joined in the match in order to start the game for this mode.");
            DrawPropertieInfo("On Round Started Spawn", "enum", "When a player join in a match with this mode but a round is already started, what to do?");
            DrawPropertieInfo("On Player Die", "enum", "What happens after a player die in this mode?");
            DrawPropertieInfo("Goal Name", "string", "The name of the points/objective of this game mode, e.g kills, captures, score, etc...");
            DrawPropertieInfo("Allow Pickup Weapons", "bool", "Can players pickup weapons in this mode?");
            DrawPropertieInfo("Max Players", "int[]", "The available options of max players for this mode, these options are the ones that appear in the room creator menu in the lobby.");
            DrawPropertieInfo("Game Goal Options", "int[]", "The available options for this mode goals (kills, captures, score, etc...), these options are the ones that appear in the room creator menu in the lobby.");
            DrawPropertieInfo("Time Limits", "int[]", "The available match times options for this mode set in seconds, these options are the ones that appear in the room creator menu in the lobby.");
        }
        else if(subStep == 1)
        {
            DrawHyperlinkText("Besides the 3 modes included in MFPS, there are more of the popular game modes available for MFPS as add-ons that you can acquire, like <i><b>Demolition/Bomb Defuse, Domination/Cover Point, Gun Race/Gun Play, Elimination, and Kill Confirm</b></i>, all those are available in <link=https://www.lovattostudio.com/en/shop/>our Shop</link>.\n \nBut let's say you want to create a custom game mode, where to start?\nWell, that is a difficult question since each game mode has its unique requirements, different logic, gameplay, etc... because of that, each approach is different, so I can't tell you how to create your game mode per se, what I can do, is tell you how to integrate it to MFPS.\n \nThe first thing is to create an enum identifier for your game mode, which will be used to identify the game mode in the code, for that simply add a pseudonym of the game mode name in the script <b>GameMode.cs</b>, e.g:");
            DrawCodeText("public enum GameMode\n{\n    TDM,\n    FFA,\n    CTF,\n    SND,\n    CP,\n    GR,\n    BR,\n    ELIM,\n    DM,\n    KC,\n    <color=#0D5400FF>MyCustomMode, </color>\n}");
            DrawText("In the above example I added the <i>\"MyCustomMode\"</i> as example, but you can use anything you want, even just the initial as the others mode.");
            DownArrow();
            DrawHyperlinkText("The next thing is to define the script which will handle the integration of MFPS with your game mode, for that MFPS uses the interface <color=#DFFF2AFF>IGameMode</color> to define the functions that your game mode main script must implement, so what you have to do is <link=https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/types/interfaces>define that interface</link> in your main game mode script ➔ override the required functions of the interface in your script, to make easier, you can use the default game modes scripts <i>(bl_TeamDeathMatch.cs, bl_FreeForAll.cs, etc...)</i> as a reference and learn from its implementation\n \nOnce you define the <b>IGameMode</b> interface and its functions, there's a must-have code that you have to implement, in the Awake() function of your script <i>(where you define the interface)</i>, you have to call the Initialize() function <i>(from the Awake function)</i> and in this Initialize() function you have to register this mode with its enum identifier, like this:\n");
            DrawCodeText("if(bl_GameManager.Instance.IsGameMode(GameMode.MyCustomMode, this))\n        {\n            // Active your game mode\n            // Active all the game objects, props, UI, etc... that are for this specific mode\n            // Register to game events needed for this mode\n        }\n        else\n        {\n            // Disable any object/UI that are for this specific mode\n        }");
            DownArrow();
            DrawText("Below you will find a complete script will the default skeleton of the above explained, you can use it as the base and start to modify it with your custom requirements for your mode:");
            DrawCodeText("using Photon.Realtime;\nusing UnityEngine;\n \npublic class MyCustomModeScript : MonoBehaviour, IGameMode\n{\n \n    void Awake()\n    {\n        if (!bl_PhotonNetwork.IsConnected) return;\n \n        // Must have\n        Initialize();\n    }\n \n    #region Interface Overrides\n    public void Initialize()\n    {\n        if(bl_GameManager.Instance.IsGameMode(GameMode.MyCustomMode, this)) // replace 'MyCustomMode' with your mode identifier\n        {\n            // Active your game mode\n            // Active all the game objects, props, UI, etc... that are for this specific mode\n            // Register to game events needed for this mode\n        }\n        else\n        {\n            // Disable any object/UI that are for this specific mode\n        }\n    }\n \n    // Based in you game mode logic, determine if the local player is a winner\n    // This code is called after the game finish.\n    public bool isLocalPlayerWinner => throw new System.NotImplementedException();\n \n    public void OnFinishTime(bool gameOver)\n    {\n        // Automatically called when the game time finish.\n    }\n \n    public void OnLocalPlayerDeath()\n    {\n        // Automatically called when the local player die\n    }\n \n    public void OnLocalPlayerKill()\n    {\n        // Automatically called when the local player kill an enemy\n    }\n \n    public void OnLocalPoint(int points, Team teamToAddPoint)\n    {\n        // Called when you add a point to a team using:\n        // bl_GameManager.Instance.SetPointFromLocalPlayer(1, GameMode.MyCustomMode);\n    }\n \n    public void OnOtherPlayerEnter(Player newPlayer)\n    {\n \n    }\n \n    public void OnOtherPlayerLeave(Player otherPlayer)\n    {\n \n    }\n \n    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)\n    {\n        // Automatically called when a room property change (score, goal, team counts, etc...)\n    }\n    #endregion\n}");
        }
        else if(subStep == 2)
        {
            DrawText("If you want to define which game modes will be allowed and which don't in certain maps of your game, you can do it by setting the games that are NOT allowed in the map info No Allowed Game Modes list, which is located in each map scene info in <i>GameData ➔ All Scenes ➔ *Scene info* ➔ <b>No Allowed Game Modes.</b></i>\n \nThe modes that you add to that list will not be available for that map, and all the rest will be available.");
            DrawServerImage("img-37.png");
        }
    }

    void DrawLobbyChat()
    {
        DrawText("MFPS 2.0 include a <b>lobby chat system</b>, for players by able to communicate between meanwhile search or wait for join to a match, this chat use Photon Chat plugin, for use it you need to have a Photon Chat AppID <i>(it's not the same that Photon PUN AppID)</i>, you can get this appid from your photon dashboard:\n\nGet your AppId from the Chat Dashboard:");
        if(Buttons.FlowButton("Chat Dashboard"))
        {
            Application.OpenURL("https://www.photonengine.com/en-US/Chat");
        }
        DrawText("when you have your Chat AppID, paste it on the PhotonServerSettings:");
        DrawServerImage(6);
    }

    public AnimatorController playerAnimatorController;
    public int customWeaponStep = 0;
    public string[] weaponNames;
    public int customWeaponID, selectedSubMachine = 0;
    public ChildAnimatorStateMachine[] upperSubMachineStates;
    public string[] upperSubMachineStatesNames;
    public string customAnimationFireName;
    private AssetStoreAffiliate playerAnimationAssets;
    void DrawPlayerAnimationDoc()
    {
        if (subStep == 0)
        {
            DrawSuperText("MFPS 2.0 uses Mecanim system to handle the tp player animations, so change animations clips is simple as drag and drop the animation clip in the motion state in the Animator window, you only need a humanoid animation clip and override it in the Animator Override Controller.\n\n\n<?title=16>CHANGE PLAYER ANIMATION</title>\n\nIn order to change a specific animation clip:\n\n- Open the player prefab that you wanna change the animation for <i>(Player prefabs are located in the Resources folder).</i>\n\n- In the Animator component of your soldier model of the player prefab <b>double click</b> on the Controller field:");
            DrawServerImage("img-27.png");
            DrawText("- Now in the <b>Inspector</b> window, you will see the list of the animations clips used in the player animator controller, next to the default animation clips names you can see a field to assign and override/replace the default animation with your custom animation clips.\n \nSo, you simply have to assign your custom animations in the corresponding box, <b>based on the default animation names you can figure out which animation is for what player motion</b>.");
            DrawServerImage("img-28.png");
            DownArrow();
            DrawSuperText("Now, if you want to use differents animations for an specific player prefab, <b>e.g</b> <i>each player prefab have a different soldier mode with custom movement animations for each model</i>, the solution is pretty easy.\n\nYou simply have to create or duplicate the <?underline=>Player Animations [Override]</underline> controller and assign it in the Animator component of your soldier model in the player prefab:");
            DrawAnimatedImage(6);
        }
        else if (subStep == 1)
        {
            DrawSuperText("<?title=18>CHANGE ANIMATIONS <b>ADVANCE</b></title>\n\n- Change the animations from the <?underline=>Animator Override Control</underline> <i>(from the Basic section)</i> have a limitation, and it's that you can only replace the animation clips, the problem is that by default some animations are used in multiple states of the Animator StateMachine, e.g <i>the 'reload-ar' which is the weapon reload animation is used for the Rifle, Pistol and Sniper weapons</i>, if you want to use a different animation for each type of weapon you will have to modify the Base <?underline=>Animator Controller</underline>.\n\n1. Duplicate the default Player Animator Controller asset (<?link=asset:Assets/MFPS/Content/Art/Animations/Player/Controllers/Player [Controller].controller>Player [Controller]</link>) ➔ Select the asset in the Project window ➔ Ctrl + D or Command + D on MAC.\n\n2. Assign the duplicated <b>Animator Controller</b> in the Animator component of the soldier model of the player prefab that you want to modify the animation for.\n\n3. Open the duplicated Animator Controller in the Animator window <i><size=9><color=#76767694>(double click on the Animator Controller)</color></size></i> ➔ Find the animation state that you want to change the animation clip, on the Animator view you need figure out for what part of player body is this animations Bottom or Upper body <i>(Legs or Arms)</i>, e.g the <b>Rifle Reload</b> motion is for the Arms, so it is for the Upper Body, so go to <b>Layers</b> - and select the <?underline=>Upper</underline> layer, there you will see various state machine with the name of the weapon types to which it belongs ➔ open the weapon state machine:");
            DrawServerImage("img-29.png");
            DrawSuperText("Now you will see others animation states, those represent the weapon motion clips, in this example we are looking for the \"Reload\" state, so, select the Reload state ➔ in the inspector view you will see the settings of this state, what we are interested in is the <?underline=>Motion</underline> field, in that field you to need to assign the animation clip with which you want to replace the default one.\n\nOnce you do that, you are ready to go, you can replace all other animation clips if you want.");
            DrawServerImage(16);
        }
        else if (subStep == 2)
        {
            DrawText("As you may already know in MFPS the first person and third person animations are different, the same applies to the weapon animations, you can't use the use FPWeapon animations for the TPWeapons, instead, humanoid animations are needed for the TPWeapons.");
            DrawNote("TPWeapon animation set = the player animations needed for a weapon (Idle, Reload, Run, and Fire).");
            DrawText("By default, MFPS uses different weapon animations for each weapon type <i>(Machinegun, Pistol, Sniper, Grenade, etc...)</i> a set of animations for all the weapons of the same type which means you have multiple <b>Sniper</b> weapons, all of them will play the same set of sniper TP animations.\n \nCould be the case in where you want to <b>use a custom animation set for a specific TPWeapon</b>, this will require creating a new <b>SubStateMachine</b> in the player <b>Animator Controller</b> and creating the necessary transitions, below you will find an automated guide in how to set up the SubStateMachine in order to use custom animations for a TPWeapon.");
            DrawHorizontalSeparator();
            DrawTitleText("ADD CUSTOM WEAPON ANIMATIONS");
            DrawSuperText("The first thing we need is to know in which <b>Animator Controller</b> we want to add SubMachineState for the animation set, by default, the default Animator Controller which is used for all the MFPS player prefabs is located in: <i>Assets ➔ MFPS ➔ Content ➔ Art ➔ Animations ➔ Player ➔ Controllers ➔ Player [Controller]</i> <?link=asset:Assets/MFPS/Content/Art/Animations/Player/Controllers/Player [Controller].controller>(Click here to ping it)</link>\n \nIf you haven't changed the Animator Controller in any player prefab simply drag the default Animator Controller <i>(from the above path)</i> in the field below, otherwise drag the Animator Controller that you are using.");
            Space(10);
            var lw = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 150;

            EditorGUILayout.BeginHorizontal("box");
            Space(20);
            playerAnimatorController = EditorGUILayout.ObjectField("Player Animator Controller", playerAnimatorController, typeof(AnimatorController), true) as AnimatorController;
            if (customWeaponStep == 0)
            {
                GUI.enabled = playerAnimatorController != null;
                if (Buttons.FlowButton("Continue"))
                {
                    customWeaponStep++;
                }
                GUI.enabled = true;
            }
            Space(20);
            EditorGUILayout.EndHorizontal();

            if (customWeaponStep > 0 && playerAnimatorController != null)
            {
                Space(10);
                DrawText("Now, in the dropdown below select the <b>weapon information</b> for which you want to add the custom animations and then click on the <b>Select</b> button.");
                if (weaponNames == null)
                {
                    weaponNames = bl_GameData.Instance.AllWeaponStringList();
                }
                EditorGUILayout.BeginHorizontal("box");
                Space(20);
                customWeaponID = EditorGUILayout.Popup("Weapon", customWeaponID, weaponNames);
                if (Buttons.FlowButton("Select"))
                {
                    customWeaponStep++;
                }
                Space(20);
                EditorGUILayout.EndHorizontal();
            }

            if (customWeaponStep > 1 && playerAnimatorController != null)
            {
                Space(10);
                DrawText("Alright, now you have to do this manually:\nFirst, open the <b>Animator Controller</b> in the <b>Animator window</b>, for it select the Animator Controller <i>(the one you assigned in the field above)</i> in the <b>Project View window</b> ➔ double click over it or right mouse click > <b>Open</b>.\n \nIn the Animator window that should be opened, select the <b>Layers</b> tab in the top left corner ➔ Select the second layer <b>Upper</b> ➔");
                DrawServerImage("img-43.png");
                Space(10);
                DrawSuperText("<?background=#CCCCCCFF>Duplicate one of the SubStateMachines</background>\n \nDuplicate a SubMachineState of a weapon of the same type that you want to add the animations, <b>e.g</b> if your custom animations are for a shotgun > Duplicate the Shotgun SubMachineState.\n \nFor duplicate the SubMachineState simply select it > right mouse click > Copy > right mouse click in an empty space > Paste:");
                DrawAnimatedImage(7);
                DownArrow();
                DrawText("Once you duplicate it, click on the button below to fetch the Animator Controller data and proceed with the set up");
                if(GUILayout.Button("Fetch Animator Controller Data"))
                {
                    var rootStateMachine = playerAnimatorController.layers[1].stateMachine;
                    upperSubMachineStates = rootStateMachine.stateMachines;
                    upperSubMachineStatesNames = upperSubMachineStates.Select(x => x.stateMachine.name).ToArray();
                    customWeaponStep++;
                }
            }

            if (customWeaponStep > 2 && playerAnimatorController != null)
            {
                Space(10);
                DrawText("Alright, now in the dropdown below, select the <b>SubMachineState</b> that you just duplicate and click on the <b>Setup</b> Button.");
                GUI.enabled = customWeaponStep == 3;
                EditorGUILayout.BeginHorizontal("box");
                Space(20);
                selectedSubMachine = EditorGUILayout.Popup("SubMachineState", selectedSubMachine, upperSubMachineStatesNames);
                if (Buttons.FlowButton("SetUp"))
                {
                    if (SetupWeaponSubMachine(upperSubMachineStates[selectedSubMachine].stateMachine, playerAnimatorController))
                    {
                        customWeaponStep++;
                    }
                }
                Space(20);
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
            }

            if (customWeaponStep > 3 && playerAnimatorController != null)
            {
                Space(10);
                DrawText("Good, now the SubStateMachine is ready to use, the next step is to assign the player weapon animations ➔ in the Animator window select the <b>SubStateMachine</b> with the name of the weapon that the animations are for <i><b>(The name of the SubStateMachine that you just duplicated was automatically renamed to the weapon name that you selected)</b></i> ➔ Double click over SubStateMachine ➔ You should see some AnimatorStates <b>(Run, Idle, Reload, and Fire)</b> ➔ you can replace the Animation Clip of each state by selecting the State ➔ redirect to the Inspector window ➔ Assign the Animation Clip in the <b>Motion</b> field.");
                DrawAnimatedImage(8);
                DownArrow();

                DrawText("Once you finish assigning the animations, you are pretty much done, the last thing to do is to assign the custom animation information in the <b>TPWeapon > bl_NetworkGun</b> inspector, for it, open a player prefab <i>(you will have to do this for all the player prefabs that you are using)</i> ➔ Select the TPWeapon inside the player prefab ➔ in the inspector check the <b>Use Custom Player Animations</b> toggle ➔ Infill the next fields with this info:");
                Space(10);
                if (string.IsNullOrEmpty(customAnimationFireName))
                {
                    var weaponInfo = bl_GameData.Instance.GetWeapon(customWeaponID);
                    customAnimationFireName = weaponInfo.Name;
                }
                DrawHorizontalColumn("Custom Animator State ID", (20 + customWeaponID).ToString(), 175);
                DrawHorizontalColumn("Custom Fire Animation Name", customAnimationFireName + "Fire", 175);
                DrawServerImage("img-44.png");
                DrawText("That's");
            }

            EditorGUIUtility.labelWidth = lw;
        }
        else if(subStep == 3)
        {
            DrawText("In case you are looking for player animations to replace the default ones in the game, below you will find a hand-picked collection of assets that you can acquire from the Asset Store");
            Space(20);
            if (playerAnimationAssets == null)
            {
                playerAnimationAssets = new AssetStoreAffiliate();
                playerAnimationAssets.randomize = true;
                playerAnimationAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555464133/widget-medium");
                playerAnimationAssets.FixedHeight = 400;
            }
            else
            {
                playerAnimationAssets.OnGUI();
            }
        }
    }

    private bool SetupWeaponSubMachine(AnimatorStateMachine stateMachine, AnimatorController animatorController)
    {
        if(stateMachine == null)
        {
            Debug.LogWarning($"The StateMachine couldn't be found in this Animator Controller.");
            return false;
        }

        var weaponInfo = bl_GameData.Instance.GetWeapon(customWeaponID);
        int upperID = 20 + customWeaponID;
        var rootStateMachine = animatorController.layers[1].stateMachine;
        var equipState = rootStateMachine.states.ToList().Find(x => x.state.name == "Equip").state;

        if (equipState == null)
        {
            Debug.LogWarning($"The Equip animation state couldn't be found in the animator controller.");
            return false;
        }

        stateMachine.name = weaponInfo.Name;

        var subMachineStates = stateMachine.states.ToList();
        var idleState = subMachineStates.Find(x => x.state.name == "Idle");

        if(idleState.state == null)
        {
            Debug.LogWarning($"The Idle animation state couldn't be found in the animator controller.");
            return false;
        }

        var transition = equipState.AddTransition(idleState.state);
        transition.AddCondition(AnimatorConditionMode.Equals, upperID, "GunType");

        foreach (var state in subMachineStates)
        {
            if (state.state.name.Contains("Fire"))
            {
                state.state.name = $"{weaponInfo.Name}Fire";
            }
            var ts = state.state.transitions;
            foreach (var transit in ts)
            {
                var conditions = transit.conditions;
                var newConditions = new AnimatorCondition[conditions.Length];
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (conditions[i].parameter == "GunType" && conditions[i].mode == AnimatorConditionMode.NotEqual)
                    {
                        var outCondition = new AnimatorCondition();
                        outCondition.parameter = "GunType";
                        outCondition.mode = AnimatorConditionMode.NotEqual;
                        outCondition.threshold = upperID;
                        newConditions[i] = outCondition;

                    }
                    else
                    {
                        newConditions[i] = conditions[i];
                    }
                }
                transit.conditions = newConditions;
            }
        }

        EditorUtility.SetDirty(animatorController);
        return true;
    }

    void NamePlatesDoc()
    {
        DrawSuperText("Name Plates = <b>Above Head Player Name</b>, is the GUI Text that appears on teammates players in-game,\nit's a pretty basic feature in all multiplayer games, in MFPS this GUI is rendering using the Unity legacy OnGUI system for its simplicity for the required purpose.\n \nCustomize the design of this GUI is quite simply:\n \nIn each player prefab you will found the script <?underline=>bl_NamePlateDrawer.cs</underline> attached to it, now in order to make easier to modify the look of the name plate UI you can preview it in the editor <i>(in edit mode)</i> by instance the player prefab in a scene ➔ select the player prefab instance ➔ bl_NamePlaterDrawer ➔ Click on the button <b>Simulate [OFF]</b>.");
        DrawServerImage("img-30.png");
        DrawText("Now with the preview On, you can edit the design with the frontend properties, for it on the inspector window of bl_NamePlateDrawer ➔ click on the button Edit Present ➔ this will open the GUI style properties of the name plate.");
        DrawServerImage("img-31.png");
        DrawText("Customize the properties as you desire and preview the change in realtime in the Scene View window ➔ Once you're done, turn off the Simulation by clicking again in the <b>Simulation [On]</b> button ➔ Apply the changes to the player prefab.");
        DownArrow();
        DrawHyperlinkText("<b><size=16>Hide Health Bar</size></b>\n\nIf you want to hide the health bar from the name plate of teammates and only show the player name, you can do it by turn off the toggle <b>Show Teammates Health Bar</b> in <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link>.");
    }

    void PlayerPrefabsDoc()
    {
        DrawHyperlinkText("We called <b>Player Prefabs</b> to the unity prefab that contain all the required scripts, objects, and structure that make up the player controller.\n\nThe MFPS player prefabs are located in a special Unity folder called <b>'Resources'</b> <i>(you can find it inside the MFPS folder)</i>, if you wanna change anything related to one of the players like, modify a weapon position, animation, a script property, etc... you have to apply the change to these prefabs.\n\n<b>By default, MFPS uses 2 player prefabs</b> which are assigned in the <b><link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link></b> ➔ <b>Player1</b> and <b>Player2</b>, Player1 is used for Team 1 and Player2 for Team 2, in case the game mode is not a team-based mode like the Free For All game mode, the Player1 is used.\n\nSince version 1.8 you can also override the Player1 and Player2 per scene, which means that you can use different player prefabs for each team in each map, to do that you simply have to attach the script <b>bl_OverridePlayerPrefab.cs</b> in any object of your map scene ➔ then in the inspector of this script you will see the fields to assign the player prefabs and that's all.");
        DownArrow();
        DrawHyperlinkText("In case you are looking for a more advanced solution for the player selection or want to add more player prefabs where the players can select in-game their character, you should take a look at the <link=https://www.lovattostudio.com/en/shop/addons/player-selector/>Player Selector</link> addon.\n");
    }

    void DrawBullets()
    {
        if (subStep == 0)
        {
            DrawText("in MFPS bullets are pooled and like all other pooled objects in MFPS they are listed in <b>bl_ObjectPooling</b> script, which is attached in the <b>GameManager</b> on each map scene.\nin <b>bl_Gun</b> you assign only the \"Pooled Name\".\n\n\nYou may want to add a new bullet for a specific weapon, let's say you want add a different Trail Render, well for do it you can do this:\n\n<size=18>Duplicated a bullet prefab:</size>\n\nSelect one of the existing prefab located at MFPS ➔ Content ➔ Prefabs ➔ Weapon ➔ Projectiles ➔ *, select the prefab and Ctrl + D to Duplicated, or Command + D on Mac.\n\nThen make the changes that you want to this duplicated prefab and after this, add a new field in <b>bl_ObjectPooling</b> <i>(it is attached in GameManager object in room scenes)</i>, in the new field drag the bullet prefab and change the pooled name:");
            DrawServerImage(17);
            DrawText("then open a player prefab and select the <b>FPWeapon</b> that you want assign the bullet, in the bl_Gun script of that weapon, write the pooled name of the bullet in the field \"Bullet\".\n");
            DrawServerImage(18);
            DrawText("Apply changes to the player prefab and ready.");
        }
        else if (subStep == 1)
        {
            DrawSuperText("The bullet decals are pooled and automatically placed when a bullet hits a collider, the decal material will be selected based on the collider tag, you can define as many decals as you want.\n \nThe <b>Bullet Decal Manager</b> is located under the <b>GameManager</b> object in each map scene, <i><b>GameManager ➔ Bullet Decal Manager ➔ bl_BulletDecalManager</b></i>.\n \n<?title=18>In order to modify, add or remove a decal:</title>\n \n<?list=■>Create a new material with a simple alpha shader\nAssign the decal texture to this material.\nIn the<b>Bullet Decal Manager ➔ Decal List ➔ Surface Decals</b>, add a new tag field if the decal is for a new Tag, or if it is for an existing one, simply unfold the field.\nAdd the Decal material to the <b>Decal Materials</b> list.\nThat's.</list>");
            DrawServerImage("img-39.png");
        }
        else if (subStep == 2)
        {
            DrawText("If you need to create a different type of bullet, not just a trail renderer change but a different ballistic or a different hit detection, this is possible to do without breaking changes.\n \nThe bullet or projectiles can be extended and/or a custom class can be used, <b>you should not modify the default bullets/projectiles scripts</b>,\ninstead, create a new class and inherit it from the <color=#FFCC2AFF>bl_ProjectileBase</color> class, override the required functions and use the default scripts as reference only and code the logic of your bullet/projectile as needed.\n \nAttach your custom script to a game object and create a prefab of it, then add this prefab in the pooled list and you are ready to go.");
        }
    }

    void DrawKitsSystem()
    {
        DrawText("MFPS have a simple but functional Kit System where players can throw and pick up Ammunition or Medic kits in the map during the game, by default player can throw these kits with the <b>'H'</b> key, the type of kit <i>(ammo or medic)</i> depend of the player class.");
        DrawServerImage(7);
        DownArrow();

        DrawTitleText("Change the Key to throw kits");
        DrawText("- in the root of Player prefabs you have a script called <b>bl_ThrowKits</b>, in this one you have the property called <b>Throw Key</b>, there you can set the Key code for throw the kits.\n");
        DownArrow();
        DrawTitleText("Change the model of the kits");
        DrawServerImage(19);
        DrawText("•  You can find the kits prefabs in: <i>Assets -> MFPS -> Content -> Prefabs -> Level -> Items->*</i>\n\n•  Select the kit that you want to change the model (MedKit or AmmoKit) and drag to the scene hierarchy.\n\n•  Replace the mesh with you new model, apply the changes and save the prefab.\n\n");
        DownArrow();
        DrawTitleText("Change the model of Kit deploy indicator");
        DrawServerImage(20);
        DrawText("You can find the prefab in <i>Assets -> MFPS -> Content -> Prefabs -> Level -> AirDrop->*</i>\n");
    }

    void DrawKillZones()
    {
        DrawText("may be the case that there are limits in your map that you want to the players don't go any further, a solution that MFPS have for these cases is the <b>Kill Zones</b> where if the player enter, a warning will appear with a count down timer, if the player not leave this zone before the timer reach 0, he will automatically killed by the game and returning to a spawnpoint.\n");
        DrawServerImage(21);
        DrawText("to add a kill zone simple add a object with a <b>Box Collider</b> <i>(the Box Collider represent the zone)</i>, then add the script bl_DeathZone.cs script, setup the time that the player have to leave this zone and the string message that will appear in screen while player is in kill zone.\n");
    }

    void DrawGameSettings()
    {
        DrawText("MFPS allows players to modify some game settings of the game in-runtime like graphics quality and control settings, you as the developer are in charge to set the default values for these settings, the value that the player will have the first time that they play the game.");

        DrawHyperlinkText("To set the default values go to <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ Default Settings ➔ Setting Values ➔ *");
        DrawServerImage(28);
        DrawText("In this list, you will have all the available values, simply unfold the setting that you want to modify and set the desired value.");
        DownArrow();
        DrawTitleText("Add a new setting");
        DrawText("Add a new setting is really simple, in the same list <i><b>(Setting Values)</b></i> add a new field, set a unique name to the to identify the setting ➔ set the type of setting <i>(float, integer, bool, or string)</i> then set the default value.\n\nNow to use this value in-game you can load it with:");
        DrawCodeText("var val = bl_MFPS.Settings.GetSettingOf('THE_SETTING_NAME');");
        DrawText("once the setting is added in the list, it will automatically be saved when the player applies the settings in-game <i>(click on the <b>Save</b> button)</i>, but if you want to save the setting with your own rules you can do it with:");
        DrawCodeText("bl_MFPS.Settings.SetSettingOf('THE_SETTING_NAME', THE_SETTING_VALUE);");
        DrawText("As a reference of how you can use it in-game you can inspect the script <b><color=#00E9FFFF>bl_SingleSettingsBinding.cs</color></b>\n");
    }

    void MouseLookDoc()
    {
        DrawHyperlinkText("The Mouse Look/Camera controller is a key feature in shooter games or any fast-paced action game.\n \nIn MFPS a few techniques are used to improve the accuracy and smoothness of the look movement like <b>Frame Smoothing</b> and <b>Movement Smoothness</b>.\n \nIf you want to personalize the movement, there're some properties that you can modify in the inspector:\n \nFor the general setup, you can find the properties in <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> -> Mouse Look Settings.");
        DrawPropertieInfo("Use Smoothing", "bool", "Use Frame Smoothing?, a technique that allows achieving a more fluid movement by using the average mouse movement calculated from a certain number of past frames.");
        DrawPropertieInfo("Frame Of Smoothing", "int", "The number of frames that will be buffer to calculate the average movement, more means more smooth but less precise.");
        DrawPropertieInfo("Lerp Movement", "bool", "Apply an extra smooth layer to the movement? good for touch devices, not recommended for mouse/gamepad controllers.");
        DrawPropertieInfo("Aim Sensitivity Adjust", "enum", "Determine how the mouse sensitivity will transition to the Aim sensitivity, <b>Fixed</b> = To the exact Aim sensitivity value, <b>Relative</b> = by calculating the Camera Field Of View change.");
        DrawHorizontalSeparator();
        DrawText("<b><size=22>PER PLAYER PROPERTIES</size></b>\n\nThere're some properties that you can modify per player, you can find them in the Player prefabs ➔ bl_FirstPersonController ➔ Mouse Look ➔ *,");
        DrawHorizontalSeparator();
        DrawHyperlinkText("<b><size=22>SENSITIVITY</size></b>\n\nPlayers can change the mouse/pad sensitivity in-game, but you can set up the default sensitivity in <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ Default Settings ➔ Settings Values ➔ <b>Sensitivity</b> and <b>Aim Sensitivity</b>.");
    }

    void DrawObjectPooling()
    {
        DrawText("<b><size=15>What is Object Pooling?</size></b>\n\n<b>Instantiate()</b> and <b>Destroy()</b> are useful and necessary methods during gameplay. Each generally requires minimal CPU time.\n\nHowever, for objects created during gameplay that have a short lifespan and get destroyed in vast numbers per second like Bullets per example, the CPU needs to allocate considerably more time.\n\nThere is when Object Pooling is enter, <b>Object pooling</b> is where you pre-instantiate all the objects you’ll need at any specific moment before gameplay, in MFPS bullets, decals and hit particles are pooled.\n\nThe bl_ObjectPooling.cs class is really easy to use, all what you need to do to add a new object to pooled is listed the prefab in the <b>'pooledPrefabs'</b> list of bl_ObjectPooling inspector which is attached in the <b>GameManager</b> object in the map scenes, once you add the prefab simply set a key name and how many instances of this prefab you think will be enough and that's.\n\nNow for instance this prefab from a script, before you normally will use something like:\n\n");
        DrawCodeText("GameObject ob = Instantiate(MyPrefab, position, rotation);");
        DrawText("with bl_ObjectPooling script you simply has to replace that with:");
        DrawCodeText("GameObject ob = bl_ObjectPooling.Instance.Instantiate(\"PrefabKey\", position, rotation);");
    }

    void AddNewMenu()
    {
        DrawText("If you want add a new menu/window in the Lobby UI, follow this:\n\n1 - Create the menu/window UI: make the design of UI with what you need, but make all the design as child of a parent under canvas, example \"<i>MyNewWindow</i>\" (this is a empty game object under canvas) put all the buttons, text, images, etc.. of your new menu/window under this object.\n\n");
        DrawAnimatedImage(0);
        DrawText("2 - Add a new field in the <b>Windows</b> list in Lobby -> Canvas [Default Menu] -> bl_LobbyUI -> Windows, in the new field in this list and add the \"<i>MyNewWindow</i>\" object in the field and assign an unique name.\n");
        DrawAnimatedImage(1);
        DrawText("3 - Create a menu button: add a new button that will open the new menu/window, all the other buttons are in: Lobby -> Canvas -> Lobby -> Content -> Top Menu -> Buttons -> *, so you can duplicate one of these buttons and change the title text.\n\nin this new button add as listener the function of bl_Lobby -> ChangeWindow(string) -> and set the name of new window in the list \"Windows\".That's.\n");
        DrawAnimatedImage(2);
    }

    void DrawAddonsDoc()
    {
        DrawText("MFPS 2.0 comes with tons of features on the main package, but still missing some important features that almost all FPS have like e.g: <i>Mini Map, Login System, Level System, Localization, Shop, etc...</i> if I tell you that <b>MFPS have all those features</b>? Yes, MFPS have <b>extensions/add-ons</b> with which you can integrate all these features.\n\n<b><size=22>So you may wonder why they are not added by default?</size></b>\n\nThe main reason is to keep a relatively low price for the main core package, if all the addons were added by default in the main core the price of the package would rise to at least $250.00 or more, so we decided to add the essential features to the core and let developers choose which extra extensions they want to integrate, take in mind that the main core is fully functional by itself and doesn't require any the add-ons, they all are optional.\n\nYou can purchase these addons and import the package in the MFPS 2.0 project, pretty much every addon comes with an Automatically integration which means that you doesn't have to made any  manual change in order to integrate them.\n\nIf you wanna check which addons are available you can open the <b>Addon Manager</b> window in MFPS -> Addons -> Addons Manager.\n");
        if(Buttons.FlowButton("Open Addons Manager"))
        {
            GetWindow<MFPSEditor.Addons.MFPSAddonsWindow>("Addons Manager");
        }
        DrawText("<b><size=22>How Integrate Addons?</size></b>\n\nAll addons comes with a <b>ReadMe.txt</b> on the root folder of the addon with the instructions, but pretty much <b>all Addons comes with an Automatically integration</b>, you only have to enable and click on the Integrate MenuItem:\n");
        DrawAnimatedImage(3);
    }

    void EditorMenusDoc()
    {
        DrawText("MFPS comes with a series of uniqueness <b>Editor windows</b> that you can found in the Unity Editor top menu under the <b>MFPS</b> root menu:");
        DrawServerImage("img-26.png");
        DrawText("Here is a brief explaination of what each one of them is for:\n \n<b><size=18>MFPS ➔ Addons ➔ *</size></b>\n \n- In the submenu of this, you will find all the <i>quick actions</i> for all the addons in your project, listed by the addon name you will find the button for quick actions like enable or disable the addon, run the auto-integration and the documentation link.\n \n<b><size=18>MFPS ➔ Addons ➔ Addons Manager</size></b>\n \n- This will open the Addons Manager window, with which you can see the register of all the MFPS 2.0 available add-ons,\nwith information as the latest update of each addon, the changelogs, last versions, addons description, enable/disable addons, addons links, and in-project addons status.\n \n<b><size=18>MFPS ➔ Tutorials</size></b>\n \n- In the submenu of this, you will find listed all the build-in editor documentation and tutorials for MFPS and all the addons in your project, being the <b>Tutorials ➔ Documentation</b> the main or general documentation of MFPS.\n \n<b><size=18>MFPS ➔ Tools</size></b>\n \n- In the submenu of this, you will find various actions and helpers windows that provide useful automated operations for MFPS and or specific addons.");
        DrawText("<b><size=18>MFPS ➔ Manager</size></b>\n \n- This will open the MFPS Manager window with which you can find the front-end settings of MFPS and all the addons in your project, also, in this window you have a range of utility windows with you can manage some of the MFPS main settings as the default player weapons load-outs, weapons information, levels, etc...\n \n<b><size=18>MFPS ➔ Store</size></b>\n \n- This will open the MFPS Store window, in this window you can find listed all the available MFPS addons with their respective informations, preview images and links to acquire them.\n \n<b><size=18>MFPS ➔ MFPS</size></b>\n \n- This will open the MFPS window, which is the same window that appears automatically when you import MFPS for the first time, this window contains useful information about your current MFPS project as the MFPS version you are using, the changelog of the version, addons information, links for tutorials and contact information.\n \n<b><size=18>MFPS ➔ MFPS News</size></b>\n \n- This will open the MFPS News window, in which as the name says, you will find news about MFPS and the add-ons,\nlatest add-ons updates, third-party assets from the Asset Store that you may find interesting for shooter projects.\n \nThis information of this window is fetched from our server so you may want to check it regularly so you don't miss anything about MFPS.");
    }

    void DrawGameTexts()
    {
        DrawText("If you want to change some text in the game or maybe just change the text grammar, most part of the text is directly assigned in the UI Text's components which are located inside the canvas objects in each scene of the game, but also, there is some text that is set/modified in runtime by code, to make easy for you to find all this text we have placed all those in a single script which is <b>bl_GameTexts.cs</b>\n \nin this script you will find all the text that assigned by code in runtime, you can modify them from that script, this facilitates the work of for example adding your own localization system.");
        DrawServerImage("img-22.png");
    }

    void GameInputDoc()
    {
        if (subStep == 0)
        {
            DrawText("A critical aspect of shooter games <b><size=8>(or all games in general)</size></b> is the user control, but this is kinda special in action games since players usually have their own way to set up the inputs with which they feel comfortable playing, MFPS by default have the \"standard\" input set up which is set in the most fps games.\n \nSince version 1.9, MFPS comes with a custom Input Manager that allows rebinding the input keys in runtime through a menu in the settings window, you can define the default inputs in the \"Input Mapped\" and let the players decide if they wanna change in-game.\n \nAlternatively, if you want to use a third-party Input Manager system, MFPS also facilitates this, all the in-game used inputs are defined in the script bl_GameInput.cs, you can change the code of the functions to point to your custom input system.\n \nYou can identify which input is for what game action by the function name <i>(Fire, Reload, Jump, etc...)</i>");
        }
        else if (subStep == 1)
        {
            DrawText("If you want change the default input mapped of the keyboard or gamepad you simple have to modify the mapped object.\n\nSelected the input mapped scriptable object, if you are using one of the default ones, they are located at: <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Presents ➔ Input Mappeds➔*</i>, select it and in the inspector you will have the list of all the setup inputs, fold out the input that you want to modify and edit the info <i>(keycode, axis name, description, etc..)</i>\n");
            DrawServerImage("img-40.png");
            DownArrow();
            DrawText("Also you can reorder the input order, the same order in this list is the order that will be displayed in game.");
        }
        else if(subStep == 2)
        {
            DrawText("Add a new input is really simple, basically you only have to add a new field in the input mapped and set the keycode of the input.\n\n1 - Select the Input Mapped in which you want to add the input, by default there're only 2: Keyboard and Xbox Controller, so select the one that you want edit, these mappeds are located at: <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Presents ➔ Input Mappeds➔*</i>\n\n2 - In the inspector of the mapped you will see the list called<b> Button Map</b> with all the current inputs, add a new field in this list and fill the info of it:");

            DrawPropertieInfo("KeyName", "string", "The custom key name of this input");
            DrawPropertieInfo("PrimaryKey", "KeyCode", "The Unity Keycode of this input");
            DrawPropertieInfo("PrimaryAxis", "string", "if this input is not a key but a axis, set the name here.");

            DownArrow();

            DrawText("Once you have configured the key, now you can use it in your code, the usage is pretty similar to the default Unity Input, instead of:\n");
            DrawCodeText("Input.GetKeyDown('keyName'){...}");
            DrawText("You have to use:");
            DrawCodeText("bl_Input.isButtonDown('keyName'){...}");
            DrawText("or");
            DrawCodeText("bl_Input.isButton('keyName'){...}\nbl_Input.isButtonUp('keyName'){...}");
            DrawText("Where the <i>'keyName'</i> value is the <b>KeyName</b> of your setup input in the Input Mapped.\n");
        }
        else if(subStep == 3)
        {
            DrawText("In order to use a GamePad/Controler with MFPS and Input Manager you have to do some extra steps.\n \n- First, if you did not override the Input Settings when install MFPS <i>(using the installer window)</i> you have to modify the Unity Input Settings to add the required control axis, Input Manager comes with a prepared <b>Input Settings.asset</b> with all this already set up, so you only have to click the button below.");

            if (!File.Exists("ProjectSettings/InputManager-backup.asset"))
            {
                if (Buttons.FlowButton("Setup Unity Input Manager"))
                {
                    string sourcePath = "Assets/MFPS/Content/Prefabs/Presents/Input Mappeds/InputManager.txt";
                    if (!File.Exists(sourcePath))
                    {
                        Debug.LogWarning("The MFPS InputSettings data couldn't be found.");
                        return;
                    }
                    string imFile = "ProjectSettings/InputManager.asset";
                    if (!File.Exists(imFile))
                    {
                        Debug.LogWarning("The InputManager data couldn't be found.");
                        return;
                    }
                    File.Move(imFile, imFile.Replace("InputManager", "InputManager-backup"));
                    File.Copy(sourcePath, "ProjectSettings/InputManager.asset");
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                GUILayout.Label("MFPS input settings has been integrated already.");
            }

            DownArrow();

            DrawSuperText("Now in the <?link=asset:Assets/MFPS/Resources/InputManager.asset>InputManager</link> in the field <b>Mapped</b> set the input mapped for your controller, by default the addon comes with the InputManager mapped for xbox controller, drag this or your created one in the <b>Mapped</b> field");
            DrawServerImage("img-41.png");
        }
        else if(subStep == 4)
        {
            DrawText("To create a new input Mapped simply go to the folder where you want to create it <i>(in the Project View window)</i> -> Right Mouse Click -> Create -> MFPS -> Input -> Input Mapped, now you will see the new created object, select it and setup all the inputs of your controller keyboard.");
            DrawServerImage("img-42.png");
            DrawText("Optionally you can just duplicate one of default mappeds and edit the inputs.");
        }
    }

    private AssetStoreAffiliate uiAssets;
    void GameUIDoc()
    {
        if(subStep == 0)
        {
            DrawSuperText("An important modification and most of the time skipped or not much time invested in is the redesign of the user interface, it's extremely important that you modify the default UI and not just the color but the actual layout and if possible the sprites and the overall design since it not only will make your game seem more unique but also will make your game not just another quick copycat game and avoid bad reviews of players because of this.\n \nThere are no special steps in MFPS to modify the UI, you can modify it as you would do in any other Unity project that uses UGUI, all the UI is structured and designed inside the Canvas's in each scene, there you can change the images, text, font, sprites, layout, etc... if you are not familiar with the Unity UI system, check this tutorial first:\n<?link=https://learn.unity.com/tutorial/ui-components#>https://learn.unity.com/tutorial/ui-components#</link>");

            DrawText("If you are already familiar with Unity's UI system then you can modify pretty much anything of the default UI, you simply have to <b>make sure to not delete components that are referenced/required by a script</b>, if you aren't sure if you can delete a UI object and you want to hide it then you can simply disable it instead.");
            DrawText("<b><size=18>Lobby UI</size></b>\n \nAll the lobby UI can be found in the <b>MainMenu</b> scene hierarchy <b>Canvas</b>, a common question I receive is:\n \n<b><size=14>Where to change the background image?</size></b>\n \nYou can change the lobby background image in the inspector of this image component attached to this object in the hierarchy:");
            DrawServerImage("img-48.png");
            DrawText("<b><size=18>Maps UI</size></b>\n \nWhen you make a modification to the UI of a map scene, you don't have to do it again for your other map scenes you just have to apply the changes to the UI prefab and it will be synced in all the other scenes.");
        }else if(subStep == 1)
        {
            DrawText("In case you are looking for UI kits to replace the default UI of the game, below you will find a hand-picked collection of assets that you can acquire from the Asset Store");
            Space(20);
            if (uiAssets == null)
            {
                uiAssets = new AssetStoreAffiliate();
                uiAssets.randomize = true;
                uiAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555480555/widget-medium");
                uiAssets.FixedHeight = 400;
            }
            else
            {
                uiAssets.OnGUI();
            }
        }
    }

    void DrawFriendListDoc()
    {
        DrawText("Photon have a <b>Friend List</b> feature which allow players to check the status of other users connected in the same server, you only have to send the UserIDs of the players that you wanna check the status, the system is pretty limited since you only can know when a player is \"Online\" or not, and only allows you join to the player room if this is in one.\n\nMFPS comes with this Photon feature added, in the Main Menu scene you can add friends and save it locally <i>(or in database if you are using ULogin Pro)</i>, once you added you'll be able to see when this player is online and also you will be able to join to the same room when the player is in one room.\n\nAdded a friend doesn't require confirmation of the other player, you simple set the exact player name and ready.\n\nIn order to add a friend simply click in the top right button (in the lobby) with the person icon:\n\n");
        DrawServerImage(10);
        DrawText("The friend list have a limit number of friends that can be added (by default is 25) this only make sense if you are saving the friends in a database (using ULogin Pro for example) since the more friends they add per player more will be the size of the player data in the database.\n\nYou can change this limit in GameData -> MaxFriendsAdded.\n");
    }

    void CrosshairDoc()
    {
        DrawText("The Crosshair or reticle is a basic feature in most shooter games, in MFPS you can easily change the shape, color, and size of the crosshair, also you can use different crosshair for each type of weapon.");
        DownArrow();
        DrawTitleText("Modify crosshair");
        DrawText("Open one of your map scenes ➔ go to <i>(in the Hierarchy window)</i> <b>UI ➔ PlayerUI ➔ Crosshair ➔ Crosshairs ➔ *</b>, there you will see all the available crosshairs setups, by default each one of them is used for different kind of weapons <i>(machineguns, shotguns, knife, etc...)</i>.\n\nSo what you have to do here is Open one of the crosshair setup/styles and apply any kind of modification that you want, you can remove or add any UI component that you want, just make sure everything is under the crosshair style root object.");
        DrawServerImage(32);
        DownArrow();
        DrawTitleText("Hit Marker");
        DrawText("The <b>hit marker</b> is a small cross that shows up when the local player hit an enemy.\n\nYou can customize the design of this on <b>UI ➔ PlayerUI ➔ Crosshair ➔ Crosshairs ➔ Hitmarker</b>.\n\nThe hit marker appears with simple scale-up animation, you can define the final size of the animation in <b>bl_UCrosshair ➔ Increase Amount</b>.");
    }

    void FootStepsDoc()
    {
        DrawText("The footstep sound in MFPS is driven by the surface Tag, there're some predefined tags like <i>Metal, Concrete, Dirt, Wood, and Water</i>, when a player moves a footstep sound will play depending on the surface where the player is over, of course, you can change these sounds or add more surface tags, also you can change the sounds per player prefab so you can have different sounds for different player models.");
        DrawTitleText("Change Sounds");
        DrawHyperlinkText("In order to change the default, MFPS footstep sounds you can simply replace the AudioClips in the default <link=asset:Assets/MFPS/Content/Prefabs/Presents/Audio/FootStepsLibrary.asset>FootStepLibrary</link>, unfold the <b>Groups</b> list ➔ unfold the tag group field ➔ replace the AudioClips in the list.");
        DrawServerImage(29);
        DownArrow();
        DrawTitleText("Add Surfaces");
        DrawHyperlinkText("If you wanna add a new surface <i>(identified by a different tag)</i> and you wanna play a specific footstep sound for it, you can do it by simply add a new field in the Groups list of the <link=asset:Assets/MFPS/Content/Prefabs/Presents/Audio/FootStepsLibrary.asset>FootStepLibrary</link>, in that new field, in the propertie <b>Tag</b> set the tag identifier for that surface.");
        DownArrow();
        DrawTitleText("Terrain Surfaces");
        DrawText("Since the footstep system is driven by <b>Tags</b> you can only set a tag per object/mesh, this comes with a problem for the <b>Unity Terrain system</b>, since the terrain is a single object/mesh, you can set only one tag for it, that is inconvenient because terrains usually have various layers with different textures that simulate different surfaces, so in order to make the footstep system work correctly with the Unity Terrain system <i>(play different sounds depending on the terrain layer)</i> you need to set up your terrain layers as follow.\n\n•  First, select the Terrain object in your scene hierarchy and add the script <color=#00E9FFFF><b>bl_TerrainSurfaces.cs</b></color>\n\n•  Once you add the script you will notice in the inspector of the script that the list <b>TerrainSurfaces</b> have various fields, each of these fields represent one of the layers <i>(textures)</i> in the <b>Terrain</b>, what you have to do is set the <b>Tag</b> name to each layer, if you fold out one of the fields you will see the texture of the layer, so depending on the texture you can set which <b>tag</b> should be assigned in the property <b>Tag</b>.");
        DrawServerImage(30);
    }

    void DoorsDoc()
    {
        DrawSuperText("MFPS includes an optimized door system that can be used to add dynamism to your maps.\n \nThe system is designed to perform well with a high amount of doors per map, of course, synchronized over the network, and really simple to use.\n \n<?title=18>HOW TO ADD DOORS?</title>\n \n- In order to add a door to your map, you only need 2 steps:\n \n<b>1. Place the door on your map:</b> Drag and drop the door prefab in your scene hierarchy and placed it in your map design, you can find the <?link=asset:Assets/MFPS/Content/Prefabs/Level/Items/Door.prefab>default door prefab</link> located in: <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Level ➔ Items ➔ Door</i>\n \n<b>2. Register the new door:</b> In the map scene hierarchy, go to ItemManager ➔ bl_DoorManager ➔ click on the button '<b>Collect all active doors in scene</b>' ➔ that's.");
        DrawServerImage("img-32.png");
        Space(20);
        DrawSuperText("<?title=18>HOW TO CREATE A NEW DOOR?</title>\n \n- If you want to create a door with your custom door model, you simply have to use the <?link=asset:Assets/MFPS/Content/Prefabs/Level/Items/Door.prefab>default MFPS door</link> prefab and replace its model.\n \n1. Instance the <?link=asset:Assets/MFPS/Content/Prefabs/Level/Items/Door.prefab>default MFPS door</link> prefab in any scene and Unpack the prefab instance <i>(right-click on the door instance ➔ Unpack Prefab Completely)</i>\n \n2. Drag your custom door model on the door instance under Door ➔ Door Model ➔ *, and manually place the new door model in the same position, rotation, and scale as the default model.\n \n3. Select the root of the door instance ➔ <b>bl_BasicDoor</b> ➔ in the <b>Door Pivot</b> field, drag the transform of your new door model that will be the point where the door will rotate around (pivot point).\n \n4. Delete the default model <i>(Door ➔ Door Model ➔ <b>Default Door Model</b>)</i> and save the door as a new prefab by dragging it in any folder of the project view.\n \nIf your door model rotation is not right, simply create a new Door Settings <i>(Project View ➔ Right Click ➔ MFPS ➔ Level ➔ Door Settings)</i> ➔ assign it to the door prefab ➔ bl_BasicDoor ➔ <b>Door Settings</b> and assign your new door model pivot rotation values.");
    }

    void GameStaffDoc()
    {
        DrawText("You may want to highlight working game development members with a badge on their behalf for example<b> Lovatto <color=#FF0000FF>[Admin]</color></b> with a different color that normal players, so other users can see that is a staff member on the game, on MFPS 2.0 there are a simple way to do this and you can set up right on the inspector.\n\nGo to <b>Game Data</b> and find the \"Game Team\" section at the bottom of the inspector:\n");
        DrawServerImage(23);
        DrawText("in this list you can add much member as you want, with a simple settings to set up:\n");
        DrawHorizontalColumn("UserName", "The name that the staff member need write to access to this account.");
        DrawHorizontalColumn("Role", "The staff rank / role on the team.");
        DrawHorizontalColumn("Password", "When try to sing in with this account name a password window will appear to write the password (with other names will not appear), so normal player can't fake identity.");
        DrawHorizontalColumn("Color", "The color with the name text will appear in the game.");
    }

    void DrawMobileDoc()
    {
        DrawText("If you are build targeting to a mobile platforms e.g: Android or iOS, keep in mind that although MFPS does work on mobile platforms, there are some things that you have to do before to build, since by default MFPS 2.0 is setup with high quality graphics for high-end devices for demonstration purposes, so by default it's not optimized for mobile platforms, also the main core package doesn't include any mobile control/input.");

        DrawHyperlinkText("So first thing you need is to integrate a mobile input control, for this, there a in-house addon solution specifically for MFPS 2.0 which is: <link=https://www.lovattostudio.com/en/shop/addons/mfps-mobile-control/>Mobile Control</link>, which contains all the necessary buttons/inputs to work in mobile/touch devices and the integration is automatically, but alternatively you can integrate your preferred third-party system if you want.");

        DrawText("Secondly, you have to made some manual optimization work, the same that you would do in any other mobile project, starting by removing the Post-Process effects <b><size=8><i>(you can do this by removing the Post Processing Stack package from the Unity Package Manager)</i></size></b>, Change the Standard Shaders to mobile friendly ones, reducing the textures quality and resolution, and of course your new levels/maps/models have to be mobile friendly, etc... <b>You don't have to do any code change</b>, MFPS code is mobile ready and optimized for low-end platforms.\n");
        DrawText("So basically, to build for mobile what you have to do is some graphic optimization, below I will leave you some useful links to tutorials that will help you with graphic optimization and things to keep in consideration:");
        DrawLinkText("https://docs.unity3d.com/2020.1/Documentation/Manual/MobileOptimizationPracticalGuide.html", true);
        DrawLinkText("https://learn.unity.com/search/?k=%5B%22tag%3A5816095d0909150016dc7b17%22%2C%22lang%3Aen%22%2C%22q%3Aoptimization%22%5D", true);
        DrawLinkText("https://cgcookie.com/articles/maximizing-your-unity-games-performance", true);
        DrawLinkText("http://www.theappguruz.com/blog/graphics-optimization-in-unity", true);

        DrawSuperText("If you are just starting with mobile development and want to save some time, there's also an <b>MFPS Mobile</b> asset which is ready to use for mobile platforms, it comes with the Mobile Input Controller already integrated and the game optimization is already done, if you are interested, check it out here: <?link=https://www.lovattostudio.com/en/shop/mobile/mfps-mobile/>MFPS Mobile</link>");
    }

    void PlayerIKDoc()
    {
        DrawText("The Third-person/Remote player upper-body poses are defined by the animations clips in the Animator Controller, but some bones of the model rig are override controlled by Inverse Kinematic aka IK, this to achieve multiple things, one of them is so that <b>you don't have to have a custom player animation for each one of the weapons</b> since normally the left arm goes in different parts depending on the weapon.\n \nSo MFPS solves this using IK in these specific bones that you can easily modify per weapon.\n \nIn order to edit the left arm position/pose of one of the TPWeapons do the following:\n \n1. In the Editor <i>(in edit mode)</i> drag one of the player prefabs in a scene <i>(preferable a clean scene so you can focus)</i>.\n \n2. Go to the Remote Weapons which are located under the right hand of your player model in the hierarchy -> select the TPWeapon to edit the pose and active it.\n \n3. With the weapon selected -> In the inspector window you will see a button called \"<b>Edit Hand Position</b>\" -> click it and move/rotate the IK target in the scene view until you get the desired position.");
        DrawAnimatedImage(5);
    }

    void PostProcessingDoc()
    {
        if (subStep == 0)
        {
            DrawText("By default MFPS use the Unity <b>Post-Processing stack v2</b>, which is a collection of effects and image filters that apply to the cameras to improve the visual of the game.\n\nAlthough the image effects really improve the visual aspect of the game, that also carries a cost in the performance of the game, that is why you should use this only for high-end platforms like PC or Consoles, you definitely <b>should NOT use for Mobile Platforms</b>, if you are targeting for a mobile platform you should delete this package.\n\nThis system is automatically imported from the Unity Package Manager (UPM) when you import MFPS for the first time in the project, if you want to <b>DELETE</b> it, you can do it clicking on this context menu:");
            DrawServerImage(11);
            DrawText("You can find the Post-Processing Stack official documentation here:");
            DrawLinkText("https://docs.unity3d.com/Packages/com.unity.postprocessing@2.3/manual/index.html", true);
        }
        else if (subStep == 1)
        {
            DrawSuperText("Since version 1.9, you can easily define custom <b>Post Processing Profiles per map scene</b>,\nPost Process Profile is the configuration file where you define the image effects that will be rendering in the Post Process Volume, if you need more details or you don't know how to create or use them, check the official documentation for it here: <?link=https://docs.unity3d.com/Packages/com.unity.postprocessing@3.1/manual/Quick-start.html>Post-Process Quick Start</link>\n\n<?title=18>DEFINE PROFILE IN MAP SCENE</title>\n\nTo use a custom PP Profile in a specific map scene: open the map scene in the Editor ➔ in the Hierarchy window, go to: GameManager ➔ Post Process Volume ➔ in the inspector window of bl_PostProcessEffects ➔ Assign your custom Post Process Profile in the \"Process Profile\" field and you are all set.");
        }
        else
        {
            DrawSuperText("There are some common problems related to the Post Processing package, that cause errors in the console due to the Post Processing package missing, here is how to handle them:\n \n<?title=18>DON'T WANT TO USE:</title>\n \nIf you don't intend to use the Post Processing package but you still receive errors due to missing references to the package, you have to remove the package script definition symbols, for it, go to Unity Player Settings ➔ Other Settings ➔ Script Define Symbols ➔ find and remove this part from the input field <?underline=><b>UNITY_POST_PROCESSING_STACK_V2;</b></underline> ➔ hit enter and wait for the compilation.\n \n \n<?title=18>WANT TO USE:</title>\n \nIf you want to use the Post Processing features, but receive errors due to missing references to the package, it means that the package hasn't been imported yet.\n \nTo import the package, go to Window ➔ Package Manager ➔ in the left panel find the <?underline=>Post Processing</underline> package ➔ Click in the Import button on the bottom right corner of the window.");
        }
    }

    private AssetStoreAffiliate fxAssets;
    void ParticlesDecalsDoc()
    {
        if (subStep == 0)
        {
            DrawHyperlinkText("<b><size=22>PARTICLES</size></b>\n\nOne important part of making a good looking game are the particle effects, although MFPS use them just in few events, they show frequently, so use good looking particles is not enough, they have to be optimized, the default particles in MFPS are very much just placeholders, and even tho its not necessary is recommended to replace with better ones.\n \nIn order to replace/modify a specific particle you simply have to modify their prefab, MFPS use <b>Particle System</b> for the following game effects:\n \n•  Muzzleflash\n•  Explosions\n•  Bullet hit\n•  <link=asset:Assets/MFPS/Content/Prefabs/Level/Particles/Prefabs/Impacts/BloodFast.prefab>Blood</link>\n•  Fire\n \n \nAside from the <b>Muzzleflash</b> particle which is located inside each FPWeapon, all the other particles can be found in or under the MFPS folder at <i>Assets -> MFPS -> Content -> Prefabs -> Level -> Particles->*</i>");
            DrawNote("When you modify a particle prefab, make sure to use the default prefab as a reference since these have attached custom scripts attached aside from the Particle System components which are necessary to the game work correctly.");
            Space(50);
            DrawText("<b><size=22>DECALS</size></b>\n \nMFPS doesn't use a custom decal system, since they are only used for the bullets marks, and since these are instanced too frequently it just adds a non-needed performance cost to the game, due to that, MFPS uses a simple Quad mesh with a simple transparent shader as decals.\n \nAs I mentioned the only decals that MFPS uses are for the <b>Bullet Marks</b>, these are instanced when a bullet hit a collider, the mark that will appear depending on the hit collider tag, by default MFPS use 5 different bullet marks that simulate the impact in different surfaces (metal, wood, sand, concrete and a generic one), you can change these marks textures by modifying the bullet impacts prefab material, these prefabs are located in <i>Assets -> MFPS -> Content -> Prefabs -> Level -> Particles -> WeaponEffects -> Prefabs->*</i>");
        }else if(subStep == 1)
        {
            DrawText("In case you are looking for particles or decals to replace the default ones in the game, below you will find a hand-picked collection of assets that you can acquire from the Asset Store");
            Space(20);
            if (fxAssets == null)
            {
                fxAssets = new AssetStoreAffiliate();
                fxAssets.randomize = true;
                fxAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555492698/widget-medium");
                fxAssets.FixedHeight = 400;
            }
            else
            {
                fxAssets.OnGUI();
            }
        }
    }

    void InGameChatDoc()
    {
        DrawText("A way of user-to-user communication in-game of course is the text chat, for sure MFPS implements that.\n \nIn-game, when players are on a map, they can open the text chat input field to start writing with the <b>Enter/Submit key input</b>, and with the same key or the button next to the input field can send the message which will appear <i>(by default)</i> on the bottom left corner of the screen, pretty basic, but that isn't all, the chat support team only chat so players can communicate only with teammates, for that they simply have to open the chate with a custom key by default <b>T key</b> and that is it.\n \nFor some or other reason you may <b>not want to allow user-to-user communication</b> in-game, if that the case you can simply disable the text chat by deleting or just disabling the Chat UI which is located in the Map scenes UI canvas at UI -> MenuUI -> <b>Chat</b>.");
        DrawHorizontalSeparator();
        Space(10);
        DrawText("If you want to show a custom text in the chat panel for all the players in the room by code,  you can do so like this:");
        DrawCodeText("bl_ChatRoom.Instance.SetChat('YOU TEXT HERE');");
    }

    void LadderDoc()
    {
        DrawText("The package includes a drag and drops ladder system that you can use to add more dynamic navigation to your maps.\n \nAll you have to do to add a new ladder to your map is drag the ladder prefab by default located in <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Level ➔ Items ➔ <b>Ladder</b></i> > drop in your map scene hierarchy > positioned it where you want and that's.");
        DrawHorizontalSeparator();
        DrawText("<b><size=16>Replace the Ladder model</size></b>\n \nIf you want to replace or add a new ladder model, all you have to do is use the default ladder prefab > delete the default model and replace it with your own.\n \n1. Drag the default ladder prefab locate at <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Level ➔ Items ➔ <b>Ladder</b></i> in any scene hierarchy.\n2. Drag your new ladder model inside the prefab instance and positioned it in the same position as the default ladder model in Ladder > Model > *.\n3. Delete the model inside the prefab instanced.\n4. Save the new prefab so you can use it in your map scenes.");
        DrawServerImage("img-46.png");
    }

    void MFPSEventsDoc()
    {
        DrawText("There're some special events that you can use if you wanna implement a custom feature or modification in your own scripts, e.g: when the local player spawn, when the local player dies, when receiving damage, etc...\n\nUse these events are really simple, all you have to do is subscribe a function from your script that will listen to the callback of these events when they are dispatched in runtime.\n\nYou subscribe to these events on <b>OnEnable()</b> and unsubscribe on <b>OnDisable()</b> functions:");
        DrawCodeText("void OnEnable()\n{\nbl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;\n}\n\nprivate void OnDisable()\n{\nbl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;\n}\n\nvoid OnLocalPlayerSpawn()\n{\n//execute your code\n}");
        DownArrow();
        DrawText("Below you will have the list of all the available events with a short description\n");
        DownArrow();

        DrawCodeText("bl_EventHandler.onLocalPlayerDeath");
        DrawText("Event called when the LOCAL player die in game");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onLocalPlayerSpawn");
        DrawText("Event called when the LOCAL player spawn");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onPickUpGun");
        DrawText("Event called when the local player pick up a weapon");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onChangeWeapon");
        DrawText("Event Called when the LOCAL player change of weapon");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onLocalAimChanged");
        DrawText("Event Called when the local player change their Aim state");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onMatchStart");
        DrawText("Event Called when the room match start");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onFall");
        DrawText("Event Called when the LOCAL player fall/land in a surface");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onPickUpHealth");
        DrawText("Event called when the LOCAL player pick up a health in game");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onAirKit");
        DrawText("Event called when the LOCAL player call an air drop");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onAmmoPickUp");
        DrawText("Event called when the LOCAL player pick up ammo in game");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onLocalKill");
        DrawText("Event called when the Local player get a kill or get killed in game.");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.OnRoundEnd");
        DrawText("Event called when a game round finish");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onPlayerLand");
        DrawText("Event called when the LOCAL player land a surface after falling");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onRemoteActorChange");
        DrawText("Event called when a player that is not the local player spawn or die");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onGameSettingsChange");
        DrawText("Event called when the local player change an in-game setting/option");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onEffectChange");
        DrawText("Event called when the local player change one or more post-process effect option in game.");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onGameSettingsChange");
        DrawText("Event called when the LOCAL player change their game settings from the settings in-game menu.");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onGamePause");
        DrawText("Event called when the LOCAL player pause or resume the game.");
        DrawHorizontalSeparator();
    }

    void UpdateMFPSDoc()
    {
        DrawText("MFPS get mayor version update approximately every 3-4 months, these updates comes with new features, fixes, and improvements, if you were already using MFPS you most likely will want to apply all those improves and fixes to your started project, unfortunately, due to the nature of the asset you can't just import the new update package over your existing project with the older version because that will override all your changes, making you lost all the work and progress that you have done until that moment.\n\nThe work required to update an old MFPS version project to a new MFPS version depends on various factors, like how much changes have you done, the MFPS version of your project compared to the new version, the number of changes in the new update, etc...\n\nBut would give you some method to update your MFPS version in different scenarios:");
        Space(10);
        DrawNote("<b><color=#FF0002FF>IMPORTANT:</color> BEFORE TRY ANY OF THE BELOW METHODS OR TRY UPDATE YOUR PROJECT IN GENERAL MAKE SURE TO CREATE A BACKUP COPY OF YOUR PROJECT </b>");
        Space(10);
        DrawTitleText("Front-end changes");
        DrawText("- If you only have made frontend changes like, replace the <i>player models, tweak properties, add maps, add weapons, change UI, etc...</i> but not backend changes (code changes), then update is simpler, you can import the new MFPS package over your project and just <b>unselect</b> some assets mentioned below:\n\nFirst, import the MFPS update package from the Asset Store or your disk.\n\nIn the <b>Unity Package Import window</b>, you can select which assets import and which not, by default, all assets are selected, you simply have to <b>unselect</b> the following to not override your changes:\n\n•  The <b>Resources</b> folder <i>(with all the prefabs inside and GameData)</i>\n•  MainMenu scene <i>(in case you changed the UI)</i>\n•  Any prefab that you change.\n\nDo import the ExampleLevel scene, since it almost sure that it will contain changes, and you can use as reference for your other maps scenes.\n");
        DrawServerImage(12);
        DownArrow();
        DrawTitleText("Back-end changes");
        DrawText("If you have made <b>code changes</b> to the MFPS to core scripts or modifications to the main prefabs like <b>GameManager, GameModes, Lobby, etc...</b> the update process is more complicated.\n\nFor start, it is not possible to import the new version on your current project since this would revert all your changes losing all the work done by you, to apply the changes of the new version you must merge the changes manually, checking the scripts and prefabricated modifications of the new version and comparing them with those of your project.\n\nThis process may take some time to be done and there're high chances that a wrong merger cause errors in your game, so I will give you a method that will facilitate and reduce the work to do in this case:\n\n•  First, create a new clean Unity project, in it, import the new MFPS version package as you will normally do <i>(importing Photon PUN as well)</i>, you will use this project as reference to see the frontend changes in scenes, prefabs, properties, etc...\n\n•  Then, you will <b>simulate</b> import the new MFPS version package in your started project <i>(the project with the old version of MFPS that you have modified)</i>, note that I said <b>simulate</b> because <b>you won't import it</b>, you only need that the Unity Package Import Windows show up so you can check the changed files, this window shows all the assets that the package contains and will be imported but also if one of the files already exist in the project, it shows if the file from the package is different with respect at the file in the project, you can differentiate hthose modify files with a \"Refresh\" icon at the right side of the file name:\n");
        DrawServerImage(26);
        DrawText("Now what you have to do is take note of the modified files, write their name in a simple text file so you can find later by their name as explained below.\n\nOnce you have all the changed file names collected, is time merge those files, unfortunatelly there's not a automated way to do this, you have to manually do this, check the changes on scripts, prefabs, etc...  but don't worry you don't have to check line by line, you can use some tools to automatically detect the changes in two files.\n\nFor check changes on scripts you can use these tools:\n");
        DrawHyperlinkText("Standalone program: <link=https://sourcegear.com/diffmerge/>Diffmerge</link>\nOnline System: <link=https://www.diffchecker.com/>Diffchecker</link>\n");
        DrawText("Both systems are really straight forward to use, you have two boxes, in one you assign the script and or code of the old file/script and in the other you assing the new file, then the program will analyze and highlight you the changed lines:\n");
        DrawServerImage(27);
        DrawText("You will assign the original file <i>(the one from your project)</i> in the left side and the file from the new MFPS version <i>(from the project that you create before)</i> in the right side.\n\nNow with the changes highlighted in the program you simple have to merge the changes in your project file <i>(the file of the old MFPS version)</i>\n\nWith the standalone program <b>Diffmerge</b> you can compare the whole MFPS folder and analyze all the files, by simple selecting the option: File -> Open Folder Diff... -> Set the path to the old version MFPS folder in the first box and set the path to the new version MFPS folder in the second box.\n");
        DownArrow();
        DrawText("Now that method works efficiently for text files like scripts, but for check the differences between prefabs and scenes you will have to do it differently, for check the difference between prefabs and scenes you will have to compare both projects, Fortunately, Unity allows you to open more than one instance of the editor, so you can open the old version project and the new MFPS version project in different editor windows and inspect the differences between the prefabs and or scenes in both projects.\n");


    }

    void ServerRegionDoc()
    {
        DrawText("Photon PUN offers <b>multiple server regions to connect</b>, MFPS implements all the available servers, and let decide the players if they what to connect to a specific region.\n \nBy default, Photon PUN selects the <b><i>Best Region</i></b> to connect from the player connection, which drives to a problem for games with not a lot of concurrent players that is that players are divided into different regions making it even harder to find games, so MFPS define a <b>Fixed Region</b>, which is the region where all players will connect by default and then if they want to, they can connect to a specific server manually in-game.\n \n<b><b><size=18>Define the Fixed Region</size></b></b>\n \nGo to the MainMenu scene -> Lobby -> Lobby -> <i>(Inspector window)</i> <b>Default Server</b> -> select the region code that will be the fixed/default region to connect.\n \n<b><b><size=18>Change Server Region in Runtime</size></b></b>\n \nPlayers can change the server region in-game with the bottom right dropdown in the lobby menu.");
        DrawServerImage("img-23.png");
    }

    void LocalNotificationsDoc()
    {
        DrawText("There are some in-game local notification in MFPS that appear after a certain event happens in game e.g: <i>after a kill, pick up an item, etc...</i> these notifications are the <b>Who-Kill-Who</b> on the top right corner, the <b>local kills notification</b> on the center of the screen and the left side notifications for \"<i>non-important</i>\" notifications.\n \nFor the Who-Kill-Who or Killfeed notifications, you can check their respective section in this tutorial for more details about it.\n \n<b><size=16>LOCAL KILL NOTIFICATIONS</size></b>\n \n- Is the notification that appears in the center of the screen after the local player terminates an enemy in-game, MFPS comes with two options for how to display these notifications:\n \n1. <b>QUEQE:</b> show one kill at the time, in case multiple kills happens in a row, one will show ➔ wait for the animation and a delay time ➔ hide ➔ show the next kill until no more kills to show.\n \n2. <b>LIST:</b> Show all the kills on demand, showing in a list, a new kill will be added and showed in the screen as it happens.\n \nBy default the <i>QUEQE</i> mode is used but you can change this in the <b>GameData ➔ Local Kills Show Mode</b>.\n\nThis system is modular and event based hence you can use your own system and stop using this by simply remove/disactive the Center Local Notifier object from the <b>UI ➔ MenuUI ➔ Local Notifications ➔ Center Local Notifier.</b>");
        Space(10);
        DrawText("<b><size=16>LEFT SIDE NOTIFICATIONS</size></b>\n \n- Normally used for non-important events e.g after pick up ammo, pick up a weapon, change weapon fire type, etc...\n\nThis notification system is also modular so you can remove or replace with you own system by simpling removing the Left Local Notifier from <i>UI ➔ MenuUI ➔ Local Notifications ➔ <b>Left Local Notifier</b></i>\n\nYou can invoke your own notifications with this code:");
        DrawCodeText("using MFPS.Runtime.UI;\n            ...\n            void ShowNotificationSample()\n            {\n                new MFPSLocalNotification(\"MY NOTIFICATION TEXT HERE\");\n            }");
    }

    void NetworkStats()
    {
        DrawText("MFPS network framework can broadcast application and lobby network statistics to clients. You can make use of this data to debug your game network. You can also brag about these statistics in your game to show how popular it is. :]\n \nThere are two types of network data that you can see on MFPS:\n \n<b><size=22>NETWORK TRANSPORT STATS</size></b>\n \nThis can be used to display some basic but useful information about the current network outgoing and incoming network packages along with the local player ping, the information is displayed in the top left corner of the screen.\n \nThis feature is turned off by default but you can easily enable it from the GameData -> Show Network Stats");
        DrawServerImage("img-25.png");
        DrawText("<b><size=22>LOBBY STATS</size></b>\n \nLobby statistics can be useful if you want to show the activity of your game. Lobby statistics are per region so you will only see the stats from the server region where you are connected.\n \nYou can get information about:\n \nNumber of live rooms\nTotal number of players joined to the lobby or joined to the lobby's rooms\n \nThis feature is enabled by default but you can turn it off in the MainMenu scene -> Lobby -> bl_Lobby -> Show Photon Statistics.");
        DrawServerImage("img-24.png");
    }

    void PlayerHitboxDoc()
    {
        if(subStep == 0)
        {
            DrawText("A hitbox is a basic shape collider that detects when something collides with the player, those colliders essentially are simple shapes like boxes, spheres, or capsules, in MFPS they can be found in each of the standard humanoids bones of the player models in player prefabs.\n \nThese hitboxes are automatically set up when you create a new player with '<b>Add Player Tutorial</b>' but there may be the case that these colliders don't shape well to the player model bones, the collider may be too big, too small, or off position, for these cases, a manually retouch is needed, you will have to manually adjust the collider bounds to the bone, you can do this by simply selecting the bone transform that contains the Collider component and adjusts the center and size properties from the inspector.\n \n<b>The goal is that each hitbox collider fits as much as possible to the model.</b>");
            DrawServerImage("img-33.png");
        } else if(subStep == 1)
        {
            DrawText("The main purpose of the player hitboxes is to <b>detect when a bullet hit the player</b>, in MFPS you can have a different damage base for each hitbox in order to cause more or less damage depending on which part of the body the bullet hit.\n \nFor this, you can use the <b>bl_HitboxManager</b> script which is attached in each player prefab and bots prefabs, in the player prefabs you can find attached the '<b>Remote</b>' child, when you select this child, in the inspector window of the <i>bl_HitboxManager</i> script, you will have different damage multipliers which you can modify to cause more or less damage in certain body parts, by default these multiplier values are applied per segment <i>(Head, Chest, Arms, and Legs)</i>:");
            DrawServerImage("img-34.png");
            DownArrow();
            DrawText("Optionally, if you want to assign a damage multiplier per hitbox instead of per segment, simply turn off the '<b>Multiply value per segment?</b>' toggle in the inspector ➔ foldout the hitbox by clicking in its name ➔ set the damage multiplier.");
            DrawServerImage("img-35.png");
        }
    }

    void PlayerDamageDoc()
    {
        if(subStep == 0)
        {
            DrawSuperText("Players by default can receive damage from weapons, fall damage, and vehicle collisions, this damage is received by the player hitboxes or in the bl_PlayerHealthManagerBase script.\n \nIf you want to deal damage to a player here is how you can do it:\n \n<?background=#D1D1D1FF><b><size=16>Deal damage to a local player</size></b></background>\n \n- Dealing damage to a local player can be done by calling the <b>DoDamage(...)</b> function of the <b>bl_PlayerHealthManagerBase</b> script which is attached at the root of the player instance\n \nThat function expects <b>DamageData</b> parameter which contains all the information regarding the damage that is given.\n \nWhat you need is a reference to the player that you want to apply the damage, how you get that player reference depends on how your damage is executed, e.g let's say you do it with a Raycast, then if your Raycast detects a local player > you get the <i>bl_PlayerHealthManagerBase</i> reference from that player > call the <b>DoDamage(...)</b>and pass the <b>DamageData</b>, e.g:");
            DrawCodeText("void DealDamageFunction()\n    {\n        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit raycast, 10))\n        {\n            if (raycast.collider.isLocalPlayerCollider())\n            {\n                var playerReferences = raycast.transform.GetComponent<bl_PlayerReferences>();\n \n                DamageData damageData = new DamageData()\n                {\n                    Damage = 20, // base damage to apply\n                    Direction = transform.position,\n                    Cause = DamageCause.Player,\n                    MFPSActor = bl_MFPS.LocalPlayer.MFPSActor, // MFPS actor that cause this damage\n                    // Check the other DamageData properties\n                };\n \n                playerReferences.playerHealthManager.DoDamage(damageData);\n            }\n        }\n    }");
            DrawText("And that will do it, the example above will deal 20 damage to the target player.\n \nIf you just want to instantly kill the local player then you can just call the <b>Suicide()</b> function in <b>bl_MFPS.LocalPlayer</b> like this:");
            DrawCodeText("bl_MFPS.LocalPlayer.Suicide();");
            DrawHorizontalSeparator();
            DrawSuperText("Now, the above will only work for the local player, if you want to deal damage to a remote player <i>(a player not controlled by the local client)</i> this is how you can do it\n\n<?background=#D1D1D1FF><b><size=16>Deal damage to a remote player</size></b></background>");
            DrawSuperText("You can only deal damage to a remote player through a hitbox reference of the player, these hitboxes are the colliders that wrap the player model, these have attached the <b>bl_HitBoxBase</b> inherited script which contains the function <b>ReceiveDamage(...)</b> which is what you have to call to give damage.\n \nWhat you need is a reference to the hitbox of the remote player that you want to apply the damage, how you get that reference depends on how you want to apply the damage, let's say you what to apply the damage by an explosion for which you detect the colliders in a certain radius of the explosion origin, this is how you can implement the damage function:");
            DrawCodeText("void DealDamageFunction()\n    {\n        Collider[] hittedColliders = Physics.OverlapSphere(transform.position, 10);\n        foreach (Collider collider in hittedColliders)\n        {\n            // if you want to apply the damage only to players\n            // if (!collider.CompareTag(bl_MFPS.HITBOX_TAG)) continue;\n \n            var damageable = collider.GetComponent<IMFPSDamageable>();\n            if (damageable == null) continue;\n \n            DamageData damageData = new DamageData()\n            {\n                Damage = 50,\n                Direction = transform.position,\n                MFPSActor = bl_MFPS.LocalPlayer.MFPSActor,\n                Cause = DamageCause.Explosion\n                // see the other DamageData properties that you can use.\n            };\n \n            // send the damage to the hit box.\n            damageable.ReceiveDamage(damageData);\n        }\n    }");
        }
        else if(subStep == 1)
        {
            DrawSuperText("<b><?background=#D1D1D1FF><b><size=16>Deal damage to objects</size></b></background></b>\n \nIf you want to apply damage to a game object other than a player or bot you can do it by simply implementing the <b>IMFPSDamageable</b> interface in your custom script.\n \nLet's say you have a barrel that you want to apply damage when a bullet hit it, you don't have to made any change to the MFPS Core scripts, you simply have to implement the <b>IMFPSDamageable</b> interface and override the <b>ReceiveDamage(...)</b> function in your custom script, e.g:\n \nYour custom script could look like this by default:");
            DrawCodeText("public class bl_Test : MonoBehaviour\n{\n    public int Health = 100;\n \n    public void ReduceHealth(int damage)\n    {\n        Health -= damage;\n \n        if(Health <= 0)\n        {\n            // Destroy or wherever happens when run out of health\n        }\n    }\n}");
            DrawText("You have to implement the <b>IMFPSDamageable</b> interface and override the interface function <b>ReceiveDamage()</b> like this:");
            DrawCodeText("public class bl_Test : MonoBehaviour, IMFPSDamageable\n{\n\n    public int Health = 100;\n \n    void IMFPSDamageable.ReceiveDamage(DamageData damageData)\n    {\n        ReduceHealth(damageData.Damage);\n    }\n \n    public void ReduceHealth(int damage)\n    {\n        Health -= damage;\n \n        if(Health <= 0)\n        {\n            // Destroy or wherever happens when run out of health\n        }\n    }\n}");
            DrawText("And that will do it, keep in mind that your object must have a collider in order to be hit by the bullets.");
        }
    }

    void CommonQADoc()
    {
        DrawSpoilerBox("Bots walk trough walls and objects", "You have to bake the <b>Navmesh</b> in your map scenes in order to let the bots know where they can navigate in your map.\n\nIf you don't know what Navmesh is or how to bake it, check this: <link=https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html>https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html</link>");

        DrawSpoilerBox("Map objects randomly disappear in certain areas.", "This is caused you are using <b>Occlusion Culling</b> and you haven't Bake it in your map scene, for more info about Occlusion Culling, check this: <link=https://docs.unity3d.com/Manual/OcclusionCulling.html>https://docs.unity3d.com/Manual/OcclusionCulling.html</link>\n\nHow to bake it, check this:\n<link=https://docs.unity3d.com/Manual/occlusion-culling-getting-started.html>https://docs.unity3d.com/Manual/occlusion-culling-getting-started.html</link>");

        DrawSpoilerBox("Is there a max limit of players per room?", "There's not a fixed number of players that can join in the same room at the same time, but since each player add an extra stress to the server and consume resources for the local client device after certain amount of players the game will start to feel 'Laggy' both in refresh rate (FPS) as in the network latency (Ping).\n\nThe number of players before the game start experimenting this performance issue depend on various factors like the runtime platform, device specs, network connection, etc...\n\nWe have done our own tests with the default MFPS (1.5) to have some benchmarks, these are the result:\n\nFor <b>PC</b> with these specs:\n<i>Intel i7 3.70GHz\n16Gb Ram\nNvidia GTX 1070</i>\n\n18 players in the same room\nMedium graphic quality\nRun with a average of 60-80 FPS\n\nFor <b>Mobile</b>:\nusing a Samsung S8 Plus\n\n12 Players in the same room\nWith MFPS optimized for mobile <i>(not the default scenes)</i>\nRun with a average of 60-75 FPS\n\nYou can use these statistics as a reference but keep in mind that there are many factors which can influence the result, so it is recommended that you do your own tests.");

        DrawSpoilerBox("Alternative network solution than Photon?", "Since MFPS comes with full source code you can make any change that you want, that includes integrate other network solution.\n\nBut by default this is not possible from a front-end option <i>(like a toggle to switch between libraries)</i>,\nintegration other network library will require a lot of code changes to switch from the Photon syntax to your network sdk syntax, and there is the possibility that some methods or features of Photon do not exist or are done differently in your network library.\n");

        DrawSpoilerBox("Can I host my own dedicated server?", "Yes, Photon offers a solution for host your own server using their <b>Photon OnPremise</b> <i>(a.k.a Photon Server)</i> switch to this from the default Photon PUN (Cloud) doesn't require code changes, all that you have to do is setup the Photon Server SDK and set the IP in your PhotonServerSettings.\n\nInfo: <link=https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-in-5min>Photon Server Information</link>\n");

        DrawSpoilerBox("Rooms doesn't show up for other players?", "Sometimes when you testing your game with other players you may find out that the rooms that you or one of the testers created doesn't appear in the Room/Server list, the most common cause of this is that you and your tester <b>are not in the same server region</b>.\n\nIn the MainMenu/Lobby scene, in the bottom right corner you will see a drop-down with a region name, you can use it to select and change of server, so make sure you and your testers are connected to the same server region.");
    }

    void KnownIssuesDoc()
    {
        DrawText("On this page, we will list known issues with MFPS. The focus here is on issues that we can't fix or workaround at the moment but not just bugs or errors, but also <b>problems that affect the development workflow with MFPS</b>, the answers you gonna get here are more personal than professional <b>from the main developer Lovatto</b>.");
        DownArrow();

        DrawText("<b><size=22>ABSTRACTION</size></b>\n \nFor experienced programmers, one of the things that probably will quickly notice after look at the MFPS source code is their level of abstraction in most of the classes, which is basically none, this is especially a problem when the goal is to implement or modifying features to the game since the only way to do it is modifying the original scripts which derive to another problem = you no longer able to update MFPS to new versions since these will override the changed you did.\n \nSo, why is that? why not implement a cleaner code that can be maintainable, scalable, and easy to inherence?\nWell the answer requires some context but I'll try to explain it shortly:\n \n- MFPS originally was not created as a game template to be distributed for other developers, MFPS was a personal game, it wasn't even a game, it was a learning project, so the goal was to get things done and then worry about nicer code, wasn't until MFPS was released as a game template back in 2018 that the core rebuilding start and still continues, with each new update MFPS is improved not just with new features but also the features that already exist improved with more cleaner code that allows abstraction to make the template more modular and easier to modify for programmers.\n \nBut of course, the work is not done yet, I know that but MFPS has so many features, and in order to have better implementations of the current features or at least the same but with more inference able code is hard work, and pretty much, I'm the only person working on the code, so it will take some time to rewrite everything and even more when you take in account that I don't work full time in MFPS.");
        Space(10);
        DrawText("<b><size=22>URP, HDRP, TMP, INPUT SYSTEM SUPPORT</size></b>\n \nUnity is constantly improved and we try to use their latest technologies as possible, Unity release their new improved features that in the future will replace their current ones like the new Render Pipelines URP and HDRP, Text Mesh Pro, New Input System, etc...\n \nBut pretty much these features left the Preview/Beta phase mid last year <i><size=8><color=#76767694>(2020)</color></size></i>, before that they were not recommended for production, due to that, they were not implemented in MFPS, now that they are production-ready is the time to start using them, but of course, require some work, and due I was already working on MFPS 3.0 I started implementing these new features in it <i><size=8><color=#76767694>(MFPS 3.0)</color></size></i>, but I'm here to talk about MFPS 2.0, will these features implement in MFPS 2.0 as well? well, I won't promise all of them, but Text Mesh Pro and the New Input System support are planned to be added in a future update.\n \nWhat about URP and HDRP?\nWell, MFPS 2.0 already supports them, just not by default, you have to convert the project manually, check the Universal RP section in this documentation for more info.");
        Space(10);
        DrawText("<b><size=22>SUPPORT BOTS IN ALL GAME MODES</size></b>\n \nAnother frequent question from MFPS users is <b>why there's not support for the bots in all the game modes?</b>\n \nThe answer may disappoint you, it's simply because is too much work,\nyep :/, Artificial Intelligence is a tricky area and even more when you add multiplayer to it, even for dummy bots like the MFPS ones is a hard time developing, however, MFPS include a multiplayer AI Shooter system in the core package with support for two of the most played game modes, the problem is that all the other game modes require a custom behave of the bots to play with the game mode rules, there's when things get complicated and require a lot of work, but that puts me in a dilemma, of course as you may understand in order to keep developing and working in MFPS I need to get an income from it that is reasonable and according to the work and time that I putting on it, well, as I mentioned before AI development is one of the areas that I personally find more frustrating and that more time requires to see some progress so in order to pay off the work that I have to put it on I will have to sell it <i><size=8><color=#76767694>(the AI support for other game modes)</color></size></i> as an Addon,\nbut I rather prefer to work in other features at the time that I consider more suitable.\n \nDoes this mean there will be not supported for the bots in the other game modes?\nNo, it just means I don't have planned yet.");
        Space(10);
        DrawText("<b><size=22>GRAMMAR</size></b>\n \nMay you excuse me? D:\nis evident by seeing the documentation, code, comments, etc... that English grammar is not my strength... how about that,\nwell, this is simply explained because I'm not a native English speaker BUT that's not an excuse because most of people is not but doesn't have that many mistakes, I know that but as you may already know <i>(or more likely not)</i> I'm from El Salvador, a really tiny country that you won't notice unless zooming in on Google map in Central America, here my friend, are not too many places where you can improve other languages skills, but I try my best so please don hate me :[\n \nIn meantime feel free to give me your feedback for any grammar error you saw, I'll highly appreciate it!");
    }

    [MenuItem("MFPS/Tutorials/Documentation", false, 111)]
    private static void ShowWindowMFPS()
    {
        EditorWindow.GetWindow(typeof(MFPSGeneralDoc));
    }
}