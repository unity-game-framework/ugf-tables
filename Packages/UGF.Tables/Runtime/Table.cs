using System;
using System.Collections.Generic;
using UGF.EditorTools.Runtime.Ids;
using UnityEngine;

namespace UGF.Tables.Runtime
{
    [Serializable]
    public class Table<TEntry> : ITable where TEntry : class, ITableEntry
    {
        [SerializeField] private List<TEntry> m_entries = new List<TEntry>();

        public List<TEntry> Entries { get { return m_entries; } }

        IReadOnlyList<ITableEntry> ITable.Entries { get { return Entries; } }

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

        public bool Remove(TEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            return m_entries.Remove(entry);
        }

        public T GetByName<T>(string name) where T : ITableEntry
        {
            return (T)(ITableEntry)GetByName(name);
        }

        public TEntry GetByName(string name)
        {
            return TryGetByName(name, out TEntry entry) ? entry : throw new ArgumentException($"Entry not found by the specified name: '{name}'.");
        }

        public bool TryGetByName<T>(string name, out T entry) where T : class, ITableEntry
        {
            if (TryGetByName(name, out TEntry result))
            {
                entry = (T)(ITableEntry)result;
                return true;
            }

            entry = default;
            return false;
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

        public T Get<T>(GlobalId id) where T : class, ITableEntry
        {
            return (T)(ITableEntry)Get(id);
        }

        public TEntry Get(GlobalId id)
        {
            return TryGet(id, out TEntry entry) ? entry : throw new ArgumentException($"Entry not found by the specified id: '{id}'.");
        }

        public bool TryGet<T>(GlobalId id, out T entry) where T : class, ITableEntry
        {
            if (TryGet(id, out TEntry value))
            {
                entry = (T)(ITableEntry)value;
                return true;
            }

            entry = default;
            return false;
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

        void ITable.Add(ITableEntry entry)
        {
            Add((TEntry)entry);
        }

        bool ITable.Remove(string name, out ITableEntry entry)
        {
            if (Remove(name, out TEntry value))
            {
                entry = value;
                return true;
            }

            entry = default;
            return false;
        }

        bool ITable.Remove(GlobalId id, out ITableEntry entry)
        {
            if (Remove(id, out TEntry value))
            {
                entry = value;
                return true;
            }

            entry = default;
            return false;
        }

        bool ITable.Remove(ITableEntry entry)
        {
            return Remove((TEntry)entry);
        }

        ITableEntry ITable.GetByName(string name)
        {
            return GetByName(name);
        }

        bool ITable.TryGetByName(string name, out ITableEntry entry)
        {
            if (TryGetByName(name, out TEntry value))
            {
                entry = value;
                return true;
            }

            entry = default;
            return false;
        }

        ITableEntry ITable.Get(GlobalId id)
        {
            return Get(id);
        }

        bool ITable.TryGet(GlobalId id, out ITableEntry entry)
        {
            if (TryGet(id, out TEntry value))
            {
                entry = value;
                return true;
            }

            entry = default;
            return false;
        }
    }
}
