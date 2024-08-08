using System;
using System.Collections.Generic;
using UGF.EditorTools.Editor.Ids;
using UGF.EditorTools.Runtime.Ids;
using UnityEditor;

namespace UGF.Tables.Editor
{
    public class TableTreeColumnSearcher : ITableTreeColumnSearcher
    {
        public static TableTreeColumnSearcher Default { get; } = new TableTreeColumnSearcher();

        public bool Check(SerializedProperty serializedProperty, string search)
        {
            if (serializedProperty == null) throw new ArgumentNullException(nameof(serializedProperty));
            if (search == null) throw new ArgumentNullException(nameof(search));

            return OnCheck(serializedProperty, search);
        }

        protected virtual bool OnCheck(SerializedProperty serializedProperty, string search)
        {
            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.String:
                case SerializedPropertyType.Color:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.Bounds:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                case SerializedPropertyType.RectInt:
                case SerializedPropertyType.BoundsInt:
                case SerializedPropertyType.Hash128:
                {
                    object value = serializedProperty.boxedValue;
                    string text = value != null ? value.ToString() : string.Empty;

                    return OnCheck(text, search);
                }
                case SerializedPropertyType.ManagedReference:
                {
                    string text = serializedProperty.managedReferenceId.ToString();

                    return OnCheck(text, search);
                }
                case SerializedPropertyType.Enum:
                {
                    string[] names = serializedProperty.enumDisplayNames;
                    int index = serializedProperty.enumValueIndex;
                    string text = names[index];

                    return OnCheck(text, search);
                }
                case SerializedPropertyType.Generic:
                {
                    if (serializedProperty.isArray)
                    {
                        string text = serializedProperty.arraySize.ToString();

                        return OnCheck(text, search);
                    }

                    if (serializedProperty.type == nameof(GlobalId))
                    {
                        string text = GlobalIdEditorUtility.GetGuidFromProperty(serializedProperty);

                        return OnCheck(text, search);
                    }

                    return false;
                }
                default:
                {
                    return false;
                }
            }
        }

        protected virtual bool OnCheck(string text, string search)
        {
            if (string.IsNullOrEmpty(search)) throw new ArgumentException("Value cannot be null or empty.", nameof(search));

            if (search.Contains(' '))
            {
                string[] query = search.Split(' ');

                return OnCheck(text, query);
            }

            return text.Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        protected virtual bool OnCheck(string text, IReadOnlyList<string> searchQuery)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentException("Value cannot be null or empty.", nameof(text));
            if (searchQuery == null) throw new ArgumentNullException(nameof(searchQuery));

            for (int i = 0; i < searchQuery.Count; i++)
            {
                string search = searchQuery[i];

                if (text.Contains(search, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
