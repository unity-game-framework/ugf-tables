using System;
using System.Collections.Generic;
using System.Reflection;

namespace UGF.Tables.Editor
{
    internal class TableTreeEntryFieldComparer : IComparer<FieldInfo>
    {
        public static IComparer<FieldInfo> Default { get; } = new TableTreeEntryFieldComparer();

        public int Compare(FieldInfo x, FieldInfo y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;

            int xCount = GetInheritanceCount(x.DeclaringType);
            int yCount = GetInheritanceCount(y.DeclaringType);

            return xCount.CompareTo(yCount);
        }

        private static int GetInheritanceCount(Type type)
        {
            int count = 0;

            while (type != null)
            {
                count++;

                type = type.BaseType;
            }

            return count;
        }
    }
}
