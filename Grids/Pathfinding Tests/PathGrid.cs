using System.Collections.Generic;
using UnityEngine;

namespace Shears.Grids
{
    [RequireComponent(typeof(ShearsGrid))]
    public class PathGrid : ShearsBehaviour
    {
        private readonly Dictionary<GridNode, EntityPositionGroup> nodePositions = new();
        private ShearsGrid grid;

        private ShearsGrid Grid => this.LazyGet(ref grid);
        public float NodeSize => Grid.NodeSize;

        private readonly struct EntityPositionGroup
        {
            public EntityPosition Center { get; }
            public EntityPosition Top { get; }
            public EntityPosition Bottom { get; }
            public EntityPosition Left { get; }
            public EntityPosition Right { get; }

            public EntityPositionGroup(
                EntityPosition center,
                EntityPosition top,
                EntityPosition bottom,
                EntityPosition left,
                EntityPosition right
            )
            {
                Center = center;
                Top = top;
                Bottom = bottom;
                Left = left;
                Right = right;
            }

            public void Dispose()
            {
                Top?.Dispose();
                Bottom?.Dispose();
                Left?.Dispose();
                Right?.Dispose();
            }

            public bool TryGetPositionInDirection(Direction direction, out EntityPosition position)
            {
                position = direction switch
                {
                    Direction.Up => Top,
                    Direction.Down => Bottom,
                    Direction.Left => Left,
                    Direction.Right => Right,
                    _ => null,
                };

                return position != null;
            }

            public EntityPosition GetClosestSurfaceFromOriginDirection(Direction direction)
            {
                return direction switch
                {
                    Direction.Up => FirstValid(Bottom, Left, Right, Top),
                    Direction.Down => FirstValid(Top, Left, Right, Bottom),
                    Direction.Left => FirstValid(Right, Bottom, Top, Left),
                    Direction.Right => FirstValid(Left, Bottom, Top, Right),
                    Direction.Forward => FirstValid(Bottom, Left, Right, Top),
                    Direction.Back => FirstValid(Bottom, Left, Right, Top),
                    _ => null,
                };
            }

            private static EntityPosition FirstValid(
                EntityPosition p0,
                EntityPosition p1,
                EntityPosition p2,
                EntityPosition p3
            )
            {
                if (p0 is SurfaceEntityPosition)
                    return p0;
                else if (p1 is SurfaceEntityPosition)
                    return p1;
                else if (p2 is SurfaceEntityPosition)
                    return p2;
                else if (p3 is SurfaceEntityPosition)
                    return p3;
                else
                    return null;
            }
        }

        private void Awake()
        {
            UpdateSurfaces();

            var info = new List<GridNodeInfo<DoorwayNodeData>>();
            GetAllNodeData(info);

            info[0].Data.ConnectedNode = Grid.GetNode(info[1].GridPosition);
            info[2].Data.ConnectedNode = Grid.GetNode(info[3].GridPosition);
        }

        public bool TryGetNodeData<T>(out GridNodeInfo<T> info)
            where T : GridNodeData => Grid.TryGetNodeData(out info);

        public void GetAllNodeData<T>(List<GridNodeInfo<T>> info)
            where T : GridNodeData => Grid.GetAllNodeData(info);

        public Vector3 GridToWorld(Vector3Int gridPosition) =>
            Grid.transform.TransformPoint(NodeSize * (Vector3)gridPosition);

        public bool TryGetPosition(
            Vector3 worldPosition,
            out EntityPosition position,
            Direction surfaceBias = Direction.Down
        )
        {
            if (TryGetSurfacePosition(worldPosition, out var surfacePosition, surfaceBias))
            {
                position = surfacePosition;
                return true;
            }

            if (!Grid.TryGetNodeForWorldPosition(worldPosition, out var node))
            {
                LogWarning($"Could not get node for world position: {worldPosition}.");
                position = null;
                return false;
            }

            if (!nodePositions.TryGetValue(node, out var group))
            {
                LogVerbose($"Position is obstructed for node: {node}.");
                position = null;
                return false;
            }

            position = group.GetClosestSurfaceFromOriginDirection(surfaceBias);
            return position != null;
        }

        public bool TryGetSurfacePosition(
            Vector3Int GridPosition,
            out SurfaceEntityPosition groundPosition,
            Direction direction = Direction.Down
        )
        {
            if (!Grid.TryGetNode(GridPosition, out var node))
            {
                LogWarning($"Could not get node for Grid position: {GridPosition}.");
                groundPosition = null;
                return false;
            }

            return TryGetSurfacePosition(node, out groundPosition, direction);
        }

        public bool TryGetSurfacePosition(
            Vector3 worldPosition,
            out SurfaceEntityPosition groundPosition,
            Direction direction = Direction.Down
        )
        {
            if (!Grid.TryGetNodeForWorldPosition(worldPosition, out var node))
            {
                LogWarning($"Could not get node for world position: {worldPosition}.");
                groundPosition = null;
                return false;
            }

            return TryGetSurfacePosition(node, out groundPosition, direction);
        }

        public bool TryGetSurfacePosition(
            EntityPosition position,
            out SurfaceEntityPosition groundPosition,
            Direction direction = Direction.Down
        )
        {
            if (!Grid.TryGetNode(position.GridPosition, out var node))
            {
                LogWarning($"Could not get node for Grid position: {position.GridPosition}.");
                groundPosition = null;
                return false;
            }

            return TryGetSurfacePosition(node, out groundPosition, direction);
        }

        public void GetNeighbors(EntityPosition position, List<EntityPosition> neighbors)
        {
            if (!Grid.TryGetNode(position.GridPosition, out var node))
            {
                neighbors.Clear();
                LogWarning($"Could not get node for Grid position: {position.GridPosition}.");
                return;
            }

            GetNeighbors(node, neighbors);
        }

        public bool TryGetClosestFloorPosition(
            EntityPosition currentPosition,
            out SurfaceEntityPosition floorPosition
        )
        {
            if (!Grid.TryGetNode(currentPosition.GridPosition, out var previousNode))
            {
                floorPosition = null;
                return false;
            }

            var currentGridPosition = currentPosition.GridPosition;
            currentGridPosition.y--;

            while (currentGridPosition.y >= 0)
            {
                if (!Grid.TryGetNode(currentGridPosition, out var node))
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

        private bool TryGetSurfacePosition(
            GridNode node,
            out SurfaceEntityPosition groundPosition,
            Direction direction
        )
        {
            groundPosition = null;

            if (!nodePositions.TryGetValue(node, out var group))
            {
                LogWarning($"Ground position is obstructed for node: {node}.");

                return false;
            }

            if (
                group.TryGetPositionInDirection(direction, out var position)
                && position is SurfaceEntityPosition surface
            )
                groundPosition = surface;

            return groundPosition != null;
        }

        private void GetNeighbors(GridNode node, List<EntityPosition> neighbors)
        {
            neighbors.Clear();

            var GridPosition = node.GridPosition;
            EntityPosition downLeft = null;
            EntityPosition downRight = null;
            EntityPosition upLeft = null;
            EntityPosition upRight = null;

            if (Grid.TryGetNode(GridPosition.With(y: GridPosition.y + 1), out var upNode))
            {
                TryGetPositionInDirection(upNode, Direction.Left, out upLeft);
                TryGetPositionInDirection(upNode, Direction.Right, out upRight);
            }

            if (Grid.TryGetNode(GridPosition.With(y: GridPosition.y - 1), out var downNode))
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

            if (
                group.TryGetPositionInDirection(direction, out var possiblePosition)
                && possiblePosition is SurfaceEntityPosition
            )
            {
                position = possiblePosition;
                return true;
            }

            var nextNodePosition = node.GridPosition + direction.ToVectorInt();

            if (!Grid.TryGetNode(nextNodePosition, out var nextNode))
                return false;

            if (!nodePositions.TryGetValue(nextNode, out var nextGroup))
            {
                LogWarning("Target position is obstructed!");
                return false;
            }

            position = nextGroup.GetClosestSurfaceFromOriginDirection(direction);

            return position != null;
        }

        public void InsertGrid(PathGrid Grid, List<GridNode> clonedNodes = null)
        {
            this.Grid.InsertGrid(Grid.Grid, clonedNodes);

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
            Grid.Shift(rangeStart, rangeEnd, direction, distance, ignorePositions);

            if (updateSurfaces)
                UpdateSurfaces();
        }

        private void UpdateSurfaces()
        {
            foreach (var position in nodePositions.Values)
                position.Dispose();

            nodePositions.Clear();

            foreach (var node in Grid.Nodes)
            {
                if (node == null)
                    continue;

                nodePositions[node] = GetPositionGroup(node);
            }
        }

        private EntityPositionGroup GetPositionGroup(GridNode node)
        {
            var worldPosition = Grid.GetWorldPosition(node);
            var GridPosition = node.GridPosition;

            EntityPosition top = null;
            EntityPosition bottom = null;
            EntityPosition left = null;
            EntityPosition right = null;

            if (node.TryGetData(out SurfaceNodeData surface))
            {
                if (!surface.IsSlope)
                    return new(null, null, null, null, null);

                bottom = new SurfaceEntityPosition(
                    node,
                    GridPosition,
                    worldPosition,
                    node,
                    GridPosition,
                    surface.SlopingDirection.GetNormal(Grid),
                    Direction.Up,
                    surface
                );

                return new(null, null, bottom, null, null);
            }

            if (Grid.TryGetNode(GridPosition.With(y: GridPosition.y + 1), out var upNode))
            {
                if (upNode.TryGetData(out SurfaceNodeData upSurface))
                {
                    var upWorldPosition = Grid.GetWorldPosition(upNode);
                    var surfacePosition = 0.5f * (worldPosition + upWorldPosition);
                    var normal = Grid.transform.TransformDirection(Vector3.down);

                    top = new SurfaceEntityPosition(
                        node,
                        GridPosition,
                        surfacePosition,
                        upNode,
                        upNode.GridPosition,
                        normal,
                        Direction.Up,
                        upSurface
                    );
                }
            }

            if (Grid.TryGetNode(GridPosition.With(y: GridPosition.y - 1), out var downNode))
            {
                var downWorldPosition = Grid.GetWorldPosition(downNode);

                if (downNode.TryGetData(out SurfaceNodeData downSurface))
                {
                    var surfacePosition = 0.5f * (worldPosition + downWorldPosition);
                    var normal = Grid.transform.TransformDirection(Vector3.up);

                    bottom = new SurfaceEntityPosition(
                        node,
                        GridPosition,
                        surfacePosition,
                        downNode,
                        downNode.GridPosition,
                        normal,
                        Direction.Down,
                        downSurface
                    );
                }
            }

            if (Grid.TryGetNode(GridPosition.With(x: GridPosition.x - 1), out var leftNode))
            {
                var leftWorldPosition = Grid.GetWorldPosition(leftNode);

                if (leftNode.TryGetData(out SurfaceNodeData leftSurface))
                {
                    var surfacePosition = 0.5f * (worldPosition + leftWorldPosition);
                    var normal = Grid.transform.TransformDirection(Vector3.right);

                    left = new SurfaceEntityPosition(
                        node,
                        GridPosition,
                        surfacePosition,
                        leftNode,
                        leftNode.GridPosition,
                        normal,
                        Direction.Left,
                        leftSurface
                    );
                }
            }

            if (Grid.TryGetNode(GridPosition.With(x: GridPosition.x + 1), out var rightNode))
            {
                var rightWorldPosition = Grid.GetWorldPosition(rightNode);

                if (rightNode.TryGetData(out SurfaceNodeData rightSurface))
                {
                    var surfacePosition = 0.5f * (worldPosition + rightWorldPosition);
                    var normal = Grid.transform.TransformDirection(Vector3.left);

                    right = new SurfaceEntityPosition(
                        node,
                        GridPosition,
                        surfacePosition,
                        rightNode,
                        rightNode.GridPosition,
                        normal,
                        Direction.Right,
                        rightSurface
                    );
                }
            }

            var center = new AirEntityPosition(node, node.GridPosition, worldPosition);

            return new(center, top, bottom, left, right);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;

            foreach (var node in Grid.Nodes)
            {
                if (nodePositions.TryGetValue(node, out var group))
                {
                    if (group.Top is SurfaceEntityPosition topSurface)
                        Gizmos.DrawRay(topSurface.WorldPosition, topSurface.SurfaceNormal);

                    if (group.Right is SurfaceEntityPosition rightSurface)
                        Gizmos.DrawRay(rightSurface.WorldPosition, rightSurface.SurfaceNormal);

                    if (group.Bottom is SurfaceEntityPosition bottomSurface)
                        Gizmos.DrawRay(bottomSurface.WorldPosition, bottomSurface.SurfaceNormal);

                    if (group.Left is SurfaceEntityPosition leftSurface)
                        Gizmos.DrawRay(leftSurface.WorldPosition, leftSurface.SurfaceNormal);
                }
            }
        }
    }
}
