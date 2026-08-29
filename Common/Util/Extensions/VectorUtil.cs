using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    public static class VectorUtil
    {
        private const float ONE_THIRD = 1.0f / 3.0f;

        public static Vector3 ClampComponents(this Vector3 v, float min, float max)
        {
            v.x = Mathf.Clamp(v.x, min, max);
            v.y = Mathf.Clamp(v.y, min, max);
            v.z = Mathf.Clamp(v.z, min, max);

            return v;
        }

        public static Vector2 ClampComponents(this Vector2 v, float min, float max)
        {
            v.x = Mathf.Clamp(v.x, min, max);
            v.y = Mathf.Clamp(v.y, min, max);

            return v;
        }

        public static Vector3Int ClampComponents(this Vector3Int v, int min, int max)
        {
            v.x = Mathf.Clamp(v.x, min, max);
            v.y = Mathf.Clamp(v.y, min, max);
            v.z = Mathf.Clamp(v.z, min, max);

            return v;
        }

        public static Vector3Int ClampMin(this Vector3Int v, int min)
        {
            v.x = Mathf.Min(v.x, min);
            v.y = Mathf.Min(v.y, min);
            v.z = Mathf.Min(v.z, min);

            return v;
        }

        public static Vector3Int ClampMax(this Vector3Int v, int max)
        {
            v.x = Mathf.Max(v.x, max);
            v.y = Mathf.Max(v.y, max);
            v.z = Mathf.Max(v.z, max);

            return v;
        }

        public static Vector3 MultiplyComponents(this Vector3 v0, Vector3 v1)
        {
            return new(v0.x * v1.x, v0.y * v1.y, v0.z * v1.z);
        }

        public static float GetAverage(this Vector3 v)
        {
            return ONE_THIRD * (v.x + v.y + v.z);
        }

        public static void Deconstruct(this Vector2 v, out float x, out float y)
        {
            x = v.x;
            y = v.y;
        }

        public static void Deconstruct(this Vector3 v, out float x, out float y, out float z)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static void Deconstruct(this Vector3Int v, out int x, out int y, out int z)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static Vector3 Deg2Rad(this Vector3 self)
        {
            return new Vector3(
                Mathf.Deg2Rad * self.x,
                Mathf.Deg2Rad * self.y,
                Mathf.Deg2Rad * self.z
            );
        }

        public static Vector3 RandomRange(Vector3 min, Vector3 max)
        {
            return new(
                UnityEngine.Random.Range(min.x, max.x),
                UnityEngine.Random.Range(min.y, max.y),
                UnityEngine.Random.Range(min.z, max.z)
            );
        }

        public static Vector3 With(
            this Vector3 v,
            float? x = null,
            float? y = null,
            float? z = null
        )
        {
            return new(x ?? v.x, y ?? v.y, z ?? v.z);
        }

        /// <summary>
        /// Round a <see cref="Vector3"/> to a <see cref="Vector3Int"/>.
        /// </summary>
        /// <param name="vector">The vector to round.</param>
        /// <returns>The rounded integer vector.</returns>
        public static Vector3Int RoundToInt(this Vector3 vector)
        {
            var x = Mathf.RoundToInt(vector.x);
            var y = Mathf.RoundToInt(vector.y);
            var z = Mathf.RoundToInt(vector.z);

            return new(x, y, z);
        }

        public static Vector3Int With(
            this Vector3Int v,
            int? x = null,
            int? y = null,
            int? z = null
        )
        {
            return new(x ?? v.x, y ?? v.y, z ?? v.z);
        }

        /// <summary>
        /// Check if a position is within a range.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <param name="min">The minimum bounds.</param>
        /// <param name="max">The maximum bounds.</param>
        /// <returns>Whether or not the position is within the passed range.</returns>
        public static bool WithinRange(Vector3Int position, Vector3Int min, Vector3Int max)
        {
            return position.x >= min.x
                && position.y >= min.y
                && position.z >= min.z
                && position.x <= max.x
                && position.y <= max.y
                && position.z <= max.z;
        }

        public static Vector3 EulerMap(this Vector3 v)
        {
            if (v.x > 180f)
                v.x -= 360f;

            if (v.y > 180f)
                v.y -= 360f;

            if (v.z > 180f)
                v.z -= 360f;

            return v;
        }

        /// <summary>
        /// Calculate a vector with the minimum values of all passed in vectors component-wise.
        /// </summary>
        /// <param name="vectors">The vectors to consider.</param>
        /// <returns>A vector with components equal to the minimum component of each passed vector.</returns>
        public static Vector3 Min(params Vector3[] vectors)
        {
            if (vectors.Length == 0)
                return Vector3.zero;

            var min = vectors[0];

            for (int i = 1; i < vectors.Length; i++)
            {
                var vector = vectors[i];

                if (vector.x < min.x)
                    min.x = vector.x;
                if (vector.y < min.y)
                    min.y = vector.y;
                if (vector.z < min.z)
                    min.z = vector.z;
            }

            return min;
        }

        /// <summary>
        /// Calculate a vector with the maximum values of all passed in vectors component-wise.
        /// </summary>
        /// <param name="vectors">The vectors to consider.</param>
        /// <returns>A vector with components equal to the maximum component of each passed vector.</returns>
        public static Vector3 Max(params Vector3[] vectors)
        {
            if (vectors.Length == 0)
                return Vector3.zero;

            var max = vectors[0];

            for (int i = 1; i < vectors.Length; i++)
            {
                var vector = vectors[i];

                if (vector.x > max.x)
                    max.x = vector.x;
                if (vector.y > max.y)
                    max.y = vector.y;
                if (vector.z > max.z)
                    max.z = vector.z;
            }

            return max;
        }

        /// <inheritdoc cref="Min(Vector3[])"/>
        public static Vector3Int Min(params Vector3Int[] vectors)
        {
            if (vectors.Length == 0)
                return Vector3Int.zero;

            var min = vectors[0];

            for (int i = 1; i < vectors.Length; i++)
            {
                var vector = vectors[i];

                if (vector.x < min.x)
                    min.x = vector.x;
                if (vector.y < min.y)
                    min.y = vector.y;
                if (vector.z < min.z)
                    min.z = vector.z;
            }

            return min;
        }

        /// <inheritdoc cref="Max(Vector3[])"/>
        public static Vector3Int Max(params Vector3Int[] vectors)
        {
            if (vectors.Length == 0)
                return Vector3Int.zero;

            var max = vectors[0];

            for (int i = 1; i < vectors.Length; i++)
            {
                var vector = vectors[i];

                if (vector.x > max.x)
                    max.x = vector.x;
                if (vector.y > max.y)
                    max.y = vector.y;
                if (vector.z > max.z)
                    max.z = vector.z;
            }

            return max;
        }

        public static void MinMax<T>(
            out Vector3Int min,
            out Vector3Int max,
            IReadOnlyList<T> list,
            Func<T, Vector3Int> minSelector,
            Func<T, Vector3Int> maxSelector
        )
        {
            min = minSelector(list[0]);
            max = maxSelector(list[0]);

            for (int i = 1; i < list.Count; i++)
            {
                var currentMin = minSelector(list[i]);
                var currentMax = maxSelector(list[i]);

                if (currentMin.x < min.x)
                    min.x = currentMin.x;
                if (currentMin.y < min.y)
                    min.y = currentMin.y;
                if (currentMin.z < min.z)
                    min.z = currentMin.z;

                if (currentMax.x > max.x)
                    max.x = currentMax.x;
                if (currentMax.y > max.y)
                    max.y = currentMax.y;
                if (currentMax.z > max.z)
                    max.z = currentMax.z;
            }
        }

        /// <summary>
        /// Get the axis direction most aligned with this vector.
        /// </summary>
        /// <param name="vector">The vector to calculate with.</param>
        /// <returns>The most aligned axis.</returns>
        public static Vector3 GetMostAlignedAxis(this Vector3 vector)
        {
            var normalized = vector.normalized;

            float absX = Mathf.Abs(normalized.x);
            float absY = Mathf.Abs(normalized.y);
            float absZ = Mathf.Abs(normalized.z);

            if (absX > absY && absX > absZ)
                return normalized.x > 0 ? Vector3.right : Vector3.left;
            else if (absY > absX && absY > absZ)
                return normalized.y > 0 ? Vector3.up : Vector3.down;
            else
                return normalized.z > 0 ? Vector3.forward : Vector3.back;
        }
    }
}
