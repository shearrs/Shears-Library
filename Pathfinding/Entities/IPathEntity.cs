using UnityEngine;

namespace Shears.Pathfinding
{
    public interface IPathEntity
    {
        public Vector3 Position { get; }
        public Direction DesiredSurfaceDirection { get; }
    }
}
