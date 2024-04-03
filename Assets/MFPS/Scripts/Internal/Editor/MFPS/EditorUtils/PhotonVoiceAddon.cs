using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using MFPSEditor;
#if !UNITY_WEBGL && PVOICE
using Photon.Voice.Unity;
using Photon.Voice.PUN;
#endif

public class PhotonVoiceAddon : MonoBehaviour
{

    private const string DEFINE_KEY = "PVOICE";

#if !PVOICE
    [MenuItem("MFPS/Addons/Voice/Enable")]
    private static void Enable()
    {
        bl_GameData.Instance.UseVoiceChat = true;
        EditorUtility.SetDirty(bl_GameData.Instance);
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#endif

#if PVOICE
    [MenuItem("MFPS/Addons/Voice/Disable")]
    private static void Disable()
    {
        bl_GameData.Instance.UseVoiceChat = false;
        EditorUtility.SetDirty(bl_GameData.Instance);
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif

    [MenuItem("MFPS/Addons/Voice/Integrate")]
    private static void Instegrate()
    {

#if PVOICE
        //setup the player 1
        SetUpPlayerPrefab(bl_GameData.Instance.Player1.gameObject);
        SetUpPlayerPrefab(bl_GameData.Instance.Player2.gameObject);


#if PSELECTOR
        foreach(var p in MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.AllPlayers)
        {
            if (p.Prefab == null ) continue;
         SetUpPlayerPrefab(p.Prefab.gameObject);
        }
#endif

        if (AssetDatabase.IsValidFolder("Assets/MFPS/Scenes"))
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            string path = "Assets/MFPS/Scenes/MainMenu.unity";
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            bl_Lobby lb = FindObjectOfType<bl_Lobby>();
            if (lb != null)
            {
                GameObject old = GameObject.Find("PhotonVoice");
                if(old != null)
                {
                    DestroyImmediate(old);
                    Debug.Log("Remove old setup");
                }
                if (FindObjectOfType<PunVoiceClient>() == null)
                {
                    GameObject nobj = new GameObject("PhotonVoice");
                    var pvs = nobj.AddComponent<PunVoiceClient>();
                    pvs.AutoConnectAndJoin = true;
                    pvs.AutoLeaveAndDisconnect = true;
                    pvs.ApplyDontDestroyOnLoad = true;
                    pvs.KeepAliveInBackground = 5000;
                    pvs.ApplyDontDestroyOnLoad = true;

                    Recorder r = nobj.AddComponent<Recorder>();
                    r.MicrophoneType = Recorder.MicType.Unity;
                    r.TransmitEnabled = true;
                    r.VoiceDetection = true;
                    r.DebugEchoMode = false;

                    pvs.PrimaryRecorder = r;
                    nobj.AddComponent<bl_PhotonAudioDisabler>().isGlobal = true;
                    EditorUtility.SetDirty(nobj);
                    EditorUtility.SetDirty(pvs);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
                Debug.Log("Photon Voice Integrated, enable it on GameData.");
            }
            else
            {
                Debug.Log("Can't found Menu scene.");
            }
        }
        else
        {
            Debug.LogWarning("Can't complete the integration of the addons because MFPS folder structure has been change, please do the manual integration.");
        }
#else
        Debug.LogWarning("Enable Photon Voice addon before integrate.");
#endif
    }

#if PVOICE
    static void SetUpPlayerPrefab(GameObject prefab)
    {
        if (prefab == null) return;

        GameObject p1 = prefab;
        PhotonVoiceView pvv = p1.GetComponent<PhotonVoiceView>();
        if (pvv == null)
        {
            pvv = p1.AddComponent<PhotonVoiceView>();
        }
        Speaker speaker = p1.GetComponent<Speaker>();
        if (speaker == null)
        {
            speaker = p1.AddComponent<Speaker>();
        }
        EditorUtility.SetDirty(p1);
    }
#endif

    [MenuItem("MFPS/Addons/Voice/Package")]
    private static void OpenPackagePage()
    {
        AssetStore.Open("content/130518");
    }

    [MenuItem("MFPS/Tools/Fix Define Symbols")]
    private static void FixDefineSymbols()
    {
        bool defines = EditorUtils.CompilerIsDefine("LM");
        if (!defines)
        {
            EditorUtils.SetEnabled(DEFINE_KEY, false);
        }
    }

#if UNITY_POST_PROCESSING_STACK_V2
    [MenuItem("MFPS/Tools/Delete Post-Processing")]
    private static void DeletePP()
    {
        UnityEditor.PackageManager.Client.Remove("com.unity.postprocessing");
        EditorUtils.SetEnabled("UNITY_POST_PROCESSING_STACK_V2", false);
    }
#endif

}