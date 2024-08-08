using System;
using System.Collections.Generic;
using UGF.EditorTools.Editor.Ids;
using UGF.EditorTools.Runtime.Ids;
using UnityEditor;
using UnityEngine;

namespace UGF.Tables.Editor
{
    public class TableTreeColumnComparer : IComparer<SerializedProperty>
    {
        public static TableTreeColumnComparer Default { get; } = new TableTreeColumnComparer();

        public int Compare(SerializedProperty x, SerializedProperty y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;

            if (x.propertyType != y.propertyType)
            {
                return 0;
            }

            return OnCompare(x, y);
        }

        protected virtual int OnCompare(SerializedProperty x, SerializedProperty y)
        {
            switch (x.propertyType)
            {
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Integer: return x.intValue.CompareTo(y.intValue);
                case SerializedPropertyType.Boolean: return x.boolValue.CompareTo(y.boolValue);
                case SerializedPropertyType.Float: return x.floatValue.CompareTo(y.floatValue);
                case SerializedPropertyType.Character:
                case SerializedPropertyType.String: return string.Compare(x.stringValue, y.stringValue, StringComparison.Ordinal);
                case SerializedPropertyType.ObjectReference: return x.objectReferenceInstanceIDValue.CompareTo(y.objectReferenceInstanceIDValue);
                case SerializedPropertyType.Vector2: return x.vector2Value.magnitude.CompareTo(y.vector2Value.magnitude);
                case SerializedPropertyType.Vector3: return x.vector3Value.magnitude.CompareTo(y.vector3Value.magnitude);
                case SerializedPropertyType.Vector4: return x.vector4Value.magnitude.CompareTo(y.vector4Value.magnitude);
                case SerializedPropertyType.Rect: return x.rectValue.size.magnitude.CompareTo(y.rectValue.size.magnitude);
                case SerializedPropertyType.Bounds: return x.boundsValue.size.magnitude.CompareTo(y.boundsValue.size.magnitude);
                case SerializedPropertyType.Quaternion: return x.quaternionValue.eulerAngles.magnitude.CompareTo(y.quaternionValue.normalized);
                case SerializedPropertyType.Vector2Int: return x.vector2IntValue.magnitude.CompareTo(y.vector2IntValue.magnitude);
                case SerializedPropertyType.Vector3Int: return x.vector3IntValue.magnitude.CompareTo(y.vector3IntValue.magnitude);
                case SerializedPropertyType.RectInt: return x.rectIntValue.size.magnitude.CompareTo(y.rectIntValue.size.magnitude);
                case SerializedPropertyType.BoundsInt: return x.boundsIntValue.size.magnitude.CompareTo(y.boundsIntValue.size.magnitude);
                case SerializedPropertyType.Hash128: return x.hash128Value.CompareTo(y.hash128Value);
                case SerializedPropertyType.ManagedReference: return x.managedReferenceId.CompareTo(y.managedReferenceId);
                case SerializedPropertyType.Color:
                {
                    var xVector = (Vector4)x.colorValue;
                    var yVector = (Vector4)y.colorValue;

                    return xVector.magnitude.CompareTo(yVector.magnitude);
                }
                case SerializedPropertyType.Enum:
                {
                    switch (x.numericType)
                    {
                        case SerializedPropertyNumericType.UInt8:
                        case SerializedPropertyNumericType.UInt16:
                        case SerializedPropertyNumericType.Int8:
                        case SerializedPropertyNumericType.Int16:
                        case SerializedPropertyNumericType.Int32: return x.intValue.CompareTo(y.intValue);
                        case SerializedPropertyNumericType.Int64:
                        case SerializedPropertyNumericType.UInt32:
                        case SerializedPropertyNumericType.UInt64: return x.longValue.CompareTo(y.longValue);
                        default: return 0;
                    }
                }
                case SerializedPropertyType.Generic:
                {
                    if (x.isArray)
                    {
                        return x.arraySize.CompareTo(y.arraySize);
                    }

                    if (x.type == nameof(GlobalId))
                    {
                        GlobalId xId = GlobalIdEditorUtility.GetGlobalIdFromProperty(x);
                        GlobalId yId = GlobalIdEditorUtility.GetGlobalIdFromProperty(y);

                        return xId.CompareTo(yId);
                    }

                    return 0;
                }
                default:
                {
                    return 0;
                }
            }
        }
    }
}
