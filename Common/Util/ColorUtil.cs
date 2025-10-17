using UnityEngine;

namespace Shears
{
    public static class ColorUtil
    {
        public static Color With(this Color color, float? r = null, float? g = null, float? b = null, float? a = null)
        {
            return new Color(r == null ? color.r : r.Value, g ==  null ? color.g : g.Value, b == null ? color.b : b.Value, a == null ? color.a : a.Value);
        }
    }
}
