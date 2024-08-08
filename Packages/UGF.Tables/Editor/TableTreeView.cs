using System;
using System.Collections.Generic;
using UGF.EditorTools.Runtime.Ids;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UGF.Tables.Editor
{
    public class TableTreeView : TreeView
    {
        public SerializedProperty SerializedProperty { get; }
        public TableTreeOptions Options { get; }
        public SerializedProperty PropertyEntries { get; }
        public TableTreeViewState State { get; }
        public int SearchColumnIndex { get { return State.SearchColumnIndex; } set { State.SearchColumnIndex = value; } }
        public TableTreeColumnOptions SearchColumn { get { return HasSearchColumn ? Options.Columns[State.SearchColumnIndex] : throw new ArgumentException("Value not specified."); } }
        public bool HasSearchColumn { get { return State.SearchColumnIndex >= 0 && State.SearchColumnIndex < Options.Columns.Count; } }
        public TableTreeColumnOptions SortColumn { get { return HasSortColumn ? Options.Columns[Header.sortedColumnIndex] : throw new ArgumentException("Value not specified."); } }
        public bool HasSortColumn { get { return Header.sortedColumnIndex >= 0 && Header.sortedColumnIndex < Options.Columns.Count; } }
        public int ItemsCount { get { return m_items.Count; } }
        public int VisibleCount { get; private set; }
        public int ColumnCount { get { return Header.state.columns.Length; } }
        public int ColumnVisibleCount { get { return Header.state.visibleColumns.Length; } }
        public TableTreeViewHeader Header { get { return (TableTreeViewHeader)multiColumnHeader; } }

        public event TableTreeViewDrawRowCellHandler DrawRowCell;
        public event Action DrawRowsBefore;
        public event Action DrawRowsAfter;
        public event Action KeyEventProcessing;
        public event Action ContextMenuClicked;
        public event TableTreeViewContextMenuItemHandler ContextMenuItemClicked;

        private readonly TableTreeViewItemComparer m_comparer = new TableTreeViewItemComparer();
        private readonly Dictionary<int, TableTreeViewItem> m_items;
        private readonly List<TreeViewItem> m_rows;

        public TableTreeView(SerializedProperty serializedProperty, TableTreeOptions options) : this(serializedProperty, options, TableTreeEditorUtility.CreateState(options))
        {
        }

        public TableTreeView(SerializedProperty serializedProperty, TableTreeOptions options, TableTreeViewState state) : base(state, new TableTreeViewHeader(state.Header, options))
        {
            SerializedProperty = serializedProperty ?? throw new ArgumentNullException(nameof(serializedProperty));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            PropertyEntries = SerializedProperty.FindPropertyRelative(Options.PropertyEntriesName);
            State = state;

            m_items = new Dictionary<int, TableTreeViewItem>(PropertyEntries.arraySize);
            m_rows = new List<TreeViewItem>(PropertyEntries.arraySize);

            cellMargin = EditorGUIUtility.standardVerticalSpacing;
            rowHeight = options.RowHeight;
            showAlternatingRowBackgrounds = true;
            enableItemHovering = true;

            Header.sortingChanged += OnSortingChanged;
        }

        protected override TreeViewItem BuildRoot()
        {
            m_items.Clear();

            var root = new TreeViewItem(0, -1);

            for (int i = 0; i < PropertyEntries.arraySize; i++)
            {
                SerializedProperty propertyElement = PropertyEntries.GetArrayElementAtIndex(i);
                int id = TableTreeEditorInternalUtility.GetEntryId(propertyElement, Options);

                var item = new TableTreeViewItem(id, i, propertyElement, Options);

                root.AddChild(item);

                if (!m_items.TryAdd(item.id, item))
                {
                    item.id = HashCode.Combine(item.id, i);

                    m_items.Add(item.id, item);
                }
            }

            if (root.hasChildren)
            {
                if (HasSortColumn)
                {
                    MultiColumnHeaderState.Column columnState = Header.GetColumn(Header.sortedColumnIndex);

                    m_comparer.SetColumn(SortColumn, columnState.sortedAscending);

                    OnSort(root);

                    m_comparer.ClearColumn();
                }

                SetupDepthsFromParentsAndChildren(root);
            }

            return root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            m_rows.Clear();

            IList<TreeViewItem> items;

            if (root.hasChildren)
            {
                if (hasSearch)
                {
                    OnSearchRows(root, m_rows, searchString);

                    items = m_rows;
                }
                else
                {
                    items = base.BuildRows(root);
                }
            }
            else
            {
                items = ArraySegment<TreeViewItem>.Empty;
            }

            VisibleCount = items.Count;

            return items;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var rowItem = (TableTreeViewItem)args.item;
            int count = args.GetNumVisibleColumns();
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            for (int i = 0; i < count; i++)
            {
                int columnIndex = args.GetColumn(i);

                if (columnIndex >= 0 && columnIndex < Options.Columns.Count)
                {
                    TableTreeColumnOptions column = Options.Columns[columnIndex];

                    if (rowItem.ColumnProperties.TryGetValue(column, out SerializedProperty serializedProperty))
                    {
                        Rect position = args.GetCellRect(i);

                        position.yMin += spacing;
                        position.yMax -= spacing;

                        if (columnIndex == columnIndexForTreeFoldouts)
                        {
                            position.xMin += foldoutWidth + spacing * 3F;
                            position.xMax -= spacing;
                        }

                        if (DrawRowCell != null)
                        {
                            DrawRowCell?.Invoke(position, rowItem, serializedProperty, column);
                        }
                        else
                        {
                            EditorGUI.PropertyField(position, serializedProperty, GUIContent.none, false);
                        }
                    }
                }
            }
        }

        protected override void BeforeRowsGUI()
        {
            base.BeforeRowsGUI();

            DrawRowsBefore?.Invoke();
        }

        protected override void AfterRowsGUI()
        {
            base.AfterRowsGUI();

            DrawRowsAfter?.Invoke();
        }

        protected override void KeyEvent()
        {
            KeyEventProcessing?.Invoke();
        }

        protected override void ContextClickedItem(int id)
        {
            base.ContextClickedItem(id);

            TableTreeViewItem item = GetItem(id);

            ContextMenuItemClicked?.Invoke(item);
        }

        protected override void ContextClicked()
        {
            base.ContextClicked();

            ContextMenuClicked?.Invoke();
        }

        public void Revert()
        {
            SerializedProperty.serializedObject.Update();
            Reload();
        }

        public void Apply()
        {
            SerializedProperty.serializedObject.ApplyModifiedProperties();
            Reload();
        }

        public bool HasSelected()
        {
            for (int i = 0; i < state.selectedIDs.Count; i++)
            {
                int id = state.selectedIDs[i];

                if (TryGetItem(id, out _))
                {
                    return true;
                }
            }

            return false;
        }

        public int GetSelectedCount()
        {
            int count = 0;

            for (int i = 0; i < state.selectedIDs.Count; i++)
            {
                int id = state.selectedIDs[i];

                if (TryGetItem(id, out _))
                {
                    count++;
                }
            }

            return count;
        }

        public void GetSelectionIndexes(ICollection<int> indexes)
        {
            if (indexes == null) throw new ArgumentNullException(nameof(indexes));

            for (int i = 0; i < state.selectedIDs.Count; i++)
            {
                int id = state.selectedIDs[i];

                if (TryGetItem(id, out TableTreeViewItem item))
                {
                    indexes.Add(item.Index);
                }
            }
        }

        public void GetSelection(ICollection<TableTreeViewItem> selection)
        {
            if (selection == null) throw new ArgumentNullException(nameof(selection));

            for (int i = 0; i < state.selectedIDs.Count; i++)
            {
                int id = state.selectedIDs[i];

                if (TryGetItem(id, out TableTreeViewItem item))
                {
                    selection.Add(item);
                }
            }
        }

        public void ClearSelection()
        {
            SetSelection(ArraySegment<int>.Empty);
        }

        public void ClearSorting()
        {
            Header.sortedColumnIndex = -1;

            Reload();
        }

        public void ResetColumns()
        {
            int[] columns = new int[Header.state.columns.Length];

            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = i;
            }

            Header.state.visibleColumns = columns;
        }

        public bool TryFocusAtItem(GlobalId entryId)
        {
            if (!entryId.IsValid()) throw new ArgumentException("Value should be valid.", nameof(entryId));

            if (TryGetItemByEntryId(entryId, out TableTreeViewItem item))
            {
                state.selectedIDs.Clear();
                state.selectedIDs.Add(item.id);

                SetSelection(state.selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
                return true;
            }

            return false;
        }

        public TableTreeViewItem GetItem(int id)
        {
            return TryGetItem(id, out TableTreeViewItem item) ? item : throw new ArgumentException($"Table tree view item not found by the specified id: '{id}'.");
        }

        public bool TryGetItem(int id, out TableTreeViewItem item)
        {
            return m_items.TryGetValue(id, out item);
        }

        public bool TryGetItemByEntryId(GlobalId id, out TableTreeViewItem item)
        {
            if (!id.IsValid()) throw new ArgumentException("Value should be valid.", nameof(id));

            foreach ((_, TableTreeViewItem value) in m_items)
            {
                SerializedProperty propertyId = value.SerializedProperty.FindPropertyRelative(Options.PropertyIdName);
                GlobalId entryId = propertyId.hash128Value;

                if (entryId == id)
                {
                    item = value;
                    return true;
                }
            }

            item = default;
            return false;
        }

        public void GetItems(ICollection<TableTreeViewItem> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach ((_, TableTreeViewItem item) in m_items)
            {
                items.Add(item);
            }
        }

        private void OnSearchRows(TreeViewItem root, ICollection<TreeViewItem> rows, string search)
        {
            for (int i = 0; i < root.children.Count; i++)
            {
                var item = (TableTreeViewItem)root.children[i];
                bool itemMatch = false;

                if (OnSearch(item, search))
                {
                    rows.Add(item);
                    itemMatch = true;
                }

                if (item.hasChildren)
                {
                    bool itemAdded = false;

                    for (int c = 0; c < item.children.Count; c++)
                    {
                        var child = (TableTreeViewItem)item.children[c];

                        if (itemMatch || OnSearch(child, search))
                        {
                            if (!itemMatch && !itemAdded)
                            {
                                rows.Add(item);
                                itemAdded = true;
                            }

                            rows.Add(child);
                        }
                    }
                }
            }
        }

        private bool OnSearch(TableTreeViewItem item, string search)
        {
            TableTreeColumnOptions column = SearchColumn;

            if (item.ColumnProperties.TryGetValue(column, out SerializedProperty serializedProperty))
            {
                ITableTreeColumnSearcher searcher = column.Searcher ?? TableTreeColumnSearcher.Default;

                return searcher.Check(serializedProperty, search);
            }

            return false;
        }

        private void OnSort(TreeViewItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (item.hasChildren)
            {
                item.children.Sort(m_comparer);

                for (int i = 0; i < item.children.Count; i++)
                {
                    TreeViewItem child = item.children[i];

                    OnSort(child);
                }
            }
        }

        private void OnSortingChanged(MultiColumnHeader header)
        {
            Reload();
        }
    }
}
