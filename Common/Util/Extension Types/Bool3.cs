using UnityEngine;

namespace Shears
{
    /// <summary>
    /// A composite type for storing 3 boolean values.
    /// </summary>
    [System.Serializable]
    public struct Bool3
    {
        public static Bool3 False = new(false, false, false);
        public static Bool3 True = new(true, true, true);

        public bool x;
        public bool y;
        public bool z;

        public Bool3(bool x, bool y, bool z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
