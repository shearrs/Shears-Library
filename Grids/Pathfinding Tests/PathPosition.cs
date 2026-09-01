using UnityEngine;

namespace Shears.Grids
{
    public readonly struct PathPosition
    {
        public Vector3 Position { get; }
        public Vector3 Normal { get; }

        public PathPosition(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }
    }
}
