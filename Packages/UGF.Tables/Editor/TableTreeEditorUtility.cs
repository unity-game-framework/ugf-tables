using System;
using System.Collections.Generic;
using System.Reflection;
using UGF.EditorTools.Editor.IMGUI;
using UGF.EditorTools.Runtime.Ids;
using UGF.RuntimeTools.Runtime.Tables;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UGF.Tables.Editor
{
    public static class TableTreeEditorUtility
    {
        private static readonly Type[] m_windowDockTypes;

        static TableTreeEditorUtility()
        {
            var types = new List<Type> { typeof(TableTreeWindow) };

            types.AddRange(TypeCache.GetTypesDerivedFrom<TableTreeWindow>());

            m_windowDockTypes = types.ToArray();
        }

        public static TableTreeWindow ShowWindow(TableAsset asset, GlobalId focusItemId = default)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            TableTreeWindow window = GetWindow(asset);

            ShowWindow(window, asset, focusItemId);

            return window;
        }

        public static void ShowWindow(TableTreeWindow window, TableAsset asset, GlobalId focusItemId = default)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            string path = AssetDatabase.GetAssetPath(asset);
            Texture icon = AssetDatabase.GetCachedIcon(path);

            window.titleContent = new GUIContent(asset.name, icon, asset.name);
            window.minSize = new Vector2(500F, 500F);
            window.SetTarget(asset);
            window.Show();

            if (focusItemId.IsValid())
            {
                window.Drawer.TreeView.TryFocusAtItem(focusItemId);
            }

            window.Focus();
            window.Drawer.TreeView.SetFocusAndEnsureSelectedItem();
        }

        public static TableTreeWindow GetWindow(TableAsset asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            Type assetType = asset.GetType();
            Type windowType = GetWindowType(assetType);

            return GetWindow(windowType, asset);
        }

        public static TableTreeWindow GetWindow(Type windowType, TableAsset asset)
        {
            if (windowType == null) throw new ArgumentNullException(nameof(windowType));

            if (!TryGetWindow(asset, out TableTreeWindow window))
            {
                window = (TableTreeWindow)EditorIMGUIUtility.CreateWindow(windowType, m_windowDockTypes);
            }

            return window;
        }

        public static bool TryGetWindow(TableAsset asset, out TableTreeWindow window)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            TableTreeWindow[] windows = Resources.FindObjectsOfTypeAll<TableTreeWindow>();

            for (int i = 0; i < windows.Length; i++)
            {
                window = windows[i];

                if (window.HasSerializedObject && window.SerializedObject.targetObject == asset)
                {
                    return true;
                }
            }

            window = default;
            return false;
        }

        public static Type GetWindowType(Type assetType)
        {
            if (assetType == null) throw new ArgumentNullException(nameof(assetType));

            TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<TableTreeWindowAttribute>();

            var attributes = new List<(Type Type, TableTreeWindowAttribute Attribute)>();

            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];

                var attribute = type.GetCustomAttribute<TableTreeWindowAttribute>();

                if (attribute != null)
                {
                    attributes.Add((type, attribute));
                }
            }

            attributes.Sort((a, b) => a.Attribute.Priority.CompareTo(b.Attribute.Priority));

            for (int i = 0; i < attributes.Count; i++)
            {
                (Type type, TableTreeWindowAttribute attribute) = attributes[i];

                if (attribute.AssetType.IsAssignableFrom(assetType))
                {
                    return type;
                }
            }

            return typeof(TableTreeWindow);
        }

        public static TableTreeViewState CreateState(TableTreeOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var columns = new MultiColumnHeaderState.Column[options.Columns.Count];

            for (int i = 0; i < options.Columns.Count; i++)
            {
                TableTreeColumnOptions column = options.Columns[i];

                columns[i] = new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(column.DisplayName, $"{column.DisplayName} ({column.PropertyName})")
                };
            }

            return new TableTreeViewState
            {
                Header = new MultiColumnHeaderState(columns)
            };
        }

        public static TableTreeOptions CreateOptions(TableAsset asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            ITable table = asset.Get();

            return CreateOptions(table.GetType());
        }

        public static TableTreeOptions CreateOptions(Type tableType)
        {
            if (tableType == null) throw new ArgumentNullException(nameof(tableType));

            Type entryType = GetTableEntryType(tableType);

            if (!TryGetEntryChildrenPropertyName(entryType, out string childrenPropertyName))
            {
                childrenPropertyName = "m_children";
            }

            List<TableTreeColumnOptions> columns = CreateColumnOptions(entryType);

            return new TableTreeOptions(columns)
            {
                PropertyChildrenName = childrenPropertyName
            };
        }

        public static List<TableTreeColumnOptions> CreateColumnOptions(Type entryType)
        {
            if (entryType == null) throw new ArgumentNullException(nameof(entryType));

            var columns = new List<TableTreeColumnOptions>();

            if (!TryGetEntryChildrenPropertyName(entryType, out string childrenPropertyName))
            {
                childrenPropertyName = "m_children";
            }

            List<FieldInfo> fields = TableTreeEditorInternalUtility.GetSerializedFields(entryType);

            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo field = fields[i];

                string displayName = ObjectNames.NicifyVariableName(field.Name);

                columns.Add(new TableTreeColumnOptions(field.Name, displayName, TableTreeEntryType.Entry));

                if (field.Name == childrenPropertyName)
                {
                    Type type = GetTableEntryChildrenType(field.FieldType);

                    CreateColumnOptionsFromFields(columns, TableTreeEditorInternalUtility.GetSerializedFields(type), TableTreeEntryType.Child);
                }
            }

            return columns;
        }

        public static void CreateColumnOptionsFromFields(ICollection<TableTreeColumnOptions> columns, IReadOnlyList<FieldInfo> fields, TableTreeEntryType entryType)
        {
            if (columns == null) throw new ArgumentNullException(nameof(columns));
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo field = fields[i];

                string displayName = ObjectNames.NicifyVariableName(field.Name);

                columns.Add(new TableTreeColumnOptions(field.Name, displayName, entryType));
            }
        }

        public static bool TryGetEntryChildrenPropertyName(Type entryType, out string propertyName)
        {
            if (entryType == null) throw new ArgumentNullException(nameof(entryType));

            var attribute = entryType.GetCustomAttribute<TableEntryChildrenAttribute>();

            if (attribute != null)
            {
                propertyName = attribute.PropertyName;
                return true;
            }

            propertyName = default;
            return false;
        }

        public static Type GetTableEntryType(Type tableType)
        {
            if (tableType == null) throw new ArgumentNullException(nameof(tableType));

            Type[] genericArguments = tableType.GetGenericArguments();

            if (genericArguments.Length != 1)
            {
                throw new ArgumentException($"Table entry type is unknown: '{tableType}'.");
            }

            return genericArguments[0];
        }

        public static Type GetTableEntryChildrenType(Type collectionType)
        {
            if (collectionType == null) throw new ArgumentNullException(nameof(collectionType));

            if (collectionType.IsArray)
            {
                return collectionType.GetElementType();
            }

            if (collectionType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type[] genericArguments = collectionType.GetGenericArguments();

                return genericArguments[0];
            }

            throw new ArgumentException($"Table entry children type is unknown: '{collectionType}'.");
        }
    }
}
