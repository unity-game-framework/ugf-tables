using System;
using System.Collections.Generic;
using System.Reflection;
using UGF.EditorTools.Editor.Ids;
using UGF.EditorTools.Runtime.Ids;
using UnityEditor;
using UnityEngine;

namespace UGF.Tables.Editor
{
    internal static class TableTreeEditorInternalUtility
    {
        public static bool IsSingleFieldProperty(SerializedProperty serializedProperty)
        {
            if (serializedProperty == null) throw new ArgumentNullException(nameof(serializedProperty));

            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.String:
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.AnimationCurve:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.ManagedReference: return true;
                default: return false;
            }
        }

        public static bool TryPropertySetBoxedValue(SerializedProperty serializedProperty, object value, out Exception error)
        {
            if (serializedProperty == null) throw new ArgumentNullException(nameof(serializedProperty));

            try
            {
                serializedProperty.boxedValue = value;
            }
            catch (Exception exception)
            {
                error = exception;
                return false;
            }

            error = default;
            return true;
        }

        public static void PropertyInsert(SerializedProperty serializedProperty, IReadOnlyList<int> indexes, Action<SerializedProperty> initializeHandler = null)
        {
            if (serializedProperty == null) throw new ArgumentNullException(nameof(serializedProperty));
            if (indexes == null) throw new ArgumentNullException(nameof(indexes));

            if (indexes is List<int> list)
            {
                list.Sort();
            }

            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                int index = indexes[i];

                PropertyInsert(serializedProperty, index, initializeHandler);
            }
        }

        public static void PropertyInsert(SerializedProperty serializedProperty, int index, IReadOnlyList<object> values, Action<SerializedProperty> initializeHandler = null)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            for (int i = 0; i < values.Count; i++)
            {
                object value = values[i];

                PropertyInsert(serializedProperty, index, initializeHandler, value);

                index = Mathf.Min(index + 1, serializedProperty.arraySize - 1);
            }
        }

        public static SerializedProperty PropertyInsert(SerializedProperty serializedProperty, int index, object value = null)
        {
            return PropertyInsert(serializedProperty, index, null, value);
        }

        public static SerializedProperty PropertyInsert(SerializedProperty serializedProperty, int index, Action<SerializedProperty> initializeHandler = null, object value = null)
        {
            if (serializedProperty == null) throw new ArgumentNullException(nameof(serializedProperty));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

            serializedProperty.InsertArrayElementAtIndex(index);

            index = Mathf.Min(index + 1, serializedProperty.arraySize - 1);

            SerializedProperty propertyElement = serializedProperty.GetArrayElementAtIndex(index);

            if (value != null)
            {
                propertyElement.boxedValue = value;
            }

            initializeHandler?.Invoke(propertyElement);

            return propertyElement;
        }

        public static void PropertyRemove(SerializedProperty serializedProperty, IReadOnlyList<int> indexes)
        {
            if (serializedProperty == null) throw new ArgumentNullException(nameof(serializedProperty));
            if (indexes == null) throw new ArgumentNullException(nameof(indexes));

            if (indexes is List<int> list)
            {
                list.Sort();
            }

            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                int index = indexes[i];

                serializedProperty.DeleteArrayElementAtIndex(index);
            }
        }

        public static int GetEntryId(SerializedProperty serializedProperty, TableTreeOptions options)
        {
            if (serializedProperty == null) throw new ArgumentNullException(nameof(serializedProperty));
            if (options == null) throw new ArgumentNullException(nameof(options));

            SerializedProperty propertyId = serializedProperty.FindPropertyRelative(options.PropertyIdName);
            GlobalId id = GlobalIdEditorUtility.GetGlobalIdFromProperty(propertyId);

            return (int)id.First;
        }

        public static bool TryGetSerializedField(Type type, string name, out FieldInfo field)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            List<FieldInfo> fields = GetSerializedFields(type);

            for (int i = 0; i < fields.Count; i++)
            {
                field = fields[i];

                if (field.Name == name)
                {
                    return true;
                }
            }

            field = default;
            return false;
        }

        public static List<FieldInfo> GetSerializedFields(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var fields = new List<FieldInfo>();

            while (type != null)
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (field.IsPublic || field.IsDefined(typeof(SerializeField)) || field.IsDefined(typeof(SerializeReference)))
                    {
                        fields.Add(field);
                    }
                }

                type = type.BaseType;
            }

            fields.Sort(TableTreeEntryFieldComparer.Default);

            return fields;
        }
    }
}
