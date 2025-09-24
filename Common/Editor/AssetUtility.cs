using UnityEditor;
using UnityEngine;

namespace Shears.Editor
{
    /// <summary>
    /// Utility class for handling asset operations in the Unity Editor.
    /// </summary>
    public static class AssetUtility
    {
        /// <summary>
        /// Shorthand function for dirtying and saving an asset.
        /// </summary>
        /// <param name="asset">The asset to save.</param>
        public static void DirtyAndSave(Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
            AssetDatabase.Refresh();
        }
    }
}