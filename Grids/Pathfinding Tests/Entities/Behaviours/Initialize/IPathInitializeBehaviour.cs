using UnityEngine;

namespace Shears.Grids
{
    public interface IPathInitializeBehaviour
    {
        public readonly ref struct ExecuteData
        {
            public PathGrid Grid { get; }
            public Vector3 WorldPosition { get; }
            public Direction SurfaceDirection { get; }

            public ExecuteData(PathGrid grid, Vector3 worldPosition, Direction surfaceDirection)
            {
                Grid = grid;
                WorldPosition = worldPosition;
                SurfaceDirection = surfaceDirection;
            }
        }

        public bool TryGetStartPosition(ExecuteData data, out EntityPosition startPosition);

        public bool TryGetTargetPosition(ExecuteData data, out EntityPosition targetPosition);
    }
}
