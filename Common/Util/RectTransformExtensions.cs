using System.Buffers;
using UnityEngine;

namespace Shears
{
    public static class RectTransformExtensions
    {
        // From Alenya: https://discussions.unity.com/t/convert-recttransform-rect-to-rect-world/153391/5
        public static Rect GetWorldRect(this RectTransform transform)
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;

            var pool = ArrayPool<Vector3>.Shared;
            var corners = pool.Rent(4);
            transform.GetWorldCorners(corners);

            for (var i = 0; i < 4; i++)
            {
                var corner = corners[i];
                minX = Mathf.Min(corner.x, minX);
                minY = Mathf.Min(corner.y, minY);
                maxX = Mathf.Max(corner.x, maxX);
                maxY = Mathf.Max(corner.y, maxY);
            }

            pool.Return(corners);

            var position = new Vector2(minX, minY);
            var size = new Vector2(maxX - minX, maxY - minY);
            return new Rect(position, size);
        }
    }
}
