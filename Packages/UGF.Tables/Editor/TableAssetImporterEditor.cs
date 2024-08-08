using UGF.EditorTools.Editor.IMGUI;
using UGF.EditorTools.Editor.IMGUI.Scopes;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace UGF.Tables.Editor
{
    [CustomEditor(typeof(TableAssetImporter), true)]
    public class TableAssetImporterEditor : ScriptedImporterEditor
    {
        public override bool showImportedObject { get; } = false;

        private SerializedProperty m_propertyTable;
        private EditorObjectReferenceDrawer m_drawerTable;

        public override void OnEnable()
        {
            base.OnEnable();

            m_propertyTable = serializedObject.FindProperty("m_table");

            m_drawerTable = new EditorObjectReferenceDrawer(serializedObject.FindProperty("m_table"))
            {
                Drawer = { DisplayTitlebar = true }
            };

            m_drawerTable.Enable();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            m_drawerTable.Disable();
        }

        public override void OnInspectorGUI()
        {
            using (new SerializedObjectUpdateScope(serializedObject))
            {
                EditorIMGUIUtility.DrawScriptProperty(serializedObject);

                EditorGUILayout.PropertyField(m_propertyTable);

                OnDrawProperties();
            }

            ApplyRevertGUI();

            EditorGUILayout.Space();

            m_drawerTable.DrawGUILayout();
        }

        protected override bool OnApplyRevertGUI()
        {
            var importer = (TableAssetImporter)serializedObject.targetObject;

            using (new EditorGUI.DisabledScope(!importer.IsValid()))
            {
                using (new EditorGUI.DisabledScope(!importer.CanImport))
                {
                    if (GUILayout.Button("Import", GUILayout.Width(75F)))
                    {
                        importer.Import();
                    }
                }

                using (new EditorGUI.DisabledScope(!importer.CanExport))
                {
                    if (GUILayout.Button("Export", GUILayout.Width(75F)))
                    {
                        importer.Export();
                    }
                }
            }

            return base.OnApplyRevertGUI();
        }

        protected virtual void OnDrawProperties()
        {
        }
    }
}
