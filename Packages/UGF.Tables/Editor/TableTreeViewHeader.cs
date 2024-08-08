using System;
using UGF.RuntimeTools.Runtime.Options;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace UGF.Tables.Editor
{
    public class TableTreeViewHeader : MultiColumnHeader
    {
        public TableTreeOptions Options { get; }

        public event TableTreeViewHeaderContextMenuHandler ContextMenuCreating;

        public TableTreeViewHeader(MultiColumnHeaderState state, TableTreeOptions options) : base(state)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void CreateContextMenu(GenericMenu menu)
        {
            base.AddColumnHeaderContextMenuItems(menu);
        }

        protected override void AddColumnHeaderContextMenuItems(GenericMenu menu)
        {
            if (currentColumnIndex >= 0)
            {
                TableTreeColumnOptions column = Options.Columns[currentColumnIndex];

                ContextMenuCreating?.Invoke(menu, column);
            }
            else
            {
                ContextMenuCreating?.Invoke(menu, Optional<TableTreeColumnOptions>.Empty);
            }
        }
    }
}
