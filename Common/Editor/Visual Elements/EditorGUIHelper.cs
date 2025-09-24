using UnityEditor;
using UnityEngine;

namespace Shears.Editor
{
    /// <summary>
    /// Helper class copied from Unity's internal code.
    /// </summary>
    public static class EditorGUIHelper
    {
        public static float RoundToPixelGrid(float v)
        {
            const float kNearestRoundingOffset = 0.48f;
            float scale = EditorGUIUtility.pixelsPerPoint;

            return Mathf.Floor((v * scale) + kNearestRoundingOffset) / scale;
        }
    }
}
