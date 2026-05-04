using UnityEngine;

namespace Shears
{
    public static class MathUtil
    {
        public static void MinMax(out float min, out float max, params float[] values)
        {
            min = Mathf.Min(values);
            max = Mathf.Max(values);
        }
    }
}
