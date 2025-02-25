using UnityEngine;

namespace Shears
{
    public static class VectorUtil
    {
        public static Vector3 ClampComponents(Vector3 vector, float min, float max)
        {
            vector.x = Mathf.Clamp(vector.x, min, max);
            vector.y = Mathf.Clamp(vector.y, min, max);
            vector.z = Mathf.Clamp(vector.z, min, max);

            return vector;
        }
    }
}
