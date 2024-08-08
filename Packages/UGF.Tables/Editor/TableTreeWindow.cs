using System;
using UGF.RuntimeTools.Runtime.Tables;
using UnityEditor;
using UnityEngine;

namespace UGF.Tables.Editor
{
    public class TableTreeWindow : EditorWindow
    {
        [SerializeField] private string m_assetId;

        public SerializedObject SerializedObject { get { return m_serializedObject ?? throw new ArgumentException("Value not specified."); } }
        public bool HasSerializedObject { get { return m_serializedObject != null; } }
        public TableTreeDrawer Drawer { get { return m_drawer ?? throw new ArgumentException("Value not specified."); } }

        private SerializedObject m_serializedObject;
        private TableTreeDrawer m_drawer;

        protected virtual void OnEnable()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TableAsset>(AssetDatabase.GUIDToAssetPath(m_assetId));

            if (asset != null)
            {
                SetTarget(asset);
            }
        }

        protected virtual void OnDisable()
        {
            m_drawer?.Disable();
            m_drawer = null;
            m_serializedObject?.Dispose();
            m_serializedObject = null;
        }

        protected virtual void OnGUI()
        {
            if (m_serializedObject?.targetObject == null)
            {
                m_serializedObject = null;
                m_drawer = null;
            }

            m_drawer?.DrawGUILayout();
        }

        protected virtual TableTreeDrawer OnCreateDrawer(SerializedObject serializedObject)
        {
            return new TableTreeDrawer(serializedObject, CreateOptions(serializedObject));
        }

        protected TableTreeOptions CreateOptions(SerializedObject serializedObject)
        {
            return TableTreeEditorUtility.CreateOptions((TableAsset)serializedObject.targetObject);
        }

        public void SetTarget(TableAsset asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            m_serializedObject = new SerializedObject(asset);
            m_assetId = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            m_drawer?.Disable();
            m_drawer = OnCreateDrawer(m_serializedObject);
            m_drawer.Enable();
        }

        public void ClearTarget()
        {
            m_assetId = string.Empty;
            m_drawer?.Disable();
            m_drawer = null;
        }
    }
}
