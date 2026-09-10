using UnityEngine;

namespace Shears.Grids
{
    public interface IPathEntity
    {
        public Vector3 Position { get; }
        public EntityPosition EntityPosition { get; }
        public bool CanWalkOnWalls { get; }
    }
}
