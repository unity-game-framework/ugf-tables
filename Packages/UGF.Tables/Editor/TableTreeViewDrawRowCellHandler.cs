using UnityEditor;
using UnityEngine;

namespace UGF.Tables.Editor
{
    public delegate void TableTreeViewDrawRowCellHandler(Rect position, TableTreeViewItem item, SerializedProperty serializedProperty, TableTreeColumnOptions column);
}
