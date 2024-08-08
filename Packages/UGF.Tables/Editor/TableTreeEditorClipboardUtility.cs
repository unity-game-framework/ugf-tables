using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UGF.Tables.Editor
{
    public static class TableTreeEditorClipboardUtility
    {
        public static bool TrySetPropertyValueFromEntryField(SerializedProperty serializedProperty, object entry)
        {
            if (serializedProperty == null) throw new ArgumentNullException(nameof(serializedProperty));
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            if (TryGetEntryFieldValue(entry, serializedProperty.name, out object value))
            {
                if (!TableTreeEditorInternalUtility.TryPropertySetBoxedValue(serializedProperty, value, out Exception error))
                {
                    Debug.LogWarning($"Table entry property can not be set: '{serializedProperty.name}'.\n{error}");
                    return false;
                }

                return true;
            }

            Debug.LogWarning($"Table entry property can not be set: '{serializedProperty.name}'.");

            return false;
        }

        public static bool TryGetEntryFieldValue(object entry, string name, out object value)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            if (TableTreeEditorInternalUtility.TryGetSerializedField(entry.GetType(), name, out FieldInfo field))
            {
                value = field.GetValue(entry);
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryCopy(IReadOnlyList<TableTreeViewItem> items, ICollection<object> values, out Exception error)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (values == null) throw new ArgumentNullException(nameof(values));

            for (int i = 0; i < items.Count; i++)
            {
                TableTreeViewItem item = items[i];

                object value;

                try
                {
                    value = item.SerializedProperty.boxedValue;
                }
                catch (Exception exception)
                {
                    error = exception;
                    return false;
                }

                values.Add(value);
            }

            error = default;
            return true;
        }
    }
}
