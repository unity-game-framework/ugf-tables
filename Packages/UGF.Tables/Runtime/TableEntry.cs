using System;
using UGF.EditorTools.Runtime.Ids;
using UnityEngine;

namespace UGF.Tables.Runtime
{
    [Serializable]
    public class TableEntry : ITableEntry
    {
        [SerializeField] private GlobalId m_id;
        [SerializeField] private string m_name;

        public GlobalId Id { get { return m_id; } set { m_id = value; } }
        public string Name { get { return m_name; } set { m_name = value; } }
    }
}
