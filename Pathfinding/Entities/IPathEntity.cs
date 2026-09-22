using UnityEngine;

namespace Shears.Pathfinding
{
    public interface IPathEntity
    {
        public Vector3 Position { get; }
        public PathPosition PreviousPosition { get; }
        public Direction DesiredSurfaceDirection { get; }
    }
}
