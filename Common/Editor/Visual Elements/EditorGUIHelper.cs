using UnityEditor;
using UnityEngine;

namespace Shears.Editor
{
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
