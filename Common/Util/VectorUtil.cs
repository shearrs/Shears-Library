using UnityEngine;

namespace Shears
{
    public static class VectorUtil
    {
        public static Vector3 ClampComponents(this Vector3 vector, float min, float max)
        {
            vector.x = Mathf.Clamp(vector.x, min, max);
            vector.y = Mathf.Clamp(vector.y, min, max);
            vector.z = Mathf.Clamp(vector.z, min, max);

            return vector;
        }

        public static Vector2 ClampComponents(this Vector2 vector, float min, float max)
        {
            vector.x = Mathf.Clamp(vector.x, min, max);
            vector.y = Mathf.Clamp(vector.y, min, max);

            return vector;
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
    }
}
