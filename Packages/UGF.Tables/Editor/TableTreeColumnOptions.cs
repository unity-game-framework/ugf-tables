using System;
using System.Collections.Generic;
using UnityEditor;

namespace UGF.Tables.Editor
{
    public class TableTreeColumnOptions
    {
        public string PropertyName { get; }
        public string DisplayName { get; }
        public IComparer<SerializedProperty> Comparer { get; set; } = TableTreeColumnComparer.Default;
        public ITableTreeColumnSearcher Searcher { get; set; } = TableTreeColumnSearcher.Default;

        public TableTreeColumnOptions(string propertyName, string displayName)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Value cannot be null or empty.", nameof(propertyName));
            if (string.IsNullOrEmpty(displayName)) throw new ArgumentException("Value cannot be null or empty.", nameof(displayName));

            PropertyName = propertyName;
            DisplayName = displayName;
        }
    }
}
