using System;
using System.Collections.Generic;
using UGF.EditorTools.Runtime.IMGUI.Types;
using UnityEngine;

namespace UGF.Tables.Editor
{
    public static class TableTreeEditorClipboard
    {
        public static int EntriesCount { get { return TableTreeEditorUserSettings.Settings.GetData().Clipboard.Entries.Count; } }
        public static int ChildrenCount { get { return TableTreeEditorUserSettings.Settings.GetData().Clipboard.Children.Count; } }

        public static TableTreeEditorClipboardData GetData()
        {
            return TableTreeEditorUserSettings.Settings.GetData().Clipboard;
        }

        public static void Save()
        {
            TableTreeEditorUserSettings.Settings.SaveSettings();
        }

        public static bool HasMatch(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            TableTreeEditorUserSettingsData data = TableTreeEditorUserSettings.Settings.GetData();

            return data.Clipboard.Type.HasValue && data.Clipboard.Type.TryGet(out Type value) && value == type;
        }

        public static bool HasAny()
        {
            TableTreeEditorUserSettingsData data = TableTreeEditorUserSettings.Settings.GetData();

            return data.Clipboard.Entries.Count > 0 || data.Clipboard.Children.Count > 0;
        }

        public static void CopyType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            TableTreeEditorUserSettingsData data = TableTreeEditorUserSettings.Settings.GetData();

            TypeReference reference = data.Clipboard.Type;

            reference.Set(type);

            data.Clipboard.Type = reference;
        }

        public static bool TryCopyEntries(IReadOnlyList<TableTreeViewItem> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            if (!TableTreeEditorClipboardUtility.TryCopy(items, TableTreeEditorUserSettings.Settings.GetData().Clipboard.Entries, out Exception error))
            {
                Debug.LogWarning($"Table entry can not be copied.\n{error}");
                return false;
            }

            return true;
        }

        public static bool TryCopyChildren(IReadOnlyList<TableTreeViewItem> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            if (!TableTreeEditorClipboardUtility.TryCopy(items, TableTreeEditorUserSettings.Settings.GetData().Clipboard.Children, out Exception error))
            {
                Debug.LogWarning($"Table entry child can not be copied.\n{error}");
                return false;
            }

            return true;
        }

        public static void Clear()
        {
            TableTreeEditorUserSettingsData data = TableTreeEditorUserSettings.Settings.GetData();

            data.Clipboard.Type.Clear();
            data.Clipboard.Entries.Clear();
            data.Clipboard.Children.Clear();
        }
    }
}
