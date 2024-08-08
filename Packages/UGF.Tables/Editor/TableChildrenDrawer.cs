using System;
using UGF.EditorTools.Editor.IMGUI;
using UGF.EditorTools.Editor.Serialized;
using UnityEditor;

namespace UGF.Tables.Editor
{
    public class TableChildrenDrawer : TableDrawer
    {
        public string PropertyChildrenName { get; }
        public bool PropertyChildrenAlwaysExpanded { get; set; } = true;
        public ReorderableListDrawer ListChildren { get { return m_listChildren ?? throw new ArgumentException("Value not specified."); } }
        public bool HasListChildren { get { return m_listChildren != null; } }

        private ReorderableListDrawer m_listChildren;

        public TableChildrenDrawer(
            SerializedProperty serializedProperty,
            string propertyIdName = "m_id",
            string propertyNameName = "m_name",
            string propertyChildrenName = "m_children") : base(serializedProperty, propertyIdName, propertyNameName)
        {
            if (string.IsNullOrEmpty(propertyChildrenName)) throw new ArgumentException("Value cannot be null or empty.", nameof(propertyChildrenName));

            PropertyChildrenName = propertyChildrenName;
        }

        protected override void OnSelect(int index, SerializedProperty propertyEntry)
        {
            base.OnSelect(index, propertyEntry);

            SerializedProperty propertyChildren = propertyEntry.FindPropertyRelative(PropertyChildrenName);

            m_listChildren = OnListChildrenCreate(index, propertyEntry, propertyChildren);
        }

        protected override void OnDeselect(int index, SerializedProperty propertyEntry)
        {
            base.OnDeselect(index, propertyEntry);

            OnListChildrenDisable();

            m_listChildren = null;
        }

        protected override void DrawEntryProperties(int index, SerializedProperty propertyEntry)
        {
            foreach (SerializedProperty property in SerializedPropertyEditorUtility.GetChildrenVisible(propertyEntry))
            {
                if (property.name == PropertyChildrenName)
                {
                    ListChildren.DrawGUILayout();
                }
                else
                {
                    if (property.name != PropertyIdName && property.name != PropertyNameName)
                    {
                        EditorGUILayout.PropertyField(property);
                    }
                }
            }
        }

        protected virtual ReorderableListDrawer OnListChildrenCreate(int index, SerializedProperty propertyEntry, SerializedProperty propertyChildren)
        {
            if (PropertyChildrenAlwaysExpanded)
            {
                propertyChildren.isExpanded = true;
            }

            var list = new ReorderableListDrawer(propertyChildren)
            {
                DisplayElementFoldout = false
            };

            list.Enable();

            return list;
        }

        protected virtual void OnListChildrenDisable()
        {
            ListChildren.Disable();
        }
    }
}
