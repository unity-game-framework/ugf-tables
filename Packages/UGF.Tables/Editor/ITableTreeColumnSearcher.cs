using UnityEditor;

namespace UGF.Tables.Editor
{
    public interface ITableTreeColumnSearcher
    {
        bool Check(SerializedProperty serializedProperty, string search);
    }
}
