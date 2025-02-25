using UnityEditor;
using UnityEngine;

namespace Shears.Editor
{
    public static class AssetUtility
    {
        public static void DirtyAndSave(Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
            AssetDatabase.Refresh();
        }
    }
}