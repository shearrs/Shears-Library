using UnityEngine;

namespace Shears.Grids
{
    [System.Serializable]
    public class SpiderInitializeBehaviour : IPathInitializeBehaviour
    {
        public bool TryGetStartPosition(
            IPathInitializeBehaviour.ExecuteData data,
            out EntityPosition startPosition
        )
        {
            var grid = data.Grid;
            var worldPosition = data.WorldPosition;
            var surfaceDirection = data.SurfaceDirection;

            startPosition = null;

            if (!grid.TryGetPositionGroup(worldPosition, out var positionGroup))
                return false;

            if (!positionGroup.TryGetPositionInDirection(surfaceDirection, out var startSurface))
            {
                if (positionGroup.Down != null)
                    startPosition = positionGroup.Down;
                else if (positionGroup.Left != null)
                    startPosition = positionGroup.Left;
                else if (positionGroup.Right != null)
                    startPosition = positionGroup.Right;
                else if (positionGroup.Up != null)
                    startPosition = positionGroup.Up;
                else if (positionGroup.Center != null)
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
        ) => DefaultInitializeBehaviour.StaticTryGetTargetPosition(data, out targetPosition);
    }
}
