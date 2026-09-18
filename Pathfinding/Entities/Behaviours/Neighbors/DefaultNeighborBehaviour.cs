using UnityEngine;
using static Shears.Pathfinding.SurfaceNodeData;

namespace Shears.Pathfinding
{
    [System.Serializable]
    public class DefaultNeighborBehaviour : IPathNeighborBehaviour
    {
        protected static readonly Vector3Int UP = Vector3Int.up;
        protected static readonly Vector3Int DOWN = Vector3Int.down;
        protected static readonly Vector3Int LEFT = Vector3Int.left;
        protected static readonly Vector3Int RIGHT = Vector3Int.right;
        protected static readonly Vector3Int UP_RIGHT = UP + RIGHT;
        protected static readonly Vector3Int UP_LEFT = UP + LEFT;

        public void GetNeighbors(IPathNeighborBehaviour.ExecuteData data)
        {
            var grid = data.Grid;
            var currentPosition = data.CurrentPosition;
            var neighbors = data.Neighbors;
            var gridPosition = currentPosition.GridPosition;

            if (!grid.TryGetPositionGroup(gridPosition, out var group))
                return;

            void addNeighbor(EntityPosition neighbor)
            {
                if (neighbor != null)
                    neighbors.Add(neighbor);
            }

            bool isValidSlope(EntityPosition position, SlopeDirection direction)
            {
                return position is SurfaceEntityPosition surface
                    && surface.IsSlope
                    && surface.SlopeDirection == direction;
            }

            bool tryGetGroup(Direction direction, out EntityPositionGroup targetGroup)
            {
                targetGroup = default;

                if (
                    group.TryGetPositionInDirection(direction, out var localPosition)
                    && localPosition is not null
                )
                    return false;

                return grid.TryGetPositionGroup(
                    gridPosition + direction.ToVectorInt(),
                    out targetGroup
                );
            }

            bool tryGetGroupFromOffset(Vector3Int offset, out EntityPositionGroup targetGroup)
            {
                return grid.TryGetPositionGroup(gridPosition + offset, out targetGroup);
            }

            if (currentPosition is SurfaceEntityPosition currentSurface)
            {
                if (currentSurface.IsSlope)
                {
                    if (currentSurface.SlopeDirection == SlopeDirection.UpRight)
                    {
                        if (tryGetGroup(Direction.Left, out var leftGroup))
                        {
                            addNeighbor(leftGroup.Down);

                            if (isValidSlope(leftGroup.Center, SlopeDirection.UpLeft))
                                addNeighbor(leftGroup.Center);
                        }

                        if (tryGetGroupFromOffset(UP_RIGHT, out var upRightGroup))
                        {
                            addNeighbor(upRightGroup.Down);

                            if (isValidSlope(upRightGroup.Center, SlopeDirection.UpRight))
                                addNeighbor(upRightGroup.Center);
                        }
                    }
                    else if (currentSurface.SlopeDirection == SlopeDirection.UpLeft)
                    {
                        if (tryGetGroup(Direction.Right, out var rightGroup))
                        {
                            addNeighbor(rightGroup.Down);

                            if (isValidSlope(rightGroup.Center, SlopeDirection.UpRight))
                                addNeighbor(rightGroup.Center);
                        }

                        if (tryGetGroupFromOffset(UP_LEFT, out var upLeftGroup))
                        {
                            addNeighbor(upLeftGroup.Down);

                            if (isValidSlope(upLeftGroup.Center, SlopeDirection.UpLeft))
                                addNeighbor(upLeftGroup.Center);
                        }
                    }
                }
                else
                {
                    if (tryGetGroup(Direction.Left, out var leftGroup))
                    {
                        addNeighbor(leftGroup.Down);

                        if (isValidSlope(leftGroup.Center, SlopeDirection.UpLeft))
                            addNeighbor(leftGroup.Center);
                    }

                    if (tryGetGroup(Direction.Right, out var rightGroup))
                    {
                        addNeighbor(rightGroup.Down);

                        if (isValidSlope(rightGroup.Center, SlopeDirection.UpRight))
                            addNeighbor(rightGroup.Center);
                    }

                    if (tryGetGroup(Direction.Forward, out var forwardGroup))
                        addNeighbor(forwardGroup.Down);

                    if (tryGetGroup(Direction.Back, out var backGroup))
                        addNeighbor(backGroup.Down);
                }
            }
            else
            {
                if (group.Down != null)
                    addNeighbor(group.Down);
                else if (tryGetGroup(Direction.Down, out var downGroup))
                {
                    addNeighbor(downGroup.Down);
                    addNeighbor(downGroup.Center);
                }
            }
        }
    }
}
