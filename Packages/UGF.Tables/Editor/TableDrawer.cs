using System;
using System.Collections.Generic;
using UGF.EditorTools.Editor.Ids;
using UGF.EditorTools.Editor.IMGUI;
using UGF.EditorTools.Editor.IMGUI.Dropdown;
using UGF.EditorTools.Editor.Serialized;
using UGF.EditorTools.Runtime.Ids;
using UGF.Tables.Runtime;
using UnityEditor;
using UnityEngine;

namespace UGF.Tables.Editor
{
    public class TableDrawer : DrawerBase
    {
        public SerializedProperty SerializedProperty { get; }
        public SerializedProperty PropertyEntries { get; }
        public string PropertyIdName { get; }
        public string PropertyNameName { get; }
        public int SelectedIndex { get { return m_selectedIndex ?? throw new ArgumentException("Value not specified."); } }
        public bool HasSelected { get { return m_selectedIndex != null; } }
        public bool SearchById { get; set; }
        public bool ShowIndexes { get; set; }

        public event TableDrawerEntryHandler Added;
        public event TableDrawerEntryHandler Removing;
        public event TableDrawerEntryHandler Selected;
        public event TableDrawerEntryHandler Deselecting;
        public event TableDrawerMenuHandler MenuOpening;
        public event TableDrawerEntryHandler DrawingEntry;
        public event TableDrawerEntryHandler DrawingEntryHeader;
        public event TableDrawerEntryHandler DrawingEntryProperties;
        public event Action TableOpening;

        private readonly DropdownSelection<DropdownItem<int>> m_selection = new DropdownSelection<DropdownItem<int>>();
        private int? m_selectedIndex;
        private SerializedProperty m_selectedPropertyId;
        private SerializedProperty m_selectedPropertyName;
        private Styles m_styles;

        private class Styles
        {
            public GUIContent EntryNoneContent { get; } = new GUIContent("None");
            public GUIContent EntryEmptyContent { get; } = new GUIContent("Untitled");
            public GUIContent AddButtonContent { get; } = new GUIContent(EditorGUIUtility.FindTexture("Toolbar Plus"), "Add new entry.");
            public GUIContent RemoveButtonContent { get; } = new GUIContent(EditorGUIUtility.FindTexture("Toolbar Minus"), "Delete current entry.");
            public GUIContent OpenWindowContent { get; } = new GUIContent(EditorGUIUtility.FindTexture("HorizontalLayoutGroup Icon"), "Open table window.");
            public GUIContent MenuButtonContent { get; } = new GUIContent(EditorGUIUtility.FindTexture("_Menu"));
            public GUILayoutOption[] ToolbarButtonOptions { get; } = { GUILayout.Width(50F) };
            public GUILayoutOption[] ToolbarButtonSmallOptions { get; } = { GUILayout.Width(25F) };
        }

        public TableDrawer(SerializedProperty serializedProperty, string propertyIdName = "m_id", string propertyNameName = "m_name")
        {
            if (string.IsNullOrEmpty(propertyIdName)) throw new ArgumentException("Value cannot be null or empty.", nameof(propertyIdName));
            if (string.IsNullOrEmpty(propertyNameName)) throw new ArgumentException("Value cannot be null or empty.", nameof(propertyNameName));

            SerializedProperty = serializedProperty ?? throw new ArgumentNullException(nameof(serializedProperty));
            PropertyIdName = propertyIdName;
            PropertyNameName = propertyNameName;
            PropertyEntries = SerializedProperty.FindPropertyRelative("m_entries");

            m_selection.Dropdown.RootName = "Entries";
            m_selection.Dropdown.MinimumHeight = 300F;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            OnEntryDeselect();
        }

        public void DrawGUILayout()
        {
            m_styles ??= new Styles();

            SerializedProperty.isExpanded = EditorGUILayout.Foldout(SerializedProperty.isExpanded, SerializedProperty.displayName, true);

            if (SerializedProperty.isExpanded)
            {
                OnEntryControlsDraw();

                EditorGUILayout.Space();

                OnEntrySelectedDraw();
            }
        }

        protected virtual void OnAdd(int index, SerializedProperty propertyEntry)
        {
        }

        protected virtual void OnRemove(int index, SerializedProperty propertyEntry)
        {
        }

        protected virtual void OnSelect(int index, SerializedProperty propertyEntry)
        {
        }

        protected virtual void OnDeselect(int index, SerializedProperty propertyEntry)
        {
        }

        protected virtual void OnMenu(GenericMenu menu)
        {
        }

        protected virtual void OnTableOpen()
        {
            if (TableOpening != null)
            {
                TableOpening?.Invoke();
            }
            else
            {
                OpenTable();
            }
        }

        protected virtual void OnDraw(int index, SerializedProperty propertyEntry)
        {
            if (DrawingEntry != null)
            {
                DrawingEntry.Invoke(index, propertyEntry);
            }
            else
            {
                DrawEntryDefault(index, propertyEntry);
            }
        }

        protected void DrawEntryDefault(int index, SerializedProperty propertyEntry)
        {
            DrawEntryPropertiesHeader(index, propertyEntry);
            DrawEntryProperties(index, propertyEntry);
        }

        protected void DrawEntryPropertiesHeader(int index, SerializedProperty propertyEntry)
        {
            if (DrawingEntryHeader != null)
            {
                DrawingEntryHeader.Invoke(index, propertyEntry);
            }
            else
            {
                if (ShowIndexes)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.IntField("Index", SelectedIndex);
                    }
                }

                EditorGUILayout.PropertyField(m_selectedPropertyId);
                EditorGUILayout.PropertyField(m_selectedPropertyName);
            }
        }

        protected virtual void DrawEntryProperties(int index, SerializedProperty propertyEntry)
        {
            if (DrawingEntryProperties != null)
            {
                DrawingEntryProperties.Invoke(index, propertyEntry);
            }
            else
            {
                foreach (SerializedProperty property in SerializedPropertyEditorUtility.GetChildrenVisible(propertyEntry))
                {
                    if (property.name != PropertyIdName && property.name != PropertyNameName)
                    {
                        EditorGUILayout.PropertyField(property);
                    }
                }
            }
        }

        protected void OpenTable()
        {
            TableTreeEditorUtility.ShowWindow((TableAsset)SerializedProperty.serializedObject.targetObject);
        }

        private void OnEntrySelect(int index)
        {
            OnEntryDeselect();

            if (index < PropertyEntries.arraySize)
            {
                SerializedProperty propertyEntry = PropertyEntries.GetArrayElementAtIndex(index);

                m_selectedIndex = index;
                m_selectedPropertyId = propertyEntry.FindPropertyRelative(PropertyIdName);
                m_selectedPropertyName = propertyEntry.FindPropertyRelative(PropertyNameName);

                OnSelect(index, propertyEntry);

                Selected?.Invoke(index, propertyEntry);
            }
        }

        private void OnEntryDeselect()
        {
            if (m_selectedIndex != null)
            {
                int index = m_selectedIndex.Value;
                SerializedProperty propertyEntry = PropertyEntries.GetArrayElementAtIndex(m_selectedIndex.Value);

                OnDeselect(index, propertyEntry);

                Deselecting?.Invoke(index, propertyEntry);

                m_selectedIndex = null;
                m_selectedPropertyId = null;
                m_selectedPropertyName = null;
            }
        }

        private void OnEntryRemove(int index)
        {
            OnEntryDeselect();

            SerializedProperty propertyEntry = PropertyEntries.GetArrayElementAtIndex(index);

            OnRemove(index, propertyEntry);

            Removing?.Invoke(index, propertyEntry);

            PropertyEntries.DeleteArrayElementAtIndex(index);
        }

        private void OnEntryInsert(int index)
        {
            PropertyEntries.InsertArrayElementAtIndex(index);

            index = Mathf.Min(index + 1, PropertyEntries.arraySize - 1);

            SerializedProperty propertyEntry = PropertyEntries.GetArrayElementAtIndex(index);
            SerializedProperty propertyId = propertyEntry.FindPropertyRelative(PropertyIdName);
            SerializedProperty propertyName = propertyEntry.FindPropertyRelative(PropertyNameName);
            string entryName = propertyName.stringValue;

            GlobalIdEditorUtility.SetGlobalIdToProperty(propertyId, GlobalId.Generate());

            propertyName.stringValue = OnGetUniqueName(!string.IsNullOrEmpty(entryName) ? entryName : "Entry");

            foreach (SerializedProperty property in SerializedPropertyEditorUtility.GetChildrenVisible(propertyEntry))
            {
                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    property.managedReferenceValue = null;
                }
            }

            OnAdd(index, propertyEntry);
            OnEntrySelect(index);

            Added?.Invoke(index, propertyEntry);
        }

        private void OnEntrySelectedDraw()
        {
            if (m_selectedIndex != null)
            {
                OnDraw(m_selectedIndex.Value, PropertyEntries.GetArrayElementAtIndex(m_selectedIndex.Value));
            }
            else
            {
                EditorGUILayout.HelpBox("Select entry to edit or create new one.", MessageType.Info);
            }
        }

        private void OnEntryControlsDraw()
        {
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUIContent contentDropdown = m_styles.EntryNoneContent;

                if (m_selectedPropertyName != null)
                {
                    string entryName = m_selectedPropertyName.stringValue;

                    contentDropdown = !string.IsNullOrEmpty(entryName) ? new GUIContent(entryName) : m_styles.EntryEmptyContent;
                }

                Rect rectDropdown = GUILayoutUtility.GetRect(contentDropdown, EditorStyles.toolbarDropDown);

                if (DropdownEditorGUIUtility.Dropdown(rectDropdown, GUIContent.none, contentDropdown, m_selection, OnGetEntryKeyItems, out DropdownItem<int> selected, FocusType.Keyboard, EditorStyles.toolbarDropDown))
                {
                    OnEntrySelect(selected.Value);
                }

                if (GUILayout.Button(m_styles.AddButtonContent, EditorStyles.toolbarButton, m_styles.ToolbarButtonOptions))
                {
                    int index = m_selectedIndex ?? PropertyEntries.arraySize;

                    OnEntryInsert(index);
                }

                using (new EditorGUI.DisabledScope(m_selectedIndex == null))
                {
                    if (GUILayout.Button(m_styles.RemoveButtonContent, EditorStyles.toolbarButton, m_styles.ToolbarButtonOptions))
                    {
                        OnEntryRemove(SelectedIndex);
                    }
                }

                if (GUILayout.Button(m_styles.OpenWindowContent, EditorStyles.toolbarButton, m_styles.ToolbarButtonSmallOptions))
                {
                    OnTableOpen();
                }

                Rect rectMenu = GUILayoutUtility.GetRect(m_styles.MenuButtonContent, EditorStyles.toolbarButton, m_styles.ToolbarButtonSmallOptions);

                if (GUI.Button(rectMenu, m_styles.MenuButtonContent, EditorStyles.toolbarButton))
                {
                    OnMenuOpen(rectMenu);
                }
            }
        }

        private void OnMenuOpen(Rect position)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Search by Id"), SearchById, () => SearchById = !SearchById);
            menu.AddItem(new GUIContent("Show Indexes"), ShowIndexes, () => ShowIndexes = !ShowIndexes);
            menu.AddSeparator(string.Empty);

            if (PropertyEntries.arraySize > 0)
            {
                menu.AddItem(new GUIContent("Clear"), false, OnMenuClear);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Clear"));
            }

            OnMenu(menu);

            MenuOpening?.Invoke(menu);

            menu.DropDown(position);
        }

        private void OnMenuClear()
        {
            OnEntryDeselect();

            PropertyEntries.ClearArray();
            PropertyEntries.serializedObject.ApplyModifiedProperties();
        }

        private IEnumerable<DropdownItem<int>> OnGetEntryKeyItems()
        {
            var items = new List<DropdownItem<int>>();

            for (int i = 0; i < PropertyEntries.arraySize; i++)
            {
                SerializedProperty propertyEntry = PropertyEntries.GetArrayElementAtIndex(i);
                string displayName;

                if (SearchById)
                {
                    SerializedProperty propertyId = propertyEntry.FindPropertyRelative(PropertyIdName);

                    displayName = GlobalIdEditorUtility.GetGuidFromProperty(propertyId);
                }
                else
                {
                    SerializedProperty propertyName = propertyEntry.FindPropertyRelative(PropertyNameName);
                    string value = propertyName.stringValue;

                    displayName = !string.IsNullOrEmpty(value) ? value : m_styles.EntryEmptyContent.text;
                }

                if (ShowIndexes)
                {
                    displayName = $"[{i}] {displayName}";
                }

                items.Add(new DropdownItem<int>(displayName, i));
            }

            return items;
        }

        private string OnGetUniqueName(string entryName)
        {
            string[] names = new string[PropertyEntries.arraySize];

            for (int i = 0; i < PropertyEntries.arraySize; i++)
            {
                SerializedProperty propertyEntry = PropertyEntries.GetArrayElementAtIndex(i);
                SerializedProperty propertyName = propertyEntry.FindPropertyRelative(PropertyNameName);

                names[i] = propertyName.stringValue;
            }

            return ObjectNames.GetUniqueName(names, entryName);
        }
    }
}
