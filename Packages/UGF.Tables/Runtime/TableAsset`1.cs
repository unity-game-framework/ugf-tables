using UnityEngine;

namespace UGF.Tables.Runtime
{
    public abstract class TableAsset<TEntry> : TableAsset where TEntry : class, ITableEntry
    {
        [SerializeField] private Table<TEntry> m_table = new Table<TEntry>();

        public Table<TEntry> Table { get { return m_table; } }

        protected override ITable OnGet()
        {
            return m_table;
        }

        protected override void OnSet(ITable table)
        {
            m_table = (Table<TEntry>)table;
        }
    }
}
