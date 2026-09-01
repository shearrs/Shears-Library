using System.Collections.Generic;
using UnityEngine;

namespace Shears.Grids
{
    [RequireComponent(typeof(ShearsGrid))]
    public class PathGrid : ShearsBehaviour
    {
        private readonly Dictionary<GridNode, EntityPositionGroup> nodePositions = new();
        private ShearsGrid grid;

        public float NodeSize => grid.NodeSize;

        private readonly struct EntityPositionGroup
        {
            public EntityPosition Top { get; }
            public EntityPosition Bottom { get; }
            public EntityPosition Left { get; }
            public EntityPosition Right { get; }

            public EntityPositionGroup(
                EntityPosition top,
                EntityPosition bottom,
                EntityPosition left,
                EntityPosition right
            )
            {
                Top = top;
                Bottom = bottom;
                Left = left;
                Right = right;
            }

            public EntityPosition GetClosestPositionFromOriginDirection(Direction direction)
            {
                static EntityPosition firstValid(
                    EntityPosition p0,
                    EntityPosition p1,
                    EntityPosition p2,
                    EntityPosition p3
                )
                {
                    if (p0 != null)
                        return p0;
                    else if (p1 != null)
                        return p1;
                    else if (p2 != null)
                        return p2;
                    else
                        return p3;
                }

                return direction switch
                {
                    Direction.Up => firstValid(Bottom, Left, Right, Top),
                    Direction.Down => firstValid(Top, Left, Right, Bottom),
                    Direction.Left => firstValid(Right, Bottom, Top, Left),
                    Direction.Right => firstValid(Left, Bottom, Top, Right),
                    Direction.Forward => firstValid(Bottom, Left, Right, Top),
                    Direction.Back => firstValid(Bottom, Left, Right, Top),
                    _ => null,
                };
            }
        }

        private void Awake()
        {
            grid = GetComponent<ShearsGrid>();

            UpdateSurfaces();
        }

        public bool TryGetGroundPosition(
            Vector3Int gridPosition,
            out SurfaceEntityPosition groundPosition
        )
        {
            if (!grid.TryGetNode(gridPosition, out var node))
            {
                LogWarning($"Could not get node for grid position: {gridPosition}.");
                groundPosition = null;
                return false;
            }

            return TryGetGroundPosition(node, out groundPosition);
        }

        public bool TryGetGroundPosition(
            Vector3 worldPosition,
            out SurfaceEntityPosition groundPosition
        )
        {
            if (!grid.TryGetNodeForWorldPosition(worldPosition, out var node))
            {
                LogWarning($"Could not get node for world position: {worldPosition}.");
                groundPosition = null;
                return false;
            }

            return TryGetGroundPosition(node, out groundPosition);
        }

        public bool TryGetGroundPosition(
            EntityPosition position,
            out SurfaceEntityPosition groundPosition
        )
        {
            if (!grid.TryGetNode(position.GridPosition, out var node))
            {
                LogWarning($"Could not get node for grid position: {position.GridPosition}.");
                groundPosition = null;
                return false;
            }

            return TryGetGroundPosition(node, out groundPosition);
        }

        public void GetNeighbors(EntityPosition position, List<EntityPosition> neighbors)
        {
            if (!grid.TryGetNode(position.GridPosition, out var node))
            {
                neighbors.Clear();
                LogWarning($"Could not get node for grid position: {position.GridPosition}.");
                return;
            }

            GetNeighbors(node, neighbors);
        }

        public bool TryGetClosestFloorPosition(
            EntityPosition currentPosition,
            out SurfaceEntityPosition floorPosition
        )
        {
            if (!grid.TryGetNode(currentPosition.GridPosition, out var previousNode))
            {
                floorPosition = null;
                return false;
            }

            var currentGridPosition = currentPosition.GridPosition;
            currentGridPosition.y--;

            while (currentGridPosition.y >= 0)
            {
                if (!grid.TryGetNode(currentGridPosition, out var node))
                    break;

                if (node.TryGetData(out SurfaceNodeData _))
                {
                    if (
                        TryGetPositionInDirection(
                            previousNode,
                            Direction.Down,
                            out var surfacePosition
                        )
                    )
                    {
                        floorPosition = surfacePosition as SurfaceEntityPosition;
                        return true;
                    }
                }

                previousNode = node;
                currentGridPosition.y--;
            }

            floorPosition = null;
            return false;
        }

        private bool TryGetGroundPosition(GridNode node, out SurfaceEntityPosition groundPosition)
        {
            groundPosition = null;

            if (!nodePositions.TryGetValue(node, out var group))
            {
                LogWarning($"Ground position is obstructed for node: {node}.");

                return false;
            }

            if (group.Bottom != null && group.Bottom is SurfaceEntityPosition surface)
                groundPosition = surface;

            return true;
        }

        private void GetNeighbors(GridNode node, List<EntityPosition> neighbors)
        {
            neighbors.Clear();

            var gridPosition = node.GridPosition;
            EntityPosition downLeft = null;
            EntityPosition downRight = null;
            EntityPosition upLeft = null;
            EntityPosition upRight = null;

            if (grid.TryGetNode(gridPosition.With(y: gridPosition.y + 1), out var upNode))
            {
                TryGetPositionInDirection(upNode, Direction.Left, out upLeft);
                TryGetPositionInDirection(upNode, Direction.Right, out upRight);
            }

            if (grid.TryGetNode(gridPosition.With(y: gridPosition.y - 1), out var downNode))
            {
                TryGetPositionInDirection(downNode, Direction.Left, out downLeft);
                TryGetPositionInDirection(downNode, Direction.Right, out downRight);
            }

            TryGetPositionInDirection(node, Direction.Up, out var up);
            TryGetPositionInDirection(node, Direction.Down, out var down);
            TryGetPositionInDirection(node, Direction.Left, out var left);
            TryGetPositionInDirection(node, Direction.Right, out var right);
            TryGetPositionInDirection(node, Direction.Forward, out var forward);
            TryGetPositionInDirection(node, Direction.Back, out var back);

            neighbors.Add(up);
            neighbors.Add(down);
            neighbors.Add(left);
            neighbors.Add(right);
            neighbors.Add(forward);
            neighbors.Add(back);
            neighbors.Add(upLeft);
            neighbors.Add(upRight);
            neighbors.Add(downLeft);
            neighbors.Add(downRight);
        }

        private bool TryGetPositionInDirection(
            GridNode node,
            Direction direction,
            out EntityPosition position
        )
        {
            position = null;

            if (!nodePositions.TryGetValue(node, out var group))
            {
                LogWarning("Current position is obstructed!");
                return false;
            }

            if (direction == Direction.Up && group.Top != null)
            {
                position = group.Top;
                return true;
            }
            else if (direction == Direction.Down && group.Bottom != null)
            {
                position = group.Bottom;
                return true;
            }
            else if (direction == Direction.Left && group.Left != null)
            {
                position = group.Left;
                return true;
            }
            else if (direction == Direction.Right && group.Right != null)
            {
                position = group.Right;
                return true;
            }

            var nextNodePosition = node.GridPosition + direction.ToVectorInt();

            if (!grid.TryGetNode(nextNodePosition, out var nextNode))
                return false;

            if (!nodePositions.TryGetValue(nextNode, out var nextGroup))
            {
                LogWarning("Target position is obstructed!");
                return false;
            }

            position = nextGroup.GetClosestPositionFromOriginDirection(direction);

            return position != null;
        }

        public void InsertGrid(PathGrid grid, List<GridNode> clonedNodes = null)
        {
            this.grid.InsertGrid(grid.grid, clonedNodes);

            UpdateSurfaces();
        }

        public void Shift(
            Vector3Int rangeStart,
            Vector3Int rangeEnd,
            Direction direction,
            int distance,
            HashSet<Vector3Int> ignorePositions = null,
            bool updateSurfaces = true
        )
        {
            grid.Shift(rangeStart, rangeEnd, direction, distance, ignorePositions);

            if (updateSurfaces)
                UpdateSurfaces();
        }

        private void UpdateSurfaces()
        {
            nodePositions.Clear();

            foreach (var node in grid.Nodes)
            {
                if (node == null)
                    continue;

                nodePositions[node] = GetPositionGroup(node);
            }
        }

        private EntityPositionGroup GetPositionGroup(GridNode node)
        {
            var worldPosition = grid.GetWorldPosition(node);
            var gridPosition = node.GridPosition;

            EntityPosition top = null;
            EntityPosition bottom = null;
            EntityPosition left = null;
            EntityPosition right = null;

            if (node.TryGetData(out SurfaceNodeData surface))
            {
                if (!surface.IsSlope)
                    return new(null, null, null, null);

                bottom = new SurfaceEntityPosition(
                    node,
                    gridPosition,
                    worldPosition,
                    node.GridPosition,
                    surface.SlopingDirection.GetNormal(grid),
                    Direction.Up,
                    surface
                );

                return new(null, bottom, null, null);
            }

            if (grid.TryGetNode(gridPosition.With(y: gridPosition.y + 1), out var upNode))
            {
                var upWorldPosition = grid.GetWorldPosition(upNode);

                if (upNode.TryGetData(out SurfaceNodeData upSurface))
                {
                    var surfacePosition = 0.5f * (worldPosition + upWorldPosition);
                    var normal = grid.transform.TransformDirection(Vector3.down);

                    top = new SurfaceEntityPosition(
                        node,
                        gridPosition,
                        surfacePosition,
                        upNode.GridPosition,
                        normal,
                        Direction.Up,
                        upSurface
                    );
                }
                else
                    top = new AirEntityPosition(upNode, upNode.GridPosition, upWorldPosition);
            }

            if (grid.TryGetNode(gridPosition.With(y: gridPosition.y - 1), out var downNode))
            {
                var downWorldPosition = grid.GetWorldPosition(downNode);

                if (downNode.TryGetData(out SurfaceNodeData downSurface))
                {
                    var surfacePosition = 0.5f * (worldPosition + downWorldPosition);
                    var normal = grid.transform.TransformDirection(Vector3.up);

                    bottom = new SurfaceEntityPosition(
                        node,
                        gridPosition,
                        surfacePosition,
                        downNode.GridPosition,
                        normal,
                        Direction.Down,
                        downSurface
                    );
                }
                else
                    bottom = new AirEntityPosition(
                        downNode,
                        downNode.GridPosition,
                        downWorldPosition
                    );
            }

            if (grid.TryGetNode(gridPosition.With(x: gridPosition.x - 1), out var leftNode))
            {
                var leftWorldPosition = grid.GetWorldPosition(leftNode);

                if (leftNode.TryGetData(out SurfaceNodeData leftSurface))
                {
                    var surfacePosition = 0.5f * (worldPosition + leftWorldPosition);
                    var normal = grid.transform.TransformDirection(Vector3.right);

                    left = new SurfaceEntityPosition(
                        node,
                        gridPosition,
                        surfacePosition,
                        leftNode.GridPosition,
                        normal,
                        Direction.Left,
                        leftSurface
                    );
                }
                else
                    left = new AirEntityPosition(
                        leftNode,
                        leftNode.GridPosition,
                        leftWorldPosition
                    );
            }

            if (grid.TryGetNode(gridPosition.With(x: gridPosition.x + 1), out var rightNode))
            {
                var rightWorldPosition = grid.GetWorldPosition(rightNode);

                if (rightNode.TryGetData(out SurfaceNodeData rightSurface))
                {
                    var surfacePosition = 0.5f * (worldPosition + rightWorldPosition);
                    var normal = grid.transform.TransformDirection(Vector3.left);

                    right = new SurfaceEntityPosition(
                        node,
                        gridPosition,
                        surfacePosition,
                        rightNode.GridPosition,
                        normal,
                        Direction.Right,
                        rightSurface
                    );
                }
                else
                    right = new AirEntityPosition(
                        rightNode,
                        rightNode.GridPosition,
                        rightWorldPosition
                    );
            }

            return new(top, bottom, left, right);
        }
    }
}
