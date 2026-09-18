using Shears.Logging;
using UnityEngine;

namespace Shears.Pathfinding
{
    [System.Serializable]
    public class DefaultInitializeBehaviour : IPathInitializeBehaviour
    {
        public bool TryGetStartPosition(
            IPathInitializeBehaviour.ExecuteData data,
            out EntityPosition startPosition
        )
        {
            var grid = data.Grid;
            var worldPosition = data.WorldPosition;
            var surfaceDirection = Direction.Down;

            startPosition = null;

            if (!grid.TryGetPositionGroup(worldPosition, out var positionGroup))
                return false;

            if (!positionGroup.TryGetPositionInDirection(surfaceDirection, out var startSurface))
            {
                if (positionGroup.Center != null)
                    startPosition = positionGroup.Center;
                else
                    return false;
            }
            else
                startPosition = startSurface;

            return true;
        }

        public bool TryGetTargetPosition(
            IPathInitializeBehaviour.ExecuteData data,
            out EntityPosition targetPosition
        ) => StaticTryGetTargetPosition(data, out targetPosition);

        public static bool StaticTryGetTargetPosition(
            IPathInitializeBehaviour.ExecuteData data,
            out EntityPosition targetPosition
        )
        {
            var grid = data.Grid;
            var worldPosition = data.WorldPosition;

            targetPosition = null;

            if (!grid.TryGetPositionGroup(worldPosition, out var positionGroup))
                return false;

            if (!positionGroup.TryGetPositionInDirection(Direction.Down, out var targetSurface))
                return false;
            else
                targetPosition = targetSurface;

            return true;
        }
    }
}
