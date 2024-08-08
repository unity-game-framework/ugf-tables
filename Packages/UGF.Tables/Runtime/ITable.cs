using System.Collections.Generic;
using UGF.EditorTools.Runtime.Ids;

namespace UGF.Tables.Runtime
{
    public interface ITable
    {
        int Count { get; }

        bool TryGetEntryId(string name, out GlobalId id);
        bool TryGetEntryName(GlobalId id, out string name);
        IEnumerable<ITableEntry> GetEntries();
    }
}
