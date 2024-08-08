using System.Collections.Generic;
using UGF.EditorTools.Runtime.Ids;

namespace UGF.Tables.Runtime
{
    public interface ITable<TEntry> : ITable
    {
        IReadOnlyList<TEntry> Entries { get; }

        void Add(TEntry entry);
        bool Remove(string name);
        bool Remove(string name, out TEntry entry);
        bool Remove(GlobalId id);
        bool Remove(GlobalId id, out TEntry entry);
        TEntry GetByName(string name);
        bool TryGetByName(string name, out TEntry entry);
        TEntry Get(GlobalId id);
        bool TryGet(GlobalId id, out TEntry entry);
    }
}
