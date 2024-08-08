using System;

namespace UGF.Tables.Runtime
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableEntryChildrenAttribute : Attribute
    {
        public string PropertyName { get; }

        public TableEntryChildrenAttribute() : this("m_children")
        {
        }

        public TableEntryChildrenAttribute(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Value cannot be null or empty.", nameof(propertyName));

            PropertyName = propertyName;
        }
    }
}
