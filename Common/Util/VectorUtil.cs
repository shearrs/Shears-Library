using UnityEngine;

namespace Shears
{
    public static class VectorUtil
    {
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
            return new
            (
                v0.x * v1.x,
                v0.y * v1.y,
                v0.z * v1.z
            );
        }

        public static void Deconstruct(this Vector2 self, out float x, out float y)
        {
            x = self.x;
            y = self.y;
        }

        public static Vector3 Deg2Rad(this Vector3 self)
        {
            return new Vector3(Mathf.Deg2Rad * self.x, Mathf.Deg2Rad * self.y, Mathf.Deg2Rad * self.z);
        }

        public static Vector3 RandomRange(Vector3 min, Vector3 max)
        {
            return new(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z)
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

        public static Vector3 With(this Vector3 v, float? x = null, float? y = null, float? z = null)
        {
            return new(x == null ? v.x : x.Value, y == null ? v.y : y.Value, z == null ? v.z : z.Value);
        }
    }
}
