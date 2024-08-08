using System;
using System.Collections.Generic;
using UGF.EditorTools.Runtime.Ids;
using UnityEngine;

namespace UGF.Tables.Runtime
{
    [Serializable]
    public class Table<TEntry> : ITable<TEntry> where TEntry : ITableEntry
    {
        [SerializeField] private List<TEntry> m_entries = new List<TEntry>();

        public List<TEntry> Entries { get { return m_entries; } }
        public int Count { get { return m_entries.Count; } }

        IReadOnlyList<TEntry> ITable<TEntry>.Entries { get { return Entries; } }

        public void Add(TEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            m_entries.Add(entry);
        }

        public bool Remove(string name)
        {
            return Remove(name, out _);
        }

        public bool Remove(string name, out TEntry entry)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            for (int i = 0; i < m_entries.Count; i++)
            {
                entry = m_entries[i];

                if (entry.Name == name)
                {
                    m_entries.RemoveAt(i);
                    return true;
                }
            }

            entry = default;
            return false;
        }

        public bool Remove(GlobalId id)
        {
            return Remove(id, out _);
        }

        public bool Remove(GlobalId id, out TEntry entry)
        {
            if (!id.IsValid()) throw new ArgumentException("Value should be valid.", nameof(id));

            for (int i = 0; i < m_entries.Count; i++)
            {
                entry = m_entries[i];

                if (entry.Id == id)
                {
                    m_entries.RemoveAt(i);
                    return true;
                }
            }

            entry = default;
            return false;
        }

        public TEntry GetByName(string name)
        {
            return TryGetByName(name, out TEntry entry) ? entry : throw new ArgumentException($"Entry not found by the specified name: '{name}'.");
        }

        public bool TryGetByName(string name, out TEntry entry)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            for (int i = 0; i < m_entries.Count; i++)
            {
                entry = m_entries[i];

                if (entry.Name == name)
                {
                    return true;
                }
            }

            entry = default;
            return false;
        }

        public TEntry Get(GlobalId id)
        {
            return TryGet(id, out TEntry entry) ? entry : throw new ArgumentException($"Entry not found by the specified id: '{id}'.");
        }

        public bool TryGet(GlobalId id, out TEntry entry)
        {
            if (!id.IsValid()) throw new ArgumentException("Value should be valid.", nameof(id));

            for (int i = 0; i < m_entries.Count; i++)
            {
                entry = m_entries[i];

                if (entry.Id == id)
                {
                    return true;
                }
            }

            entry = default;
            return false;
        }

        public bool TryGetEntryId(string name, out GlobalId id)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            for (int i = 0; i < m_entries.Count; i++)
            {
                TEntry entry = m_entries[i];

                if (entry.Name == name)
                {
                    id = entry.Id;
                    return true;
                }
            }

            id = default;
            return false;
        }

        public bool TryGetEntryName(GlobalId id, out string name)
        {
            if (!id.IsValid()) throw new ArgumentException("Value should be valid.", nameof(id));

            for (int i = 0; i < m_entries.Count; i++)
            {
                TEntry entry = m_entries[i];

                if (entry.Id == id)
                {
                    name = entry.Name;
                    return true;
                }
            }

            name = default;
            return false;
        }

        public IEnumerable<ITableEntry> GetEntries()
        {
            for (int i = 0; i < m_entries.Count; i++)
            {
                yield return m_entries[i];
            }
        }
    }
}
