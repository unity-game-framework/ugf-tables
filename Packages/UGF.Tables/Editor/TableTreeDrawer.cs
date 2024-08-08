using System;
using System.Collections.Generic;
using System.Text;
using UGF.EditorTools.Editor.Ids;
using UGF.EditorTools.Editor.IMGUI;
using UGF.EditorTools.Editor.IMGUI.Dropdown;
using UGF.EditorTools.Runtime.Ids;
using UGF.RuntimeTools.Runtime.Options;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UGF.Tables.Editor
{
    public class TableTreeDrawer : DrawerBase
    {
        public SerializedObject SerializedObject { get; }
        public TableTreeOptions Options { get; }
        public TableTreeView TreeView { get; }
        public bool DisplayToolbar { get; set; } = true;
        public bool DisplayFooter { get; set; } = true;

        public event TableTreeViewDrawRowCellHandler DrawRowCellValue;

        private readonly Type m_targetType;
        private readonly Action<SerializedProperty> m_entryInitializeHandler;
        private readonly Func<IEnumerable<DropdownItem<int>>> m_searchSelectionItemsHandler;
        private readonly DropdownSelection<DropdownItem<int>> m_searchSelection = new DropdownSelection<DropdownItem<int>>();
        private readonly List<int> m_selectedIndexes = new List<int>();
        private readonly List<TableTreeViewItem> m_selectedItems = new List<TableTreeViewItem>();
        private SearchField m_search;
        private Styles m_styles;

        private class Styles
        {
            public GUIStyle Toolbar { get; } = EditorStyles.toolbar;
            public GUIStyle Footer { get; } = new GUIStyle("IN Footer");
            public GUIStyle FooterSection { get; } = EditorStyles.toolbarButton;
            public GUIContent FooterClipboardResetButton { get; } = new GUIContent(EditorGUIUtility.FindTexture("Toolbar Minus"), "Reset clipboard.");
            public GUIContent FooterSortingResetButton { get; } = new GUIContent(EditorGUIUtility.FindTexture("Toolbar Minus"), "Reset sorting.");
            public GUIContent FooterSelectionResetButton { get; } = new GUIContent(EditorGUIUtility.FindTexture("Toolbar Minus"), "Clear selection.");
            public GUIContent FooterColumnsResetButton { get; } = new GUIContent(EditorGUIUtility.FindTexture("Toolbar Minus"), "Reset columns.");
            public GUIStyle SearchField { get; } = new GUIStyle("ToolbarSearchTextFieldPopup");
            public GUIStyle SearchButtonCancel { get; } = new GUIStyle("ToolbarSearchCancelButton");
            public GUIStyle SearchButtonCancelEmpty { get; } = new GUIStyle("ToolbarSearchCancelButtonEmpty");
            public GUIContent SearchDropdownContent { get; } = new GUIContent(string.Empty, "Select search column.");
            public GUIContent ReloadButtonContent { get; } = new GUIContent(EditorGUIUtility.FindTexture("Refresh"), "Reload table.");
            public GUIContent AddButtonContent { get; } = new GUIContent(EditorGUIUtility.FindTexture("Toolbar Plus"), "Add new or duplicate selected entries.");
            public GUIContent RemoveButtonContent { get; } = new GUIContent(EditorGUIUtility.FindTexture("Toolbar Minus"), "Delete selected entries.");
            public GUIContent MenuButtonContent { get; } = new GUIContent(EditorGUIUtility.FindTexture("_Menu"));
            public GUIContent MenuReload { get; } = new GUIContent("Reload");
            public GUIContent MenuResetSorting { get; } = new GUIContent("Reset Sorting");
            public GUIContent MenuResetPreferences { get; } = new GUIContent("Reset Preferences");
            public GUIContent MenuResetClipboard { get; } = new GUIContent("Reset Clipboard");
            public GUIContent MenuResetSelection { get; } = new GUIContent("Reset Selection");
            public GUIContent MenuClear { get; } = new GUIContent("Clear");
            public GUIContent MenuContextCopy { get; } = new GUIContent("Copy");
            public GUIContent MenuContextPaste { get; } = new GUIContent("Paste");
            public GUIContent MenuContextColumnCopy { get; } = new GUIContent("Copy Values");
            public GUIContent MenuContextColumnPaste { get; } = new GUIContent("Paste Values");
            public GUIContent AddButtonChildrenContent { get; } = new GUIContent(EditorGUIUtility.FindTexture("Toolbar Plus"), "Add new or duplicate selected children.");
            public GUILayoutOption[] ToolbarButtonOptions { get; } = { GUILayout.Width(50F) };
            public GUILayoutOption[] ToolbarButtonSmallOptions { get; } = { GUILayout.Width(25F) };

            public GUILayoutOption[] TableLayoutOptions { get; } =
            {
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true)
            };

            public GUILayoutOption[] FooterButtonOptions { get; } =
            {
                GUILayout.MinWidth(0F)
            };
        }

        public TableTreeDrawer(SerializedObject serializedObject, TableTreeOptions options)
        {
            SerializedObject = serializedObject ?? throw new ArgumentNullException(nameof(serializedObject));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            TreeView = new TableTreeView(SerializedObject.FindProperty("m_table"), options);

            m_targetType = SerializedObject.targetObject.GetType();
            m_entryInitializeHandler = OnEntryInitialize;
            m_searchSelectionItemsHandler = OnGetSearchSelectionItems;
            m_searchSelection.Dropdown.RootName = "Search Column";
            m_searchSelection.Dropdown.MinimumWidth = 300F;
            m_searchSelection.Dropdown.MinimumHeight = 300F;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Undo.undoRedoPerformed += OnUndoOrRedoPerformed;

            TableTreeEditorState.TryRead(m_targetType, TreeView.State);

            TreeView.DrawRowCell += OnDrawRowCell;
            TreeView.KeyEventProcessing += OnKeyEventProcessing;
            TreeView.ContextMenuClicked += OnContextMenuClicked;
            TreeView.Header.ContextMenuCreating += OnContextMenuHeader;

            TreeView.Reload();
            TreeView.multiColumnHeader.ResizeToFit();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            TableTreeEditorState.Write(m_targetType, TreeView.State);
            TableTreeEditorState.Save();

            Undo.undoRedoPerformed -= OnUndoOrRedoPerformed;

            TreeView.DrawRowCell -= OnDrawRowCell;
            TreeView.KeyEventProcessing -= OnKeyEventProcessing;
            TreeView.ContextMenuClicked -= OnContextMenuClicked;
            TreeView.Header.ContextMenuCreating -= OnContextMenuHeader;
        }

        public void DrawGUILayout()
        {
            m_styles ??= new Styles();
            m_search ??= new SearchField();

            if (DisplayToolbar)
            {
                DrawToolbar();
            }

            DrawTable();

            if (DisplayFooter)
            {
                DrawFooter();
            }

            SerializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                TableTreeEditorState.Write(m_targetType, TreeView.State);
                TableTreeEditorState.Save();
            }
        }

        public void AddEntries(IReadOnlyList<object> values)
        {
            AddEntry(TreeView.PropertyEntries.arraySize, values);
        }

        public void AddEntries(int index, IReadOnlyList<object> values)
        {
            TableTreeEditorInternalUtility.PropertyInsert(TreeView.PropertyEntries, index, values, m_entryInitializeHandler);
        }

        public void AddEntry(object value = null)
        {
            AddEntry(TreeView.PropertyEntries.arraySize, value);
        }

        public void AddEntry(int index, object value = null)
        {
            TableTreeEditorInternalUtility.PropertyInsert(TreeView.PropertyEntries, index, m_entryInitializeHandler, value);
        }

        public void AddChildren(TableTreeViewItem item, IReadOnlyList<object> values)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            AddChildren(item, item.PropertyChildren.arraySize, values);
        }

        public void AddChildren(TableTreeViewItem item, int index, IReadOnlyList<object> values)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            TableTreeEditorInternalUtility.PropertyInsert(item.PropertyChildren, index, values);
        }

        public void AddChild(TableTreeViewItem item, object value = null)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            AddChild(item, item.PropertyChildren.arraySize, value);
        }

        public void AddChild(TableTreeViewItem item, int index, object value = null)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            TableTreeEditorInternalUtility.PropertyInsert(item.PropertyChildren, index, value);
        }

        public void DuplicateEntry(IReadOnlyList<int> indexes)
        {
            TableTreeEditorInternalUtility.PropertyInsert(TreeView.PropertyEntries, indexes, m_entryInitializeHandler);
        }

        public void DuplicateChildren(TableTreeViewItem item, IReadOnlyList<int> indexes)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            TableTreeEditorInternalUtility.PropertyInsert(item.PropertyChildren, indexes);
        }

        public void RemoveEntries(IReadOnlyList<int> indexes)
        {
            TableTreeEditorInternalUtility.PropertyRemove(TreeView.PropertyEntries, indexes);
        }

        public void RemoveEntry(int index)
        {
            TreeView.PropertyEntries.DeleteArrayElementAtIndex(index);
        }

        public void RemoveChildren(TableTreeViewItem item, IReadOnlyList<int> indexes)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            TableTreeEditorInternalUtility.PropertyRemove(item.PropertyChildren, indexes);
        }

        public void RemoveChild(TableTreeViewItem item, int index)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            item.PropertyChildren.DeleteArrayElementAtIndex(index);
        }

        public void PasteEntriesValues(IReadOnlyList<TableTreeViewItem> items, IReadOnlyList<object> values)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (values == null) throw new ArgumentNullException(nameof(values));

            for (int i = 0; i < items.Count; i++)
            {
                TableTreeViewItem item = items[i];

                object value = i < values.Count ? values[i] : values[^1];

                PasteEntryValues(item, value);
            }
        }

        public void PasteEntryValues(TableTreeViewItem item, object value)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (value == null) throw new ArgumentNullException(nameof(value));

            foreach ((TableTreeColumnOptions column, SerializedProperty serializedProperty) in item.ColumnProperties)
            {
                if (column.PropertyName != Options.PropertyIdName
                    && column.PropertyName != Options.PropertyChildrenName)
                {
                    TableTreeEditorClipboardUtility.TrySetPropertyValueFromEntryField(serializedProperty, value);
                }
            }
        }

        public void PasteEntryValues(TableTreeColumnOptions column, IReadOnlyList<TableTreeViewItem> items, IReadOnlyList<object> values)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (values == null) throw new ArgumentNullException(nameof(values));

            for (int i = 0; i < items.Count; i++)
            {
                TableTreeViewItem item = items[i];

                object value = i < values.Count ? values[i] : values[^1];

                PasteEntryValues(column, item, value);
            }
        }

        public void PasteEntryValues(TableTreeColumnOptions column, TableTreeViewItem item, object value)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (value == null) throw new ArgumentNullException(nameof(value));

            TableTreeEditorClipboardUtility.TrySetPropertyValueFromEntryField(item.ColumnProperties[column], value);
        }

        public void Apply()
        {
            TreeView.Apply();
        }

        public void Reload()
        {
            TreeView.Revert();
        }

        protected void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(m_styles.Toolbar))
            {
                if (GUILayout.Button(m_styles.ReloadButtonContent, EditorStyles.toolbarButton))
                {
                    Reload();
                }

                if (GUILayout.Button(m_styles.AddButtonContent, EditorStyles.toolbarButton, m_styles.ToolbarButtonOptions))
                {
                    OnEntryAdd();
                }

                using (new EditorGUI.DisabledScope(!TreeView.HasSelected()))
                {
                    if (GUILayout.Button(m_styles.RemoveButtonContent, EditorStyles.toolbarButton, m_styles.ToolbarButtonOptions))
                    {
                        OnEntryRemove();
                    }
                }

                OnDrawToolbar();
                OnDrawSearch();

                Rect rectMenu = GUILayoutUtility.GetRect(m_styles.MenuButtonContent, EditorStyles.toolbarButton, m_styles.ToolbarButtonSmallOptions);

                if (GUI.Button(rectMenu, m_styles.MenuButtonContent, EditorStyles.toolbarButton))
                {
                    OnMenuOpen(rectMenu);
                }
            }
        }

        protected void DrawTable()
        {
            Rect position;

            using (var scope = new EditorGUILayout.VerticalScope(GUIStyle.none, m_styles.TableLayoutOptions))
            {
                position = scope.rect;
            }

            TreeView.OnGUI(position);

            OnDrawTable();
        }

        protected void DrawFooter()
        {
            using (new EditorGUILayout.HorizontalScope(m_styles.Footer))
            {
                string path = AssetDatabase.GetAssetPath(SerializedObject.targetObject);
                int columnsVisible = TreeView.multiColumnHeader.state.visibleColumns.Length;
                int columnsTotal = TreeView.multiColumnHeader.state.columns.Length;
                int countVisible = TreeView.VisibleEntryCount;
                int countTotal = TreeView.PropertyEntries.arraySize;

                if (GUILayout.Button($"Path: {path}", m_styles.FooterSection, m_styles.FooterButtonOptions))
                {
                    EditorGUIUtility.PingObject(SerializedObject.targetObject);
                }

                OnDrawFooter();

                if (TableTreeEditorClipboard.HasAny() && TableTreeEditorClipboard.HasMatch(m_targetType))
                {
                    var builder = new StringBuilder("Clipboard:");

                    if (TableTreeEditorClipboard.EntriesCount > 0)
                    {
                        builder.Append($" Entries {TableTreeEditorClipboard.EntriesCount}");
                    }

                    if (TableTreeEditorClipboard.ChildrenCount > 0)
                    {
                        builder.Append($" Children {TableTreeEditorClipboard.ChildrenCount}");
                    }

                    using (new EditorGUILayout.HorizontalScope(m_styles.FooterSection))
                    {
                        GUILayout.Label(builder.ToString(), m_styles.FooterButtonOptions);

                        if (GUILayout.Button(m_styles.FooterClipboardResetButton, EditorStyles.iconButton))
                        {
                            TableTreeEditorClipboard.Clear();
                            TableTreeEditorClipboard.Save();
                        }
                    }
                }

                if (TreeView.HasSortColumn)
                {
                    using (new EditorGUILayout.HorizontalScope(m_styles.FooterSection))
                    {
                        GUILayout.Label($"Sorting Column: {TreeView.SortColumn.DisplayName}", m_styles.FooterButtonOptions);

                        if (GUILayout.Button(m_styles.FooterSortingResetButton, EditorStyles.iconButton))
                        {
                            TreeView.ClearSorting();
                        }
                    }
                }

                if (TreeView.HasSelected())
                {
                    int entryCount = TreeView.GetSelectedCount(TableTreeEntryType.Entry);
                    int childCount = TreeView.GetSelectedCount(TableTreeEntryType.Child);

                    var builder = new StringBuilder("Selected:");

                    if (entryCount > 0)
                    {
                        builder.Append($" Entries {entryCount}");
                    }

                    if (childCount > 0)
                    {
                        builder.Append($" Children {childCount}");
                    }

                    using (new EditorGUILayout.HorizontalScope(m_styles.FooterSection))
                    {
                        GUILayout.Label(builder.ToString(), m_styles.FooterButtonOptions);

                        if (GUILayout.Button(m_styles.FooterSelectionResetButton, EditorStyles.iconButton))
                        {
                            TreeView.ClearSelection();
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope(m_styles.FooterSection))
                {
                    GUILayout.Label(columnsVisible == columnsTotal
                        ? $"Columns: {columnsTotal}"
                        : $"Columns: {columnsVisible}/{columnsTotal}");

                    if (columnsVisible != columnsTotal)
                    {
                        if (GUILayout.Button(m_styles.FooterColumnsResetButton, EditorStyles.iconButton))
                        {
                            TreeView.ResetColumns();
                        }
                    }
                }

                GUILayout.Label(countVisible == countTotal
                    ? $"Entries: {countTotal}"
                    : $"Entries: {countVisible}/{countTotal}", m_styles.FooterSection);
            }
        }

        protected virtual void OnDrawToolbar()
        {
            GUILayout.FlexibleSpace();
        }

        protected virtual void OnDrawTable()
        {
        }

        protected virtual void OnDrawFooter()
        {
            GUILayout.FlexibleSpace();
        }

        protected virtual void OnDrawRowCell(Rect position, TableTreeViewItem item, SerializedProperty serializedProperty, TableTreeColumnOptions column)
        {
            if (serializedProperty.isArray && serializedProperty.propertyType == SerializedPropertyType.Generic)
            {
                if (serializedProperty.name == Options.PropertyChildrenName)
                {
                    OnDrawRowCellChildren(position, item, serializedProperty, column);
                }
                else
                {
                    OnDrawRowCellArray(position, item, serializedProperty, column);
                }
            }
            else
            {
                OnDrawRowCellValue(position, item, serializedProperty, column);
            }
        }

        protected virtual void OnDrawRowCellValue(Rect position, TableTreeViewItem item, SerializedProperty serializedProperty, TableTreeColumnOptions column)
        {
            if (DrawRowCellValue != null)
            {
                DrawRowCellValue.Invoke(position, item, serializedProperty, column);
            }
            else
            {
                position.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.PropertyField(position, serializedProperty, GUIContent.none, false);
            }
        }

        protected virtual void OnDrawRowCellArray(Rect position, TableTreeViewItem item, SerializedProperty serializedProperty, TableTreeColumnOptions column)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.IntField(position, GUIContent.none, serializedProperty.arraySize);
            }
        }

        protected virtual void OnDrawRowCellChildren(Rect position, TableTreeViewItem item, SerializedProperty serializedProperty, TableTreeColumnOptions column)
        {
            float height = EditorGUIUtility.singleLineHeight;
            float space = EditorGUIUtility.standardVerticalSpacing;

            var rectField = new Rect(position.x, position.y, position.width - height - space, height);
            var rectButton = new Rect(rectField.xMax + space, position.y + 1F, height, height);

            int count = item.PropertyChildrenSize.intValue;

            EditorGUI.PropertyField(rectField, item.PropertyChildrenSize, GUIContent.none);

            if (count != item.PropertyChildrenSize.intValue)
            {
                TreeView.Reload();
                GUIUtility.ExitGUI();
            }

            if (GUI.Button(rectButton, m_styles.AddButtonChildrenContent, EditorStyles.iconButton))
            {
                OnEntryAddChildren(item);
            }
        }

        protected virtual void OnMenuCreate(GenericMenu menu)
        {
        }

        protected virtual void OnContextMenuHeaderCreate(GenericMenu menu, Optional<TableTreeColumnOptions> column)
        {
        }

        protected virtual void OnContextMenuCreate(GenericMenu menu)
        {
        }

        private void OnDrawSearch()
        {
            float height = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect position = GUILayoutUtility.GetRect(300F, height);

            position.xMin += spacing;
            position.xMax -= spacing;
            position.yMin += spacing;

            var rectButton = new Rect(position.x, position.y, position.height, position.height);
            var rectLabel = new Rect(rectButton.xMax + spacing, position.y, position.width - rectButton.width - spacing, position.height);

            if (DropdownEditorGUIUtility.Dropdown(rectButton, GUIContent.none, m_styles.SearchDropdownContent, m_searchSelection, m_searchSelectionItemsHandler, out DropdownItem<int> selected, FocusType.Keyboard, GUIStyle.none))
            {
                TreeView.SearchColumnIndex = selected.Value;
            }

            TreeView.searchString = m_search.OnGUI(position, TreeView.searchString, m_styles.SearchField, m_styles.SearchButtonCancel, m_styles.SearchButtonCancelEmpty);

            if (string.IsNullOrEmpty(TreeView.searchString) && !m_search.HasFocus())
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    GUI.Label(rectLabel, TreeView.SearchColumn.DisplayName, EditorStyles.miniLabel);
                }
            }
        }

        private IEnumerable<DropdownItem<int>> OnGetSearchSelectionItems()
        {
            var items = new List<DropdownItem<int>>();

            for (int i = 0; i < Options.Columns.Count; i++)
            {
                TableTreeColumnOptions column = Options.Columns[i];

                items.Add(new DropdownItem<int>(column.DisplayName, i)
                {
                    Priority = Options.Columns.Count - i
                });
            }

            return items;
        }

        private void OnEntryAdd()
        {
            TreeView.GetChildrenParentSelection(m_selectedItems);

            foreach (TableTreeViewItem item in m_selectedItems)
            {
                TreeView.GetChildrenSelectionIndexes(item, m_selectedIndexes);

                DuplicateChildren(item, m_selectedIndexes);

                m_selectedIndexes.Clear();
            }

            TreeView.GetSelectionIndexes(m_selectedIndexes);

            if (m_selectedIndexes.Count > 0)
            {
                DuplicateEntry(m_selectedIndexes);
            }
            else if (!TreeView.HasSelected())
            {
                AddEntry();
            }

            m_selectedIndexes.Clear();
            m_selectedItems.Clear();

            TreeView.Apply();
        }

        private void OnEntryAddChildren(TableTreeViewItem item)
        {
            TreeView.GetChildrenSelectionIndexes(item, m_selectedIndexes);

            if (m_selectedIndexes.Count > 0)
            {
                DuplicateChildren(item, m_selectedIndexes);

                m_selectedIndexes.Clear();
            }
            else
            {
                AddChild(item);
            }

            TreeView.Apply();
            TreeView.SetExpanded(item.id, true);
        }

        private void OnEntryRemove()
        {
            TreeView.GetChildrenParentSelection(m_selectedItems);

            foreach (TableTreeViewItem item in m_selectedItems)
            {
                TreeView.GetChildrenSelectionIndexes(item, m_selectedIndexes);

                RemoveChildren(item, m_selectedIndexes);

                m_selectedIndexes.Clear();
            }

            TreeView.GetSelectionIndexes(m_selectedIndexes);

            if (m_selectedIndexes.Count > 0)
            {
                RemoveEntries(m_selectedIndexes);
            }

            m_selectedIndexes.Clear();
            m_selectedItems.Clear();

            TreeView.Apply();
        }

        private void OnEntryCopy()
        {
            TableTreeEditorClipboard.Clear();

            TreeView.GetSelection(m_selectedItems, TableTreeEntryType.Entry);

            TableTreeEditorClipboard.TryCopyEntries(m_selectedItems);

            m_selectedItems.Clear();

            TreeView.GetSelection(m_selectedItems, TableTreeEntryType.Child);

            TableTreeEditorClipboard.TryCopyChildren(m_selectedItems);

            m_selectedItems.Clear();

            if (TableTreeEditorClipboard.HasAny())
            {
                TableTreeEditorClipboard.CopyType(m_targetType);
                TableTreeEditorClipboard.Save();
            }
        }

        private void OnEntryPaste()
        {
            if (TableTreeEditorClipboard.HasAny() && TableTreeEditorClipboard.HasMatch(m_targetType))
            {
                TableTreeEditorClipboardData clipboard = TableTreeEditorClipboard.GetData();

                if (clipboard.Children.Count > 0)
                {
                    TreeView.GetSelection(m_selectedItems, TableTreeEntryType.Child);

                    if (m_selectedItems.Count > 0)
                    {
                        for (int i = 0; i < m_selectedItems.Count; i++)
                        {
                            TableTreeViewItem item = m_selectedItems[i];

                            var parent = (TableTreeViewItem)item.parent;

                            AddChildren(parent, item.Index, clipboard.Children);
                        }

                        m_selectedItems.Clear();
                    }

                    TreeView.GetSelection(m_selectedItems, TableTreeEntryType.Entry);

                    if (m_selectedItems.Count > 0)
                    {
                        for (int i = 0; i < m_selectedItems.Count; i++)
                        {
                            TableTreeViewItem item = m_selectedItems[i];

                            AddChildren(item, clipboard.Children);

                            TreeView.SetExpanded(item.id, true);
                        }

                        m_selectedItems.Clear();
                    }
                }

                if (clipboard.Entries.Count > 0)
                {
                    TreeView.GetSelection(m_selectedItems, TableTreeEntryType.Entry);

                    if (m_selectedItems.Count > 0)
                    {
                        for (int i = 0; i < m_selectedItems.Count; i++)
                        {
                            TableTreeViewItem item = m_selectedItems[i];

                            AddEntries(item.Index, clipboard.Entries);
                        }
                    }
                    else
                    {
                        int index = TreeView.PropertyEntries.arraySize;

                        AddEntries(index, clipboard.Entries);
                    }

                    m_selectedItems.Clear();
                }

                TreeView.Apply();
            }
        }

        private void OnEntryPasteValues()
        {
            OnEntryPasteValues(TableTreeEntryType.Entry);
            OnEntryPasteValues(TableTreeEntryType.Child);
        }

        private void OnEntryPasteValues(TableTreeEntryType entryType)
        {
            if (TableTreeEditorClipboard.HasAny() && TableTreeEditorClipboard.HasMatch(m_targetType))
            {
                TableTreeEditorClipboardData clipboard = TableTreeEditorClipboard.GetData();

                List<object> entries = entryType switch
                {
                    TableTreeEntryType.Entry => clipboard.Entries,
                    TableTreeEntryType.Child => clipboard.Children,
                    _ => throw new ArgumentOutOfRangeException(nameof(entryType), entryType, "Table tree column entry type is unknown.")
                };

                if (entries.Count > 0)
                {
                    if (TreeView.HasSelected())
                    {
                        TreeView.GetSelection(m_selectedItems, entryType);
                    }
                    else
                    {
                        TreeView.GetItems(m_selectedItems, entryType);
                    }

                    PasteEntriesValues(m_selectedItems, entries);

                    m_selectedItems.Clear();
                }

                TreeView.Apply();
            }
        }

        private void OnEntryPasteValues(TableTreeColumnOptions column)
        {
            if (TableTreeEditorClipboard.HasAny() && TableTreeEditorClipboard.HasMatch(m_targetType))
            {
                TableTreeEditorClipboardData clipboard = TableTreeEditorClipboard.GetData();

                List<object> entries = column.EntryType switch
                {
                    TableTreeEntryType.Entry => clipboard.Entries,
                    TableTreeEntryType.Child => clipboard.Children,
                    _ => throw new ArgumentOutOfRangeException(nameof(column.EntryType), column.EntryType, "Table tree column entry type is unknown.")
                };

                if (entries.Count > 0)
                {
                    if (TreeView.HasSelected())
                    {
                        TreeView.GetSelection(m_selectedItems, column.EntryType);
                    }
                    else
                    {
                        TreeView.GetItems(m_selectedItems, column.EntryType);
                    }

                    PasteEntryValues(column, m_selectedItems, entries);

                    m_selectedItems.Clear();
                }

                TreeView.Apply();
            }
        }

        private void OnMenuOpen(Rect position)
        {
            var menu = new GenericMenu();

            OnMenuCreate(menu);
            OnMenuResetSetup(menu);

            menu.AddSeparator(string.Empty);

            OnMenuClearSetup(menu);

            menu.DropDown(position);
        }

        private void OnKeyEventProcessing()
        {
            Event current = Event.current;

            if (current.type == EventType.ValidateCommand)
            {
                if (current.commandName is "SoftDelete" or "Copy" or "Paste" or "Duplicate")
                {
                    current.Use();
                }
            }

            if (current.type == EventType.ExecuteCommand)
            {
                if (current.commandName == "SoftDelete")
                {
                    OnEntryRemove();

                    current.Use();
                }

                if (current.commandName == "Copy")
                {
                    OnEntryCopy();

                    current.Use();
                }

                if (current.commandName == "Paste")
                {
                    OnEntryPaste();

                    current.Use();
                }

                if (current.commandName == "Duplicate")
                {
                    OnEntryAdd();

                    current.Use();
                }
            }

            if (current.control && current.type == EventType.KeyUp && current.keyCode == KeyCode.R)
            {
                Reload();
            }
        }

        private void OnContextMenuClicked()
        {
            var menu = new GenericMenu();

            if (TreeView.HasSelected())
            {
                menu.AddItem(m_styles.MenuContextCopy, false, OnEntryCopy);
            }
            else
            {
                menu.AddDisabledItem(m_styles.MenuContextCopy);
            }

            if (TableTreeEditorClipboard.HasAny() && TableTreeEditorClipboard.HasMatch(m_targetType))
            {
                menu.AddItem(m_styles.MenuContextPaste, false, OnEntryPaste);

                if (TreeView.HasSelected())
                {
                    menu.AddItem(m_styles.MenuContextColumnPaste, false, OnEntryPasteValues);
                }
                else
                {
                    menu.AddDisabledItem(m_styles.MenuContextColumnPaste);
                }
            }
            else
            {
                menu.AddDisabledItem(m_styles.MenuContextPaste);
                menu.AddDisabledItem(m_styles.MenuContextColumnPaste);
            }

            OnContextMenuCreate(menu);

            menu.AddSeparator(string.Empty);

            OnMenuResetSetup(menu);

            menu.AddSeparator(string.Empty);

            OnMenuClearSetup(menu);

            menu.ShowAsContext();
        }

        private void OnContextMenuHeader(GenericMenu menu, Optional<TableTreeColumnOptions> column)
        {
            menu.AddSeparator(string.Empty);

            if (TreeView.HasSelected())
            {
                menu.AddItem(m_styles.MenuContextColumnCopy, false, OnEntryCopy);
            }
            else
            {
                menu.AddDisabledItem(m_styles.MenuContextColumnCopy);
            }

            if (column.HasValue
                && column.Value.PropertyName != Options.PropertyIdName
                && column.Value.PropertyName != Options.PropertyChildrenName
                && TableTreeEditorClipboard.HasAny()
                && TableTreeEditorClipboard.HasMatch(m_targetType))
            {
                TableTreeEditorClipboardData clipboard = TableTreeEditorClipboard.GetData();

                bool hasEntries = column.Value.EntryType switch
                {
                    TableTreeEntryType.Entry => clipboard.Entries.Count > 0,
                    TableTreeEntryType.Child => clipboard.Children.Count > 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(column.Value.EntryType), column.Value.EntryType, "Table tree column entry type is unknown.")
                };

                if (hasEntries)
                {
                    menu.AddItem(m_styles.MenuContextColumnPaste, false, () => OnEntryPasteValues(column));
                }
                else
                {
                    menu.AddDisabledItem(m_styles.MenuContextColumnPaste);
                }
            }
            else
            {
                menu.AddDisabledItem(m_styles.MenuContextColumnPaste);
            }

            OnContextMenuHeaderCreate(menu, column);

            menu.AddSeparator(string.Empty);

            TreeView.Header.CreateContextMenu(menu);
        }

        private void OnUndoOrRedoPerformed()
        {
            TreeView.SerializedProperty.serializedObject.Update();
            TreeView.Reload();
        }

        private void OnEntryInitialize(SerializedProperty serializedProperty)
        {
            SerializedProperty propertyId = serializedProperty.FindPropertyRelative(Options.PropertyIdName);
            SerializedProperty propertyName = serializedProperty.FindPropertyRelative(Options.PropertyNameName);
            string entryName = propertyName.stringValue;

            GlobalIdEditorUtility.SetGlobalIdToProperty(propertyId, GlobalId.Generate());

            propertyName.stringValue = OnGetUniqueName(!string.IsNullOrEmpty(entryName) ? entryName : "Entry");
        }

        private string OnGetUniqueName(string entryName)
        {
            string[] names = new string[TreeView.PropertyEntries.arraySize];

            for (int i = 0; i < TreeView.PropertyEntries.arraySize; i++)
            {
                SerializedProperty propertyEntry = TreeView.PropertyEntries.GetArrayElementAtIndex(i);
                SerializedProperty propertyName = propertyEntry.FindPropertyRelative(Options.PropertyNameName);

                names[i] = propertyName.stringValue;
            }

            return ObjectNames.GetUniqueName(names, entryName);
        }

        private void OnMenuResetSetup(GenericMenu menu)
        {
            menu.AddItem(m_styles.MenuReload, false, Reload);

            if (TreeView.HasSelected())
            {
                menu.AddItem(m_styles.MenuResetSelection, false, TreeView.ClearSelection);
            }
            else
            {
                menu.AddDisabledItem(m_styles.MenuResetSelection);
            }

            if (TreeView.HasSortColumn)
            {
                menu.AddItem(m_styles.MenuResetSorting, false, TreeView.ClearSorting);
            }
            else
            {
                menu.AddDisabledItem(m_styles.MenuResetSorting);
            }

            if (TableTreeEditorState.Has(m_targetType))
            {
                menu.AddItem(m_styles.MenuResetPreferences, false, () =>
                {
                    TableTreeEditorState.TryReset(m_targetType, Options);
                    TableTreeEditorState.Save();
                    TableTreeEditorState.TryRead(m_targetType, TreeView.State);

                    TreeView.multiColumnHeader.ResizeToFit();
                });
            }
            else
            {
                menu.AddDisabledItem(m_styles.MenuResetPreferences);
            }

            if (TableTreeEditorClipboard.HasAny())
            {
                menu.AddItem(m_styles.MenuResetClipboard, false, () =>
                {
                    TableTreeEditorClipboard.Clear();
                    TableTreeEditorClipboard.Save();
                });
            }
            else
            {
                menu.AddDisabledItem(m_styles.MenuResetClipboard);
            }
        }

        private void OnMenuClearSetup(GenericMenu menu)
        {
            if (TreeView != null && TreeView.PropertyEntries.arraySize > 0)
            {
                menu.AddItem(m_styles.MenuClear, false, () =>
                {
                    TreeView.PropertyEntries.ClearArray();
                    TreeView.Apply();
                });
            }
            else
            {
                menu.AddDisabledItem(m_styles.MenuClear);
            }
        }
    }
}
