using System;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace UGF.Tables.Editor
{
    public abstract class TableAssetImporter : ScriptedImporter
    {
        public bool CanImport { get; }
        public bool CanExport { get; }

        protected TableAssetImporter() : this(true, true)
        {
        }

        protected TableAssetImporter(bool canImport, bool canExport)
        {
            CanImport = canImport;
            CanExport = canExport;
        }

        public override void OnImportAsset(AssetImportContext context)
        {
            var asset = ObjectFactory.CreateInstance<TextAsset>();

            context.AddObjectToAsset("main", asset);
            context.SetMainObject(asset);

            EditorUtility.SetDirty(this);
        }

        public bool IsValid()
        {
            return OnIsValid();
        }

        public void Import()
        {
            if (!CanImport) throw new InvalidOperationException("Table asset can not be imported.");

            OnImport();
        }

        public void Export()
        {
            if (!CanExport) throw new InvalidOperationException("Table asset can not be exported.");

            OnExport();
        }

        protected abstract bool OnIsValid();
        protected abstract void OnImport();
        protected abstract void OnExport();
    }
}
