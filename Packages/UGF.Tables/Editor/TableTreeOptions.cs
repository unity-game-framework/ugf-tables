using System;
using System.Collections.Generic;
using UnityEditor;

namespace UGF.Tables.Editor
{
    public class TableTreeOptions
    {
        public IReadOnlyList<TableTreeColumnOptions> Columns { get; }
        public string PropertyEntriesName { get; set; } = "m_entries";
        public string PropertyIdName { get; set; } = "m_id";
        public string PropertyNameName { get; set; } = "m_name";
        public string PropertyChildrenName { get; set; } = "m_children";
        public float RowHeight { get; set; } = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2F;

        public TableTreeOptions(IReadOnlyList<TableTreeColumnOptions> columns)
        {
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        }

        public bool TryGetChildrenColumn(out TableTreeColumnOptions options)
        {
            return TryGetColumn(PropertyChildrenName, out options);
        }

        public bool TryGetColumn(string propertyName, out TableTreeColumnOptions column)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Value cannot be null or empty.", nameof(propertyName));

            for (int i = 0; i < Columns.Count; i++)
            {
                column = Columns[i];

                if (column.PropertyName == propertyName)
                {
                    return true;
                }
            }

            column = default;
            return false;
        }
    }
}
