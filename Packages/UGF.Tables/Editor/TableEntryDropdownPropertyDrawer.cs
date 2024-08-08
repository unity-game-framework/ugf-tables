using UGF.EditorTools.Editor.IMGUI.PropertyDrawers;
using UGF.Tables.Runtime;
using UnityEditor;
using UnityEngine;

namespace UGF.Tables.Editor
{
    [CustomPropertyDrawer(typeof(TableEntryDropdownAttribute), true)]
    internal class TableEntryDropdownPropertyDrawer : PropertyDrawerTyped<TableEntryDropdownAttribute>
    {
        private readonly TableEntryDropdownDrawer m_drawer = new TableEntryDropdownDrawer();

        public TableEntryDropdownPropertyDrawer() : base(SerializedPropertyType.Hash128)
        {
        }

        protected override void OnDrawProperty(Rect position, SerializedProperty serializedProperty, GUIContent label)
        {
            m_drawer.SetTableType(Attribute.TableType);
            m_drawer.DrawGUI(position, label, serializedProperty);
            m_drawer.ClearTableType();
        }

        public override float GetPropertyHeight(SerializedProperty serializedProperty, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
