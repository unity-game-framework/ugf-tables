using System;
using UGF.EditorTools.Runtime.Ids;
using UnityEngine;

namespace UGF.Tables.Runtime.Tests
{
    [CreateAssetMenu(menuName = "Tests/TestTableAsset")]
    public class TestTableAsset : TableAsset<TestTableEntry>
    {
    }

    [Serializable]
    public struct TestTableEntry : ITableEntry
    {
        [SerializeField] private Hash128 m_id;
        [SerializeField] private string m_name;
        [TableEntryDropdown(typeof(TestTableAsset))]
        [SerializeField] private Hash128 m_entry;

        public GlobalId Id { get { return m_id; } set { m_id = value; } }
        public string Name { get { return m_name; } set { m_name = value; } }
        public Hash128 Entry { get { return m_entry; } set { m_entry = value; } }
    }
}
