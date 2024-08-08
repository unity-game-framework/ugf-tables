using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace UGF.Tables.Editor
{
    public class TableTreeViewItem : TreeViewItem
    {
        public TableTreeEntryType EntryType { get; }
        public int Index { get; }
        public SerializedProperty SerializedProperty { get; }
        public SerializedProperty PropertyChildren { get { return m_propertyChildren ?? throw new ArgumentException("Value not specified."); } }
        public SerializedProperty PropertyChildrenSize { get { return m_propertyChildrenSize ?? throw new ArgumentException("Value not specified."); } }
        public bool HasPropertyChildren { get { return m_propertyChildren != null; } }
        public Dictionary<TableTreeColumnOptions, SerializedProperty> ColumnProperties { get; } = new Dictionary<TableTreeColumnOptions, SerializedProperty>();

        private readonly SerializedProperty m_propertyChildren;
        private readonly SerializedProperty m_propertyChildrenSize;

        public TableTreeViewItem(int id, TableTreeEntryType entryType, int index, SerializedProperty serializedProperty, TableTreeOptions options) : base(id)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

            EntryType = entryType;
            Index = index;
            SerializedProperty = serializedProperty ?? throw new ArgumentNullException(nameof(serializedProperty));

            m_propertyChildren = SerializedProperty.FindPropertyRelative(options.PropertyChildrenName);
            m_propertyChildrenSize = m_propertyChildren?.FindPropertyRelative("Array.size");

            for (int i = 0; i < options.Columns.Count; i++)
            {
                TableTreeColumnOptions column = options.Columns[i];

                if (column.EntryType == EntryType)
                {
                    SerializedProperty propertyValue = SerializedProperty.FindPropertyRelative(column.PropertyName);

                    if (propertyValue != null)
                    {
                        ColumnProperties.Add(column, propertyValue);
                    }
                }
            }

            if (EntryType == TableTreeEntryType.Child && options.TryGetChildrenColumn(out TableTreeColumnOptions childrenColumn) && TableTreeEditorInternalUtility.IsSingleFieldProperty(SerializedProperty))
            {
                ColumnProperties.Add(childrenColumn, SerializedProperty);
            }
        }
    }
}
