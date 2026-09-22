using UnityEngine;

namespace Shears.Pathfinding
{
    [System.Serializable]
    public class WallsInitializeBehaviour : ShearsClass, IPathInitializeBehaviour
    {
        [Header("Wall Behaviour")]
        [SerializeField]
        private IPathEntity entity;

        public bool TryGetStartPosition(
            IPathInitializeBehaviour.ExecuteData data,
            out EntityPosition startPosition
        )
        {
            var grid = data.Grid;
            var worldPosition = data.WorldPosition;
            var desiredDirection = entity.DesiredSurfaceDirection;
            var previousPosition = entity.PreviousPosition;

            startPosition = null;

            if (!grid.TryGetPositionGroup(worldPosition, out var positionGroup))
                return false;

            if (!positionGroup.TryGetPositionInDirection(desiredDirection, out var startSurface))
            {
                if (
                    previousPosition != null
                    && previousPosition.EntityPosition is SurfaceEntityPosition previousSurface
                    && previousSurface.SurfaceDirection != desiredDirection
                )
                {
                    var cornerCalculatePosition =
                        worldPosition
                        + (0.5f * grid.NodeSize * previousSurface.SurfaceDirection.ToVector())
                        - 0.5f * grid.NodeSize * desiredDirection.ToVector();

                    Log("try get corner position");
                    if (
                        grid.TryGetPositionGroup(cornerCalculatePosition, out var cornerGroup)
                        && cornerGroup.TryGetPositionInDirection(
                            desiredDirection,
                            out var cornerPosition
                        )
                    )
                    {
                        startPosition = cornerPosition;
                        return true;
                    }

                    cornerCalculatePosition =
                        worldPosition
                        + (0.5f * grid.NodeSize * previousSurface.SurfaceDirection.ToVector());

                    if (
                        grid.TryGetPositionGroup(cornerCalculatePosition, out cornerGroup)
                        && cornerGroup.TryGetPositionInDirection(
                            desiredDirection,
                            out cornerPosition
                        )
                    )
                    {
                        startPosition = cornerPosition;
                        return true;
                    }

                    Log("didn't get it...");
                }

                var belowCalculatePosition =
                    worldPosition
                    + (0.25f * grid.NodeSize * entity.DesiredSurfaceDirection.ToVector());

                if (grid.TryGetPositionGroup(belowCalculatePosition, out var belowGroup))
                {
                    if (
                        belowGroup.Center is SurfaceEntityPosition belowSurface
                        && belowSurface.IsSlope
                    )
                    {
                        startPosition = belowSurface;
                        return true;
                    }
                }

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
