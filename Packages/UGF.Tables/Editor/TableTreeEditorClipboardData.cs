using System;
using System.Collections.Generic;
using UGF.EditorTools.Runtime.IMGUI.Types;
using UnityEngine;

namespace UGF.Tables.Editor
{
    [Serializable]
    public class TableTreeEditorClipboardData
    {
        [SerializeField] private TypeReference m_type;
        [SerializeReference] private List<object> m_entries = new List<object>();

        public TypeReference Type { get { return m_type; } set { m_type = value; } }
        public List<object> Entries { get { return m_entries; } }
    }
}
