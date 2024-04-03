using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using MFPSEditor.Addons;

public class MiniMapIntegration : AddonIntegrationWizard
{

    private List<PrefabList> miniMapsPrefabs = new List<PrefabList>()
    {
        new PrefabList(){ Name = "Square", Path = "Assets/Addons/UGUIMiniMap/Content/Prefabs/MiniMap2D [Square].prefab"},
         new PrefabList(){ Name = "Circle", Path = "Assets/Addons/UGUIMiniMap/Content/Prefabs/MiniMap2D [Circle].prefab"},
          new PrefabList(){ Name = "3D", Path = "Assets/Addons/UGUIMiniMap/Content/Prefabs/MiniMap3D.prefab"},
           new PrefabList(){ Name = "Global", Path = "Assets/Addons/UGUIMiniMap/Content/Prefabs/MiniMap2D [Global].prefab"},
    };

    /// <summary>
    /// 
    /// </summary>
    public override void OnEnable()
    {
        base.OnEnable();
        addonName = "Mini Map";
        addonKey = "UMM";
        allSteps = 2;

        MFPSAddonsInfo addonInfo = MFPSAddonsData.Instance.Addons.Find(x => x.KeyName == addonKey);
        Dictionary<string, string> info = new Dictionary<string, string>();
        if(addonInfo != null) { info = addonInfo.GetInfoInDictionary(); }
        Initializate(info);
    }

    public override void DrawWindow(int stepID)
    {
        if (stepID == 1)
        {
            DrawText("First step to integrate the MiniMap system is <b>setup all the players and bots prefabs of your game</b>, for it you simple have to click in the button below and All players that are used <i>(include the ones from Player Selector)</i> will be automatically set up.\n");
            if (DrawButton("Setup Players"))
            {
                EditorApplication.ExecuteMenuItem("MFPS/Addons/Minimap/Setup Players");
                NextStep();
            }
        }
        else if (stepID == 2)
        {
            DrawText("Now, you have to instance one of the MiniMap Prefabs in your map scene <i>(you have to do this in each map scene that you have)</i>, for it simple open the map scene and click in one of the buttons below.");

            if (!ExistInstanceScriptOf<bl_MiniMap>())
            {
                int result = DrawPrefabList(miniMapsPrefabs);
                if (result != -1)
                {
                    InstancePrefab(miniMapsPrefabs[result].Path);
                    Finish();
                }
            }
            else
            {
                DrawText("<i>Seems like a <b>MiniMap</b> has been integrated in this scene already.</i>");
            }
        }
    }

    [MenuItem("MFPS/Addons/Minimap/Integrate")]
    static void Open()
    {
        GetWindow<MiniMapIntegration>();
    }
}