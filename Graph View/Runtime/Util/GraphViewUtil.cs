using UnityEngine;

namespace Shears.GraphViews
{
    public static class GraphViewUtil
    {
        public static string NewGUID()
        {
            return System.Guid.NewGuid().ToString();
        }
    }
}
