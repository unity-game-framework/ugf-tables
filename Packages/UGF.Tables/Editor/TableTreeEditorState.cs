using System;
using UGF.EditorTools.Runtime.IMGUI.Types;
using UnityEditor;

namespace UGF.Tables.Editor
{
    public static class TableTreeEditorState
    {
        public static void Save()
        {
            TableTreeEditorUserSettings.Settings.SaveSettings();
        }

        public static bool Has(Type type)
        {
            return TryGet(type, out _);
        }

        public static bool TryRead(Type type, TableTreeViewState state)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (state == null) throw new ArgumentNullException(nameof(state));

            if (TryGet(type, out TableTreeViewState targetState))
            {
                string text = EditorJsonUtility.ToJson(targetState);

                EditorJsonUtility.FromJsonOverwrite(text, state);
                return true;
            }

            return false;
        }

        public static void Write(Type type, TableTreeViewState state)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (state == null) throw new ArgumentNullException(nameof(state));

            if (TryGet(type, out TableTreeViewState targetState))
            {
                string text = EditorJsonUtility.ToJson(state);

                EditorJsonUtility.FromJsonOverwrite(text, targetState);
            }
            else
            {
                TableTreeEditorUserSettingsData data = TableTreeEditorUserSettings.Settings.GetData();

                var stateData = new TableTreeEditorStateData
                {
                    Type = new TypeReference(type.AssemblyQualifiedName),
                    State = new TableTreeViewState()
                };

                string text = EditorJsonUtility.ToJson(state);

                EditorJsonUtility.FromJsonOverwrite(text, stateData.State);

                data.States.Add(stateData);
            }
        }

        public static bool TryGet(Type type, out TableTreeViewState state)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            TableTreeEditorUserSettingsData data = TableTreeEditorUserSettings.Settings.GetData();

            for (int i = 0; i < data.States.Count; i++)
            {
                TableTreeEditorStateData stateData = data.States[i];

                if (stateData.Type.TryGet(out Type assetType) && assetType == type)
                {
                    state = stateData.State;
                    return true;
                }
            }

            state = default;
            return false;
        }

        public static bool TryReset(Type type, TableTreeOptions options)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (options == null) throw new ArgumentNullException(nameof(options));

            TableTreeEditorUserSettingsData data = TableTreeEditorUserSettings.Settings.GetData();

            for (int i = 0; i < data.States.Count; i++)
            {
                TableTreeEditorStateData stateData = data.States[i];

                if (stateData.Type.TryGet(out Type assetType) && assetType == type)
                {
                    stateData.State = TableTreeEditorUtility.CreateState(options);
                    return true;
                }
            }

            return false;
        }

        public static void ClearAll()
        {
            TableTreeEditorUserSettingsData data = TableTreeEditorUserSettings.Settings.GetData();

            data.States.Clear();
        }
    }
}
