using UGF.EditorTools.Editor.IMGUI;
using UGF.EditorTools.Editor.IMGUI.Scopes;
using UGF.Tables.Runtime;
using UnityEditor;

namespace UGF.Tables.Editor
{
    [CustomEditor(typeof(TableAsset<>), true)]
    internal class TableAssetEditor : UnityEditor.Editor
    {
        private TableDrawer m_tableDrawer;

        private void OnEnable()
        {
            m_tableDrawer = new TableDrawer(serializedObject.FindProperty("m_table"));
            m_tableDrawer.Enable();
        }

        private void OnDisable()
        {
            m_tableDrawer.Disable();
        }

        public override void OnInspectorGUI()
        {
            using (new SerializedObjectUpdateScope(serializedObject))
            {
                EditorIMGUIUtility.DrawScriptProperty(serializedObject);

                m_tableDrawer.DrawGUILayout();
            }
        }
    }
}
