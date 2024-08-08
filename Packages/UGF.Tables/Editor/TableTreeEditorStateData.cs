using System;
using UGF.EditorTools.Runtime.IMGUI.Types;
using UnityEngine;

namespace UGF.Tables.Editor
{
    [Serializable]
    public class TableTreeEditorStateData
    {
        [SerializeField] private TypeReference m_type;
        [SerializeField] private TableTreeViewState m_state;

        public TypeReference Type { get { return m_type; } set { m_type = value; } }
        public TableTreeViewState State { get { return m_state; } set { m_state = value; } }
    }
}
