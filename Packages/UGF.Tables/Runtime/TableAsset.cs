using System;
using UnityEngine;

namespace UGF.Tables.Runtime
{
    public abstract class TableAsset : ScriptableObject
    {
        public ITable Get()
        {
            return OnGet();
        }

        public void Set(ITable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            OnSet(table);
        }

        protected abstract ITable OnGet();
        protected abstract void OnSet(ITable table);
    }
}
