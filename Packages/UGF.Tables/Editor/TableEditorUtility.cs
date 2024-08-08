using System;
using System.Collections.Generic;
using UGF.EditorTools.Runtime.Ids;
using UGF.RuntimeTools.Runtime.Tables;
using UnityEditor;

namespace UGF.Tables.Editor
{
    public static class TableEditorUtility
    {
        public static IReadOnlyList<TableAsset> FindTableAssetAll(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!typeof(TableAsset).IsAssignableFrom(type)) throw new ArgumentException($"Type must be derived from: '{nameof(TableAsset)}'.");

            var result = new List<TableAsset>();
            string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<TableAsset>(path);

                result.Add(asset);
            }

            return result;
        }

        public static void TryGetEntryNameFromCache(GlobalId id, Type tableType, ICollection<string> names)
        {
            if (!id.IsValid()) throw new ArgumentException("Value should be valid.", nameof(id));
            if (tableType == null) throw new ArgumentNullException(nameof(tableType));
            if (names == null) throw new ArgumentNullException(nameof(names));

            if (TableEntryCache.TryGetNameCollection(id, out TableEntryCache.EntryNameCollection nameCollection))
            {
                foreach ((GUID guid, HashSet<string> values) in nameCollection)
                {
                    if (TableEntryCache.TryGetEntryCollection(guid, out TableEntryCache.TableEntryCollection entryCollection))
                    {
                        if (tableType.IsAssignableFrom(entryCollection.TableType))
                        {
                            foreach (string value in values)
                            {
                                names.Add(value);
                            }
                        }
                    }
                }
            }
        }

        public static void TryGetEntryNameFromCache(GlobalId id, ICollection<string> names)
        {
            if (!id.IsValid()) throw new ArgumentException("Value should be valid.", nameof(id));
            if (names == null) throw new ArgumentNullException(nameof(names));

            if (TableEntryCache.TryGetNameCollection(id, out TableEntryCache.EntryNameCollection nameCollection))
            {
                foreach ((_, HashSet<string> values) in nameCollection)
                {
                    foreach (string value in values)
                    {
                        names.Add(value);
                    }
                }
            }
        }

        public static bool TryGetEntryNameFromCache(GlobalId id, out string name)
        {
            if (TableEntryCache.TryGetNameCollection(id, out TableEntryCache.EntryNameCollection nameCollection))
            {
                foreach ((_, HashSet<string> names) in nameCollection)
                {
                    foreach (string value in names)
                    {
                        name = value;
                        return true;
                    }
                }
            }

            name = default;
            return false;
        }

        public static bool TryGetEntryNameFromAll(Type type, GlobalId id, out string name)
        {
            IReadOnlyList<TableAsset> tables = FindTableAssetAll(type);

            name = default;
            return tables.Count > 0 && TryGetEntryName(tables, id, out name);
        }

        public static bool TryGetEntryName(IReadOnlyList<TableAsset> tables, GlobalId id, out string name)
        {
            if (tables == null) throw new ArgumentNullException(nameof(tables));
            if (tables.Count == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(tables));
            if (!id.IsValid()) throw new ArgumentException("Value should be valid.", nameof(id));

            for (int i = 0; i < tables.Count; i++)
            {
                ITable table = tables[i].Get();

                if (TryGetEntryName(table, id, out name))
                {
                    return true;
                }
            }

            name = default;
            return false;
        }

        public static bool TryGetEntryName(ITable table, GlobalId id, out string name)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (!id.IsValid()) throw new ArgumentException("Value should be valid.", nameof(id));

            for (int i = 0; i < table.Entries.Count; i++)
            {
                ITableEntry entry = table.Entries[i];

                if (entry.Id == id)
                {
                    name = entry.Name;
                    return true;
                }
            }

            name = default;
            return false;
        }
    }
}
