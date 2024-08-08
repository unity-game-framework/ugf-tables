using UGF.EditorTools.Runtime.Ids;

namespace UGF.Tables.Runtime
{
    public interface ITableEntry
    {
        GlobalId Id { get; }
        string Name { get; }
    }
}
