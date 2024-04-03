using UnityEngine;
using UnityEditor;

public static class MFPSEditorActions
{
 
    [MenuItem("MFPS/Actions/Reset default server")]
    static void ResetDefaultServer()
    {
        PlayerPrefs.DeleteKey(PropertiesKeys.GetUniqueKey("preferredregion"));
    }

    [MenuItem("MFPS/Actions/Delete Player Prefs")]
    static void DeleteAllPlayerPrefs()
    {
        if(EditorUtility.DisplayDialog("Delete Prefs", "Are you sure to delete all the PlayerPrefs?", "Yes", "Cancel"))
        PlayerPrefs.DeleteAll();
    }
}