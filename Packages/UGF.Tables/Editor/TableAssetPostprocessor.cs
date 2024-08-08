using System;
using System.Collections.Generic;
using UGF.Tables.Runtime;
using UnityEditor;

namespace UGF.Tables.Editor
{
    internal class TableAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            OnUpdate(importedAssets);
            OnUpdate(movedAssets);
            OnDelete(deletedAssets);
        }

        private static void OnUpdate(IReadOnlyList<string> paths)
        {
            if (paths == null) throw new ArgumentNullException(nameof(paths));

            for (int i = 0; i < paths.Count; i++)
            {
                string path = paths[i];
                GUID guid = AssetDatabase.GUIDFromAssetPath(path);
                Type type = AssetDatabase.GetMainAssetTypeFromGUID(guid);

                if (typeof(TableAsset).IsAssignableFrom(type))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<TableAsset>(path);

                    TableEntryCache.Update(guid, asset);
                }
            }
        }

        private static void OnDelete(IReadOnlyList<string> paths)
        {
            if (paths == null) throw new ArgumentNullException(nameof(paths));

            for (int i = 0; i < paths.Count; i++)
            {
                string path = paths[i];
                GUID guid = AssetDatabase.GUIDFromAssetPath(path);
                Type type = AssetDatabase.GetMainAssetTypeFromGUID(guid);

                if (typeof(TableAsset).IsAssignableFrom(type))
                {
                    TableEntryCache.Remove(guid);
                }
            }
        }
    }
}
