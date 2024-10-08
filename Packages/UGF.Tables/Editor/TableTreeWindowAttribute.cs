﻿using System;
using UGF.Tables.Runtime;

namespace UGF.Tables.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableTreeWindowAttribute : Attribute
    {
        public Type AssetType { get; }
        public int Priority { get; set; }

        public TableTreeWindowAttribute(Type assetType = null)
        {
            AssetType = assetType ?? typeof(TableAsset);
        }
    }
}
