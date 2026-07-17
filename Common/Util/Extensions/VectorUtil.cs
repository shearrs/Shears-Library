using System;
using System.Collections.Generic;
using Codice.Client.BaseCommands;
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

        public static void Deconstruct(this Vector2 self, out float x, out float y)
        {
            x = self.x;
            y = self.y;
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

        public static Vector3 X(this Vector3 v)
        {
            return new(v.x, 0.0f, 0.0f);
        }

        public static Vector3 Y(this Vector3 v)
        {
            return new(0.0f, v.y, 0.0f);
        }

        public static Vector3 Z(this Vector3 v)
        {
            return new(0.0f, 0.0f, v.z);
        }

        public static Vector3 XY(this Vector3 v)
        {
            return new(v.x, v.y, 0.0f);
        }

        public static Vector3 XZ(this Vector3 v)
        {
            return new(v.x, 0.0f, v.z);
        }

        public static Vector3 YZ(this Vector3 v)
        {
            return new(0.0f, v.y, v.z);
        }

        public static Vector3 With(
            this Vector3 v,
            float? x = null,
            float? y = null,
            float? z = null
        )
        {
            return new(
                x == null ? v.x : x.Value,
                y == null ? v.y : y.Value,
                z == null ? v.z : z.Value
            );
        }

        public static Vector3Int RoundToInt(this Vector3 v)
        {
            return new(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        }

        public static Vector3Int X(this Vector3Int v)
        {
            return new(v.x, 0, 0);
        }

        public static Vector3Int Y(this Vector3Int v)
        {
            return new(0, v.y, 0);
        }

        public static Vector3Int Z(this Vector3Int v)
        {
            return new(0, 0, v.z);
        }

        public static Vector3Int XY(this Vector3Int v)
        {
            return new(v.x, v.y, 0);
        }

        public static Vector3Int XZ(this Vector3Int v)
        {
            return new(v.x, 0, v.z);
        }

        public static Vector3Int YZ(this Vector3Int v)
        {
            return new(0, v.y, v.z);
        }

        public static Vector3Int With(
            this Vector3Int v,
            int? x = null,
            int? y = null,
            int? z = null
        )
        {
            return new(
                x == null ? v.x : x.Value,
                y == null ? v.y : y.Value,
                z == null ? v.z : z.Value
            );
        }

        public static bool WithinRange(Vector3Int value, Vector3Int min, Vector3Int max)
        {
            return value.x >= min.x
                && value.x <= max.x
                && value.y >= min.y
                && value.y <= max.y
                && value.z >= min.z
                && value.z <= max.z;
        }

        public static void Min(
            out int xMin,
            out int yMin,
            out int zMin,
            params Vector3Int[] vectors
        )
        {
            xMin = MathUtil.MinSelector(vectors, v => v.x);
            yMin = MathUtil.MinSelector(vectors, v => v.y);
            zMin = MathUtil.MinSelector(vectors, v => v.z);
        }

        public static void Max(
            out int xMax,
            out int yMax,
            out int zMax,
            params Vector3Int[] vectors
        )
        {
            xMax = MathUtil.MaxSelector(vectors, v => v.x);
            yMax = MathUtil.MaxSelector(vectors, v => v.y);
            zMax = MathUtil.MaxSelector(vectors, v => v.z);
        }

        public static void MinMax(
            out int xMin,
            out int xMax,
            out int yMin,
            out int yMax,
            out int zMin,
            out int zMax,
            params Vector3Int[] vectors
        )
        {
            MathUtil.MinMax(out xMin, out xMax, vectors, v => v.x, v => v.x);
            MathUtil.MinMax(out yMin, out yMax, vectors, v => v.y, v => v.y);
            MathUtil.MinMax(out zMin, out zMax, vectors, v => v.z, v => v.z);
        }

        public static void MinMax<T>(
            out int xMin,
            out int xMax,
            out int yMin,
            out int yMax,
            out int zMin,
            out int zMax,
            IReadOnlyList<T> list,
            Func<T, Vector3Int> minSelector,
            Func<T, Vector3Int> maxSelector
        )
        {
            if (list.Count == 0)
            {
                xMin = xMax = yMin = yMax = zMin = zMax = -1;

                return;
            }

            xMin = minSelector(list[0]).x;
            xMax = maxSelector(list[0]).x;
            yMin = minSelector(list[0]).y;
            yMax = maxSelector(list[0]).y;
            zMin = minSelector(list[0]).z;
            zMax = maxSelector(list[0]).z;

            for (int i = 1; i < list.Count; i++)
            {
                int xMinCandidate = minSelector(list[i]).x;
                int xMaxCandidate = maxSelector(list[i]).x;
                int yMinCandidate = minSelector(list[i]).y;
                int yMaxCandidate = maxSelector(list[i]).y;
                int zMinCandidate = minSelector(list[i]).z;
                int zMaxCandidate = maxSelector(list[i]).z;

                if (xMinCandidate < xMin)
                    xMin = xMinCandidate;
                if (xMaxCandidate > xMax)
                    xMax = xMaxCandidate;

                if (yMinCandidate < yMin)
                    yMin = yMinCandidate;
                if (yMaxCandidate > yMax)
                    yMax = yMaxCandidate;

                if (zMinCandidate < zMin)
                    zMin = zMinCandidate;
                if (zMaxCandidate > zMax)
                    zMax = zMaxCandidate;
            }
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

        public static void Min(
            out float xMin,
            out float yMin,
            out float zMin,
            params Vector3[] vectors
        )
        {
            xMin = MathUtil.MinSelector(vectors, v => v.x);
            yMin = MathUtil.MinSelector(vectors, v => v.y);
            zMin = MathUtil.MinSelector(vectors, v => v.z);
        }

        public static void Max(
            out float xMax,
            out float yMax,
            out float zMax,
            params Vector3[] vectors
        )
        {
            xMax = MathUtil.MaxSelector(vectors, v => v.x);
            yMax = MathUtil.MaxSelector(vectors, v => v.y);
            zMax = MathUtil.MaxSelector(vectors, v => v.z);
        }

        public static void MinMax(
            out float xMin,
            out float xMax,
            out float yMin,
            out float yMax,
            out float zMin,
            out float zMax,
            params Vector3[] vectors
        )
        {
            MathUtil.MinMax(out xMin, out xMax, vectors, v => v.x, v => v.x);
            MathUtil.MinMax(out yMin, out yMax, vectors, v => v.y, v => v.y);
            MathUtil.MinMax(out zMin, out zMax, vectors, v => v.z, v => v.z);
        }

        public static void MinMax(
            out float xMin,
            out float xMax,
            out float yMin,
            out float yMax,
            out float zMin,
            out float zMax,
            IReadOnlyList<Vector3> vectors
        )
        {
            MathUtil.MinMax(out xMin, out xMax, vectors, v => v.x, v => v.x);
            MathUtil.MinMax(out yMin, out yMax, vectors, v => v.y, v => v.y);
            MathUtil.MinMax(out zMin, out zMax, vectors, v => v.z, v => v.z);
        }

        public static void MinMax<T>(
            out float xMin,
            out float xMax,
            out float yMin,
            out float yMax,
            out float zMin,
            out float zMax,
            IReadOnlyList<T> list,
            Func<T, Vector3> minSelector,
            Func<T, Vector3> maxSelector
        )
        {
            if (list.Count == 0)
            {
                xMin = xMax = yMin = yMax = zMin = zMax = -1;

                return;
            }

            xMin = minSelector(list[0]).x;
            xMax = maxSelector(list[0]).x;
            yMin = minSelector(list[0]).y;
            yMax = maxSelector(list[0]).y;
            zMin = minSelector(list[0]).z;
            zMax = maxSelector(list[0]).z;

            for (int i = 1; i < list.Count; i++)
            {
                float xMinCandidate = minSelector(list[i]).x;
                float xMaxCandidate = maxSelector(list[i]).x;
                float yMinCandidate = minSelector(list[i]).y;
                float yMaxCandidate = maxSelector(list[i]).y;
                float zMinCandidate = minSelector(list[i]).z;
                float zMaxCandidate = maxSelector(list[i]).z;

                if (xMinCandidate < xMin)
                    xMin = xMinCandidate;
                if (xMaxCandidate > xMax)
                    xMax = xMaxCandidate;

                if (yMinCandidate < yMin)
                    yMin = yMinCandidate;
                if (yMaxCandidate > yMax)
                    yMax = yMaxCandidate;

                if (zMinCandidate < zMin)
                    zMin = zMinCandidate;
                if (zMaxCandidate > zMax)
                    zMax = zMaxCandidate;
            }
        }
    }
}
