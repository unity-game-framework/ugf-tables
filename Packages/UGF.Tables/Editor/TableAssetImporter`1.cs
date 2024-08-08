using UGF.Tables.Runtime;
using UnityEngine;

namespace UGF.Tables.Editor
{
    public abstract class TableAssetImporter<TTable> : TableAssetImporter where TTable : TableAsset
    {
        [SerializeField] private TTable m_table;

        public TTable Table { get { return m_table; } set { m_table = value; } }

        protected override bool OnIsValid()
        {
            return m_table != null;
        }
    }
}
