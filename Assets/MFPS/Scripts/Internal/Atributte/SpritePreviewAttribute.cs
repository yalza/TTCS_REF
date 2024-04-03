using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPSEditor
{
    public class SpritePreviewAttribute : PropertyAttribute
    {
        public float Height { get; set; } = 0;
        public float Width{ get; set; } = -1;
        public bool AutoScale { get; set; } = false;
        public SpritePreviewAttribute()
        {
        }

        public SpritePreviewAttribute(float height, bool autoScale = false)
        {
            Height = height;
            AutoScale = autoScale;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SpritePreviewAttribute))]
    public class SpritePreviewAttributeDrawer : PropertyDrawer
    {
        SpritePreviewAttribute script { get { return ((SpritePreviewAttribute)attribute); } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(position, property);
            }
            else
            {
                Sprite spr = property.objectReferenceValue as Sprite;
                Texture2D icon = null;
                if (spr == null)
                {
                    icon = property.objectReferenceValue as Texture2D;
                }
                else icon = spr.texture;

                Rect imgp = position;

                float height = script.Height <= 0 ? EditorGUIUtility.singleLineHeight * 2 : script.Height;
                float width = script.Width <= -1 ? imgp.height : script.Width;

                imgp.height = height;
                imgp.width = width;
                imgp.x += 20;
                GUI.DrawTexture(imgp, icon, script.AutoScale ? ScaleMode.ScaleToFit : ScaleMode.ScaleAndCrop);
                position.x += width + 25;
                position.width -= width + 25;
                position.height = EditorGUIUtility.singleLineHeight;
                position.y += EditorGUIUtility.singleLineHeight * 0.5f;
                var lw = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 100;
                EditorGUI.PropertyField(position, property);
                EditorGUIUtility.labelWidth = lw;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }
            else
            {
                if (script.Height <= 0)
                    return EditorGUIUtility.singleLineHeight * 2;
                else
                    return script.Height;
            }
        }
    }
#endif
}