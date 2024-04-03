using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UGUIMiniMap
{
    [RequireComponent(typeof(Camera)), ExecuteInEditMode]
    public class bl_MiniMapScreenShot : MonoBehaviour
    {
        /// <summary>
        /// The applied MSAA, possible values are 1,2,4 nad 8
        /// </summary>
        public int msaa = 1;
        public string[] Resolutions = new string[] { "4096", "2048", "1024", "512", "256" };
        public int CurrentResolution = 1;
        public bool backgroundTransparent = true;
        public bool previewBorders = true;
        public bool blackBorders = true;

        private static string _folderPath = "/UGUIMiniMap/Content/Art/SnapShots/";
        public static string FolderPath { get { return bl_MiniMapScreenShot._folderPath; } }
        public bl_MiniMap miniMap;

        private string SnapshotName(int width, int height)
        {
            string levelName = SceneManager.GetActiveScene().name;

            //if in editor, we have to get the name through editor
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                string[] path = SceneManager.GetActiveScene().path.Split(char.Parse("/"));
                string[] fileName = path[path.Length - 1].Split(char.Parse("."));
                levelName = fileName[0];
            }
#endif

            return string.Format("MiniMap-{0}-{1}x{2}.png", levelName, width, height);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnGUI()
        {
            if (previewBorders)
            {
                if (blackBorders) GUI.color = Color.black;
                Rect ScreenShotRect = new Rect(0, 0, Screen.height, Screen.height);
                Vector2 Center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                Vector2 HalfSize = new Vector2(ScreenShotRect.height * 0.5f, ScreenShotRect.height * 0.5f);

                GUI.DrawTexture(new Rect(Center.x - HalfSize.x, Center.y - HalfSize.y, 2, ScreenShotRect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUI.DrawTexture(new Rect(Center.x + HalfSize.x, Center.y - HalfSize.y, 2, ScreenShotRect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUI.DrawTexture(new Rect(Center.x - HalfSize.x, (Center.y - HalfSize.y), ScreenShotRect.width, 2), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUI.DrawTexture(new Rect(Center.x - HalfSize.x, (Center.y + HalfSize.y), ScreenShotRect.width + 2, 2), Texture2D.whiteTexture, ScaleMode.StretchToFill);

                GUI.color = blackBorders ? new Color(0, 0, 0, 0.3f) : new Color(1, 1, 1, 0.3f);
                GUI.DrawTexture(new Rect(Center.x - 1, Center.y - HalfSize.y, 2, ScreenShotRect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUI.DrawTexture(new Rect(Center.x - HalfSize.x, Center.y - 1, ScreenShotRect.width, 2), Texture2D.whiteTexture, ScaleMode.StretchToFill);

                GUI.color = Color.white;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetFullFolderPath()
        {
            return Application.dataPath + _folderPath;
        }

        /// <summary>
        /// Takes a map snapshot and saves it
        /// </summary>
        public void TakeSnapshot()
        {
            //TODO fix 
#if UNITY_EDITOR && !UNITY_WEBPLAYER
            Vector2 res = GetResolution();
            int w = Mathf.FloorToInt(res.x);
            int h = Mathf.FloorToInt(res.y);

            string path = EditorUtility.SaveFolderPanel("Save Screen Shot", "Assets/UGUIMiniMap/Content/Art/SnapShots/", "");
            if (string.IsNullOrEmpty(path)) return;

            //setup rendertexture
            RenderTexture rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = msaa;
            rt.filterMode = FilterMode.Trilinear;
            GetComponent<Camera>().targetTexture = rt;

            var format = backgroundTransparent ? TextureFormat.ARGB32 : TextureFormat.RGB24;
            //render the texture
            Texture2D snapshot = new Texture2D(w, h, format, false);
            GetComponent<Camera>().Render();
            RenderTexture.active = rt;
            snapshot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            GetComponent<Camera>().targetTexture = null;
            RenderTexture.active = null;
            snapshot.alphaIsTransparency = true;
            byte[] bytes = snapshot.EncodeToPNG();
            DestroyImmediate(rt);
            DestroyImmediate(snapshot);

            path += "/" + SnapshotName(w, h);
            path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);
            System.IO.File.WriteAllBytes(path, bytes);
            Debug.Log(string.Format("Saved snapshot to: {0}", path), this);
            AssetDatabase.Refresh();
            string relativepath = "Assets" + path.Substring(Application.dataPath.Length);

            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Texture>(relativepath));
#endif
        }

        Vector2 GetResolution()
        {
            switch (CurrentResolution)
            {
                case 0:
                    return new Vector2(4096, 4096);
                case 1:
                default:
                    return new Vector2(2048, 2048);
                case 2:
                    return new Vector2(1024, 1024);
                case 3:
                    return new Vector2(512, 512);
                case 4:
                    return new Vector2(256, 256);
            }
        }

        public void SetMiniMap(bl_MiniMap mm)
        {
            miniMap = mm;
            CenterBounds();
        }

        public void CenterBounds()
        {
            Vector3 v = transform.position;
            v.x = miniMap.WorldSpace.position.x;
            v.z = miniMap.WorldSpace.position.z;
            transform.position = v;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(bl_MiniMapScreenShot))]
    public class bl_MiniMapScreenShotEditor : Editor
    {
        bl_MiniMapScreenShot script;
        Camera m_Camera;

        private void OnEnable()
        {
            script = (bl_MiniMapScreenShot)target;
            m_Camera = script.GetComponent<Camera>();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("Settings", "box");
            script.CurrentResolution = EditorGUILayout.Popup("Resolution", script.CurrentResolution, script.Resolutions);
            script.msaa = EditorGUILayout.IntSlider("MSAA", script.msaa, 1, 4);
            m_Camera.orthographicSize = EditorGUILayout.Slider("Height", m_Camera.orthographicSize, 1f, 1000f);
            script.backgroundTransparent = EditorGUILayout.ToggleLeft("Transparent Background", script.backgroundTransparent);
            script.previewBorders = EditorGUILayout.ToggleLeft("Preview Borders", script.previewBorders);
            if (script.previewBorders)
            {
                script.blackBorders = EditorGUILayout.ToggleLeft("Black Borders", script.blackBorders);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Center Bounds"))
            {
                script.CenterBounds();
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Take Screen Shot", GUILayout.Height(40)))
            {
                script.TakeSnapshot();
            }
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
    }
#endif
}