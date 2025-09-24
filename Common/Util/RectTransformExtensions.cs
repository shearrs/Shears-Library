using System.Buffers;
using UnityEngine;

namespace Shears
{
    public static class RectTransformExtensions
    {
        // From Alenya: https://discussions.unity.com/t/convert-recttransform-rect-to-rect-world/153391/4
        public static Rect GetWorldRect(this RectTransform transform)
        {
            var pool = ArrayPool<Vector3>.Shared;
            var corners = pool.Rent(4);
            transform.GetWorldCorners(corners);

            var rect = new Rect(corners[0], corners[2] - corners[0]);
            pool.Return(corners);

            return rect;
        }
    }
}
