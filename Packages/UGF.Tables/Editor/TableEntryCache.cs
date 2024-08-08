using System;
using System.Collections.Generic;
using UGF.EditorTools.Runtime.Ids;
using UGF.RuntimeTools.Runtime.Tables;
using UnityEditor;

namespace UGF.Tables.Editor
{
    [InitializeOnLoad]
    internal static class TableEntryCache
    {
        private static readonly Dictionary<GUID, TableEntryCollection> m_entries = new Dictionary<GUID, TableEntryCollection>();
        private static readonly Dictionary<GlobalId, EntryNameCollection> m_names = new Dictionary<GlobalId, EntryNameCollection>();

        public class TableEntryCollection : HashSet<GlobalId>
        {
            public string TableName { get; set; }
            public Type TableType { get; set; }
        }

        public class EntryNameCollection : Dictionary<GUID, HashSet<string>>
        {
        }

        static TableEntryCache()
        {
            Update();
        }

        public static void Update()
        {
            Update(typeof(TableAsset));
        }

        public static void Update(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            Clear();

            string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");

            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GUID id = AssetDatabase.GUIDFromAssetPath(path);

                var asset = AssetDatabase.LoadAssetAtPath<TableAsset>(path);

                Update(id, asset);
            }
        }

        public static void Update(GUID guid, TableAsset asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            ITable table = asset.Get();

            Remove(guid);

            if (table.Entries.Count > 0)
            {
                Add(guid, asset);
            }
        }

        public static void Add(GUID guid, TableAsset asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            ITable table = asset.Get();

            if (table.Entries.Count > 0)
            {
                var entryCollection = new TableEntryCollection
                {
                    TableName = asset.name,
                    TableType = asset.GetType()
                };

                m_entries.Add(guid, entryCollection);

                for (int i = 0; i < table.Entries.Count; i++)
                {
                    ITableEntry entry = table.Entries[i];

                    if (!m_names.TryGetValue(entry.Id, out EntryNameCollection nameCollection))
                    {
                        nameCollection = new EntryNameCollection();

                        m_names.Add(entry.Id, nameCollection);
                    }

                    if (!nameCollection.TryGetValue(guid, out HashSet<string> names))
                    {
                        names = new HashSet<string>();

                        nameCollection.Add(guid, names);
                    }

                    names.Add(entry.Name);
                    entryCollection.Add(entry.Id);
                }
            }
        }

        public static bool Remove(GUID guid)
        {
            if (m_entries.Remove(guid, out TableEntryCollection entryCollection))
            {
                foreach (GlobalId id in entryCollection)
                {
                    if (m_names.TryGetValue(id, out EntryNameCollection nameCollection))
                    {
                        nameCollection.Remove(guid);

                        if (nameCollection.Count == 0)
                        {
                            m_names.Remove(id);
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public static void Clear()
        {
            m_entries.Clear();
            m_names.Clear();
        }

        public static bool TryGetEntryCollection(GUID guid, out TableEntryCollection entryCollection)
        {
            return m_entries.TryGetValue(guid, out entryCollection);
        }

        public static bool TryGetNameCollection(GlobalId id, out EntryNameCollection nameCollection)
        {
            if (!id.IsValid()) throw new ArgumentException("Value should be valid.", nameof(id));

            return m_names.TryGetValue(id, out nameCollection);
        }
    }
}
