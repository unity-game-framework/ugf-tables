using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace UGF.Tables.Editor
{
    public class TableTreeViewItem : TreeViewItem
    {
        public int Index { get; }
        public SerializedProperty SerializedProperty { get; }
        public Dictionary<TableTreeColumnOptions, SerializedProperty> ColumnProperties { get; } = new Dictionary<TableTreeColumnOptions, SerializedProperty>();

        public TableTreeViewItem(int id, int index, SerializedProperty serializedProperty, TableTreeOptions options) : base(id)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

            Index = index;
            SerializedProperty = serializedProperty ?? throw new ArgumentNullException(nameof(serializedProperty));

            for (int i = 0; i < options.Columns.Count; i++)
            {
                TableTreeColumnOptions column = options.Columns[i];

                SerializedProperty propertyValue = SerializedProperty.FindPropertyRelative(column.PropertyName);

                if (propertyValue != null)
                {
                    ColumnProperties.Add(column, propertyValue);
                }
            }
        }
    }
}
