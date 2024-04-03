using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using UnityEngine.UI;

public class MiniMapInitializer : MonoBehaviour
{
    private const string DEFINE_KEY = "UMM";

    [MenuItem("MFPS/Addons/Minimap/Setup Players")]
    private static void Instegrate()
    {
        bl_MiniMapItem mmi = bl_MiniMapData.Instance.PlayerSetupTemplate;
        GameObject p1 = bl_GameData.Instance.Player1.gameObject;
        MiniMapDocumentation.CreateLayer("MiniMap");
        int layerID = LayerMask.NameToLayer("MiniMap");       
        if (p1.GetComponent<bl_MiniMapItem>() == null)
        {
            bl_MiniMapItem mi = p1.AddComponent<bl_MiniMapItem>();
            mi.Target = p1.transform;
            mi.Icon = mmi.Icon;
            mi.DeathIcon = mmi.DeathIcon;
            mi.IconColor = mmi.IconColor;
            mi.Size = mmi.Size;
            mi.OffScreenSize = mmi.OffScreenSize;
            mi.m_IconType = bl_MiniMapItem.IconType.Player;
            mi.isInteractable = false;
            mi.OffScreen = true;
            mi.DestroyWithObject = true;
            var ps = p1.GetComponent<bl_PlayerReferences>();
            int cm = ps.playerCamera.cullingMask;
            cm = cm & ~(1 << layerID);
            ps.playerCamera.cullingMask = cm;
            EditorUtility.SetDirty(p1);
        }
        p1 = bl_GameData.Instance.Player2.gameObject;
        if (p1.GetComponent<bl_MiniMapItem>() == null)
        {
            bl_MiniMapItem mi = p1.AddComponent<bl_MiniMapItem>();
            mi.Target = p1.transform;
            mi.Icon = mmi.Icon;
            mi.DeathIcon = mmi.DeathIcon;
            mi.IconColor = mmi.IconColor;
            mi.Size = mmi.Size;
            mi.OffScreenSize = mmi.OffScreenSize;
            mi.m_IconType = bl_MiniMapItem.IconType.Player;
            mi.isInteractable = false;
            mi.OffScreen = true;
            mi.DestroyWithObject = true;
            var ps = p1.GetComponent<bl_PlayerReferences>();
            int cm = ps.playerCamera.cullingMask;
            cm = cm & ~(1 << layerID);
            ps.playerCamera.cullingMask = cm;
            EditorUtility.SetDirty(p1);
        }
        p1 = bl_GameData.Instance.BotTeam1.gameObject;
        if (p1.GetComponent<bl_MiniMapItem>() == null)
        {
            bl_MiniMapItem mi = p1.AddComponent<bl_MiniMapItem>();
            mi.Target = p1.transform;
            mi.Icon = mmi.Icon;
            mi.DeathIcon = mmi.DeathIcon;
            mi.IconColor = mmi.IconColor;
            mi.Size = mmi.Size;
            mi.OffScreenSize = mmi.OffScreenSize;
            mi.m_IconType = bl_MiniMapItem.IconType.Bot;
            mi.isInteractable = false;
            mi.OffScreen = true;
            mi.DestroyWithObject = true;
            EditorUtility.SetDirty(p1);
        }
        p1 = bl_GameData.Instance.BotTeam2.gameObject;
        if (p1.GetComponent<bl_MiniMapItem>() == null)
        {
            bl_MiniMapItem mi = p1.AddComponent<bl_MiniMapItem>();
            mi.Target = p1.transform;
            mi.Icon = mmi.Icon;
            mi.DeathIcon = mmi.DeathIcon;
            mi.IconColor = mmi.IconColor;
            mi.Size = mmi.Size;
            mi.OffScreenSize = mmi.OffScreenSize;
            mi.m_IconType = bl_MiniMapItem.IconType.Bot;
            mi.isInteractable = false;
            mi.OffScreen = true;
            mi.DestroyWithObject = true;
            EditorUtility.SetDirty(p1);
        }

#if PSELECTOR
        foreach(var p in bl_PlayerSelector.Data.AllPlayers)
        {
            if (p.Prefab == null ) continue;
            p1 = p.Prefab.gameObject;
            if (p1.GetComponent<bl_MiniMapItem>() != null) continue;

            bl_MiniMapItem mi = p1.AddComponent<bl_MiniMapItem>();
            mi.Target = p1.transform;
            mi.Icon = mmi.Icon;
            mi.DeathIcon = mmi.DeathIcon;
            mi.IconColor = mmi.IconColor;
            mi.Size = mmi.Size;
            mi.OffScreenSize = mmi.OffScreenSize;
            mi.m_IconType = bl_MiniMapItem.IconType.Player;
            mi.isInteractable = false;
            mi.OffScreen = true;
            mi.DestroyWithObject = true;
            var ps = p1.GetComponent<bl_PlayerReferences>();
            int cm = ps.playerCamera.cullingMask;
            cm = cm & ~(1 << layerID);
            ps.playerCamera.cullingMask = cm;
            EditorUtility.SetDirty(p1);
        }
#endif

        Debug.Log("Players setup for minimap correctly!");
    }

#if !UMM
    [MenuItem("MFPS/Addons/Minimap/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#endif

#if UMM
    [MenuItem("MFPS/Addons/Minimap/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif
}