using UnityEngine;
using static Shears.Pathfinding.SurfaceNodeData;

namespace Shears.Pathfinding
{
    [System.Serializable]
    public class WallsNeighborBehaviour : IPathNeighborBehaviour
    {
        protected static readonly Vector3Int UP = Vector3Int.up;
        protected static readonly Vector3Int DOWN = Vector3Int.down;
        protected static readonly Vector3Int LEFT = Vector3Int.left;
        protected static readonly Vector3Int RIGHT = Vector3Int.right;
        protected static readonly Vector3Int UP_RIGHT = UP + RIGHT;
        protected static readonly Vector3Int UP_LEFT = UP + LEFT;
        protected static readonly Vector3Int DOWN_RIGHT = DOWN + RIGHT;
        protected static readonly Vector3Int DOWN_LEFT = DOWN + LEFT;

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

                        if (tryGetGroup(Direction.Up, out var upGroup))
                            addNeighbor(upGroup.Right);
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

                        if (tryGetGroup(Direction.Up, out var upGroup))
                            addNeighbor(upGroup.Left);
                    }
                    else if (currentSurface.SlopeDirection == SlopeDirection.DownRight)
                    {
                        if (tryGetGroup(Direction.Left, out var leftGroup))
                        {
                            addNeighbor(leftGroup.Up);

                            if (isValidSlope(leftGroup.Center, SlopeDirection.DownLeft))
                                addNeighbor(leftGroup.Center);
                        }

                        if (tryGetGroupFromOffset(DOWN_RIGHT, out var downRightGroup))
                        {
                            addNeighbor(downRightGroup.Up);

                            if (isValidSlope(downRightGroup.Center, SlopeDirection.DownRight))
                                addNeighbor(downRightGroup.Center);
                        }

                        if (tryGetGroup(Direction.Down, out var downGroup))
                            addNeighbor(downGroup.Right);
                    }
                    else if (currentSurface.SlopeDirection == SlopeDirection.DownLeft)
                    {
                        if (tryGetGroup(Direction.Right, out var rightGroup))
                        {
                            addNeighbor(rightGroup.Up);

                            if (isValidSlope(rightGroup.Center, SlopeDirection.DownRight))
                                addNeighbor(rightGroup.Center);
                        }

                        if (tryGetGroupFromOffset(DOWN_LEFT, out var downLeftGroup))
                        {
                            addNeighbor(downLeftGroup.Up);

                            if (isValidSlope(downLeftGroup.Center, SlopeDirection.DownLeft))
                                addNeighbor(downLeftGroup.Center);
                        }
                    }
                }
                else
                {
                    void addLocalLeftAndRightSurfaces(
                        Direction left,
                        Direction right,
                        SlopeDirection localUpLeft,
                        SlopeDirection localUpRight,
                        SlopeDirection localDownLeft,
                        SlopeDirection localDownRight,
                        Vector3Int downLeftOffset,
                        Vector3Int downRightOffset
                    )
                    {
                        if (group.TryGetPositionInDirection(left, out var leftPosition))
                            addNeighbor(leftPosition);
                        else if (tryGetGroup(left, out var leftGroup))
                        {
                            if (isValidSlope(leftGroup.Center, localUpLeft))
                                addNeighbor(leftGroup.Center);

                            if (
                                leftGroup.TryGetPositionInDirection(
                                    currentSurface.SurfaceDirection,
                                    out var leftSurface
                                )
                            )
                                addNeighbor(leftSurface);
                        }

                        if (group.TryGetPositionInDirection(right, out var rightPosition))
                            addNeighbor(rightPosition);
                        else if (tryGetGroup(right, out var rightGroup))
                        {
                            if (isValidSlope(rightGroup.Center, localUpRight))
                                addNeighbor(rightGroup.Center);

                            if (
                                rightGroup.TryGetPositionInDirection(
                                    currentSurface.SurfaceDirection,
                                    out var rightSurface
                                )
                            )
                                addNeighbor(rightSurface);
                        }

                        if (tryGetGroupFromOffset(downLeftOffset, out var downLeftGroup))
                        {
                            if (isValidSlope(downLeftGroup.Center, localDownLeft))
                                addNeighbor(downLeftGroup.Center);
                            else if (
                                downLeftGroup.TryGetPositionInDirection(
                                    right,
                                    out var downLeftRightPosition
                                )
                            )
                                addNeighbor(downLeftRightPosition);
                        }

                        if (tryGetGroupFromOffset(downRightOffset, out var downRightGroup))
                        {
                            if (isValidSlope(downRightGroup.Center, localDownRight))
                                addNeighbor(downRightGroup.Center);
                            else if (
                                downRightGroup.TryGetPositionInDirection(
                                    left,
                                    out var downRightLeftPosition
                                )
                            )
                                addNeighbor(downRightLeftPosition);
                        }
                    }

                    switch (currentSurface.SurfaceDirection)
                    {
                        case Direction.Up:
                            addLocalLeftAndRightSurfaces(
                                Direction.Right,
                                Direction.Left,
                                SlopeDirection.DownRight,
                                SlopeDirection.DownLeft,
                                SlopeDirection.UpRight,
                                SlopeDirection.UpLeft,
                                UP_RIGHT,
                                UP_LEFT
                            );
                            break;
                        case Direction.Down:
                            addLocalLeftAndRightSurfaces(
                                Direction.Left,
                                Direction.Right,
                                SlopeDirection.UpLeft,
                                SlopeDirection.UpRight,
                                SlopeDirection.DownLeft,
                                SlopeDirection.DownRight,
                                DOWN_LEFT,
                                DOWN_RIGHT
                            );
                            break;
                        case Direction.Left:
                            addLocalLeftAndRightSurfaces(
                                Direction.Up,
                                Direction.Down,
                                SlopeDirection.DownLeft,
                                SlopeDirection.UpLeft,
                                SlopeDirection.DownRight,
                                SlopeDirection.UpRight,
                                UP_LEFT,
                                DOWN_LEFT
                            );
                            break;
                        case Direction.Right:
                            addLocalLeftAndRightSurfaces(
                                Direction.Down,
                                Direction.Up,
                                SlopeDirection.UpRight,
                                SlopeDirection.DownRight,
                                SlopeDirection.UpLeft,
                                SlopeDirection.DownLeft,
                                DOWN_RIGHT,
                                UP_RIGHT
                            );
                            break;
                    }

                    if (
                        tryGetGroup(Direction.Forward, out var forwardGroup)
                        && forwardGroup.TryGetPositionInDirection(
                            currentSurface.SurfaceDirection,
                            out var forwardSurface
                        )
                    )
                        addNeighbor(forwardSurface);

                    if (
                        tryGetGroup(Direction.Back, out var backGroup)
                        && backGroup.TryGetPositionInDirection(
                            currentSurface.SurfaceDirection,
                            out var backSurface
                        )
                    )
                        addNeighbor(backSurface);

                    if (currentSurface.SurfaceDirection == Direction.Up)
                    {
                        var offset = Vector3Int.down;
                        while (tryGetGroupFromOffset(offset, out var belowGroup))
                        {
                            if (belowGroup.Center != null && belowGroup.Down != null)
                            {
                                addNeighbor(belowGroup.Down);
                                break;
                            }

                            if (
                                belowGroup.Up != null
                                || belowGroup.Down != null
                                || belowGroup.Center is SurfaceEntityPosition
                            )
                                break;

                            offset += Vector3Int.down;
                        }
                    }
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
