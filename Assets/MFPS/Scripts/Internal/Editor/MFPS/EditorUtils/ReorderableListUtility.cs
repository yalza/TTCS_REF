using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using System.Collections;

namespace MFPSEditor
{
    public static class ReorderableListUtility
    {
        public static ReorderableList CreateAutoLayout(SerializedProperty property, float columnSpacing = 10f)
        {
            return CreateAutoLayout(property, true, true, true, true, null, null, columnSpacing);
        }

        public static ReorderableList CreateAutoLayout(SerializedProperty property, string[] headers, float?[] columnWidth = null, float columnSpacing = 10f)
        {
            return CreateAutoLayout(property, true, true, true, true, headers, columnWidth, columnSpacing);
        }

        public static ReorderableList CreateAutoLayout(SerializedProperty property, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton, float columnSpacing = 10f)
        {
            return CreateAutoLayout(property, draggable, displayHeader, displayAddButton, displayRemoveButton, null, null, columnSpacing);
        }

        public static ReorderableList CreateAutoLayout(SerializedProperty property, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton, string[] headers, float?[] columnWidth = null, float columnSpacing = 10f)
        {
            var list = new ReorderableList(property.serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton);
            var colmuns = new List<Column>();

            list.drawElementCallback = DrawElement(list, GetColumnsFunc(list, headers, columnWidth, colmuns), columnSpacing);
            list.drawHeaderCallback = DrawHeader(list, GetColumnsFunc(list, headers, columnWidth, colmuns), columnSpacing);

            return list;
        }

        public static bool DoLayoutListWithFoldout(ReorderableList list, string label = null)
        {
            var property = list.serializedProperty;
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label != null ? label : property.displayName);
            if (property.isExpanded)
            {
                list.DoLayoutList();
            }

            return property.isExpanded;
        }

        private static ReorderableList.ElementCallbackDelegate DrawElement(ReorderableList list, System.Func<List<Column>> getColumns, float columnSpacing)
        {
            return (rect, index, isActive, isFocused) =>
            {
                var property = list.serializedProperty;
                var columns = getColumns();
                var layouts = CalculateColumnLayout(columns, rect, columnSpacing);

                var arect = rect;
                arect.height = EditorGUIUtility.singleLineHeight;
                for (var ii = 0; ii < columns.Count; ii++)
                {
                    var c = columns[ii];

                    arect.width = layouts[ii];
                    EditorGUI.PropertyField(arect, property.GetArrayElementAtIndex(index).FindPropertyRelative(c.PropertyName), GUIContent.none);
                    arect.x += arect.width + columnSpacing;
                }
            };
        }

        private static ReorderableList.HeaderCallbackDelegate DrawHeader(ReorderableList list, System.Func<List<Column>> getColumns, float columnSpacing)
        {
            return (rect) =>
            {
                var columns = getColumns();

                if (list.draggable)
                {
                    rect.width -= 15;
                    rect.x += 15;
                }

                var layouts = CalculateColumnLayout(columns, rect, columnSpacing);
                var arect = rect;
                arect.height = EditorGUIUtility.singleLineHeight;
                for (var ii = 0; ii < columns.Count; ii++)
                {
                    var c = columns[ii];

                    arect.width = layouts[ii];
                    EditorGUI.LabelField(arect, c.DisplayName);
                    arect.x += arect.width + columnSpacing;
                }
            };
        }

        private static System.Func<List<Column>> GetColumnsFunc(ReorderableList list, string[] headers, float?[] columnWidth, List<Column> output)
        {
            var property = list.serializedProperty;
            return () =>
            {
                if (output.Count <= 0 || list.serializedProperty != property)
                {
                    output.Clear();
                    property = list.serializedProperty;

                    if (property.isArray && property.arraySize > 0)
                    {
                        var it = property.GetArrayElementAtIndex(0).Copy();
                        var prefix = it.propertyPath;
                        var index = 0;
                        if (it.Next(true))
                        {
                            do
                            {
                                if (it.propertyPath.StartsWith(prefix))
                                {
                                    var c = new Column();
                                    c.DisplayName = (headers != null && headers.Length > index) ? headers[index] : it.displayName;
                                    c.PropertyName = it.propertyPath.Substring(prefix.Length + 1);
                                    c.Width = (columnWidth != null && columnWidth.Length > index) ? columnWidth[index] : null;

                                    output.Add(c);
                                }
                                else
                                {
                                    break;
                                }

                                index += 1;
                            }
                            while (it.Next(false));
                        }
                    }
                }

                return output;
            };
        }

        private static List<float> CalculateColumnLayout(List<Column> columns, Rect rect, float columnSpacing)
        {
            var autoWidth = rect.width;
            var autoCount = 0;
            foreach (var column in columns)
            {
                if (column.Width.HasValue)
                {
                    autoWidth -= column.Width.Value;
                }
                else
                {
                    autoCount += 1;
                }
            }

            autoWidth -= (columns.Count - 1) * columnSpacing;
            autoWidth /= autoCount;

            var widths = new List<float>(columns.Count);
            foreach (var column in columns)
            {
                if (column.Width.HasValue)
                {
                    widths.Add(column.Width.Value);
                }
                else
                {
                    widths.Add(autoWidth);
                }
            }

            return widths;
        }

        private struct Column
        {
            public string DisplayName;
            public string PropertyName;
            public float? Width;
        }

        #region Simple string path based extensions
        /// <summary>
        /// Returns the path to the parent of a SerializedProperty
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static string ParentPath(this SerializedProperty prop)
        {
            int lastDot = prop.propertyPath.LastIndexOf('.');
            if (lastDot == -1) // No parent property
                return "";

            return prop.propertyPath.Substring(0, lastDot);
        }

        /// <summary>
        /// Returns the parent of a SerializedProperty, as another SerializedProperty
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static SerializedProperty GetParentProp(this SerializedProperty prop)
        {
            string parentPath = prop.ParentPath();
            return prop.serializedObject.FindProperty(parentPath);
        }
        #endregion


        #region Reflection based extensions
        // http://answers.unity3d.com/questions/425012/get-the-instance-the-serializedproperty-belongs-to.html

        /// <summary>
        /// Use reflection to get the actual data instance of a SerializedProperty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetValue<T>(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }
            if (obj is T)
                return (T)obj;
            return null;
        }

        public static Type GetTypeReflection(this SerializedProperty prop)
        {
            object obj = GetParent<object>(prop);
            if (obj == null)
                return null;

            Type objType = obj.GetType();
            const BindingFlags bindingFlags = System.Reflection.BindingFlags.GetField
                                              | System.Reflection.BindingFlags.GetProperty
                                              | System.Reflection.BindingFlags.Instance
                                              | System.Reflection.BindingFlags.NonPublic
                                              | System.Reflection.BindingFlags.Public;
            FieldInfo field = objType.GetField(prop.name, bindingFlags);
            if (field == null)
                return null;
            return field.FieldType;
        }

        /// <summary>
        /// Uses reflection to get the actual data instance of the parent of a SerializedProperty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static T GetParent<T>(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }
            return (T)obj;
        }

        private static object GetValue(object source, string name)
        {
            if (source == null)
                return null;
            Type type = source.GetType();
            FieldInfo f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null)
            {
                PropertyInfo p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null)
                    return null;
                return p.GetValue(source, null);
            }
            return f.GetValue(source);
        }

        private static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            if (enumerable == null)
                return null;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
                enm.MoveNext();
            return enm.Current;
        }

        /// <summary>
        /// Use reflection to check if SerializedProperty has a given attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static bool HasAttribute<T>(this SerializedProperty prop)
        {
            object[] attributes = GetAttributes<T>(prop);
            if (attributes != null)
            {
                return attributes.Length > 0;
            }
            return false;
        }

        /// <summary>
        /// Use reflection to get the attributes of the SerializedProperty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object[] GetAttributes<T>(this SerializedProperty prop)
        {
            object obj = GetParent<object>(prop);
            if (obj == null)
                return new object[0];

            Type attrType = typeof(T);
            Type objType = obj.GetType();
            const BindingFlags bindingFlags = System.Reflection.BindingFlags.GetField
                                              | System.Reflection.BindingFlags.GetProperty
                                              | System.Reflection.BindingFlags.Instance
                                              | System.Reflection.BindingFlags.NonPublic
                                              | System.Reflection.BindingFlags.Public;
            FieldInfo field = objType.GetField(prop.name, bindingFlags);
            if (field != null)
                return field.GetCustomAttributes(attrType, true);
            return new object[0];
        }
        #endregion
    }
}