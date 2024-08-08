using System.Collections.Generic;
using UGF.CustomSettings.Runtime;
using UnityEngine;

namespace UGF.Tables.Editor
{
    public class TableTreeEditorUserSettingsData : CustomSettingsData
    {
        [SerializeField] private TableTreeEditorClipboardData m_clipboard = new TableTreeEditorClipboardData();
        [SerializeField] private List<TableTreeEditorStateData> m_states = new List<TableTreeEditorStateData>();

        public TableTreeEditorClipboardData Clipboard { get { return m_clipboard; } }
        public List<TableTreeEditorStateData> States { get { return m_states; } }
    }
}
