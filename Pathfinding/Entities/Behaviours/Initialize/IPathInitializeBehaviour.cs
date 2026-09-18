using UnityEngine;

namespace Shears.Pathfinding
{
    public interface IPathInitializeBehaviour
    {
        public readonly ref struct ExecuteData
        {
            public PathGrid Grid { get; }
            public Vector3 WorldPosition { get; }

            public ExecuteData(PathGrid grid, Vector3 worldPosition)
            {
                Grid = grid;
                WorldPosition = worldPosition;
            }
        }

        public bool TryGetStartPosition(ExecuteData data, out EntityPosition startPosition);

        public bool TryGetTargetPosition(ExecuteData data, out EntityPosition targetPosition);
    }
}
