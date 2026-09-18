using System.Collections.Generic;
using Shears.Grids;
using UnityEngine;
using static Shears.Pathfinding.SurfaceNodeData;

namespace Shears.Pathfinding
{
    [RequireComponent(typeof(ShearsGrid))]
    public class PathGrid : ShearsBehaviour
    {
        private readonly Dictionary<GridNode, EntityPositionGroup> nodePositions = new();
        private ShearsGrid grid;

        private ShearsGrid Grid => this.LazyGet(ref grid);
        public Vector3Int Size => grid.Size;
        public float NodeSize => Grid.NodeSize;

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

        public Vector3 GridToWorld(Vector3Int gridPosition) => Grid.GridToWorld(gridPosition);

        public Vector3 GetCenter() => Grid.GetCenter();

        public void GetPositionsInBounds(Bounds worldBounds, List<EntityPosition> positions)
        {
            CollectionUtil.GetPooled(out List<GridNode> nodes);

            Grid.GetNodesInBounds(worldBounds, nodes);

            foreach (var node in nodes)
            {
                if (!nodePositions.TryGetValue(node, out var group))
                    continue;

                positions.Add(group.Up);
                positions.Add(group.Down);
                positions.Add(group.Left);
                positions.Add(group.Right);
                positions.Add(group.Center);
            }

            CollectionUtil.ReleasePooled(nodes);
        }

        public void GetNodesInBounds(Bounds worldBounds, List<GridNode> nodes) =>
            Grid.GetNodesInBounds(worldBounds, nodes);

        public bool TryGetPositionGroup(Vector3 worldPosition, out EntityPositionGroup group)
        {
            group = default;

            if (!Grid.TryGetNodeForWorldPosition(worldPosition, out var node))
            {
                LogWarning($"Could not get node for world position: {worldPosition}.");
                return false;
            }

            if (!nodePositions.TryGetValue(node, out group))
            {
                LogVerbose($"Position is obstructed for node: {node}.");
                return false;
            }
            else
                return true;
        }

        public bool TryGetPositionGroup(Vector3Int gridPosition, out EntityPositionGroup group)
        {
            group = default;

            if (!Grid.TryGetNode(gridPosition, out var node))
                return false;

            if (!nodePositions.TryGetValue(node, out group))
            {
                LogVerbose($"Position is obstructed for node: {node}.");
                return false;
            }
            else
                return true;
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

            SurfaceEntityPosition up = null;
            SurfaceEntityPosition down = null;
            SurfaceEntityPosition left = null;
            SurfaceEntityPosition right = null;
            EntityPosition center;

            if (node.TryGetData(out SurfaceNodeData surface))
            {
                if (!surface.IsSlope)
                    return new(null, null, null, null, null);

                center = new SurfaceEntityPosition(
                    node,
                    GridPosition,
                    worldPosition,
                    node,
                    GridPosition,
                    surface.SlopingDirection.GetNormal(Grid),
                    Direction.Up,
                    surface
                );

                return new(center, null, null, null, null);
            }

            if (Grid.TryGetNode(GridPosition.With(y: GridPosition.y + 1), out var upNode))
            {
                if (upNode.TryGetData(out SurfaceNodeData upSurface))
                {
                    if (
                        upSurface.IsSlope
                        && (
                            upSurface.SlopingDirection == SlopeDirection.DownLeft
                            || upSurface.SlopingDirection == SlopeDirection.DownRight
                        )
                    )
                        up = null;
                    else
                    {
                        var upWorldPosition = Grid.GetWorldPosition(upNode);
                        var surfacePosition = 0.5f * (worldPosition + upWorldPosition);
                        var normal = Grid.transform.TransformDirection(Vector3.down);

                        up = new SurfaceEntityPosition(
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
            }

            if (Grid.TryGetNode(GridPosition.With(y: GridPosition.y - 1), out var downNode))
            {
                var downWorldPosition = Grid.GetWorldPosition(downNode);

                if (downNode.TryGetData(out SurfaceNodeData downSurface))
                {
                    if (
                        downSurface.IsSlope
                        && (
                            downSurface.SlopingDirection == SlopeDirection.UpLeft
                            || downSurface.SlopingDirection == SlopeDirection.UpRight
                        )
                    )
                        down = null;
                    else
                    {
                        var surfacePosition = 0.5f * (worldPosition + downWorldPosition);
                        var normal = Grid.transform.TransformDirection(Vector3.up);

                        down = new SurfaceEntityPosition(
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
            }

            if (Grid.TryGetNode(GridPosition.With(x: GridPosition.x - 1), out var leftNode))
            {
                var leftWorldPosition = Grid.GetWorldPosition(leftNode);

                if (leftNode.TryGetData(out SurfaceNodeData leftSurface))
                {
                    if (
                        leftSurface.IsSlope
                        && (
                            leftSurface.SlopingDirection == SlopeDirection.UpLeft
                            || leftSurface.SlopingDirection == SlopeDirection.DownLeft
                        )
                    )
                        left = null;
                    else
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
            }

            if (Grid.TryGetNode(GridPosition.With(x: GridPosition.x + 1), out var rightNode))
            {
                var rightWorldPosition = Grid.GetWorldPosition(rightNode);

                if (rightNode.TryGetData(out SurfaceNodeData rightSurface))
                {
                    if (
                        rightSurface.IsSlope
                        && (
                            rightSurface.SlopingDirection == SlopeDirection.UpRight
                            || rightSurface.SlopingDirection == SlopeDirection.DownRight
                        )
                    )
                        right = null;
                    else
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
            }

            center = new AirEntityPosition(node, node.GridPosition, worldPosition);

            return new(center, up, down, left, right);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;

            foreach (var node in Grid.Nodes)
            {
                if (nodePositions.TryGetValue(node, out var group))
                {
                    if (group.Up is SurfaceEntityPosition topSurface)
                        Gizmos.DrawRay(topSurface.WorldPosition, topSurface.SurfaceNormal);

                    if (group.Right is SurfaceEntityPosition rightSurface)
                        Gizmos.DrawRay(rightSurface.WorldPosition, rightSurface.SurfaceNormal);

                    if (group.Down is SurfaceEntityPosition bottomSurface)
                        Gizmos.DrawRay(bottomSurface.WorldPosition, bottomSurface.SurfaceNormal);

                    if (group.Left is SurfaceEntityPosition leftSurface)
                        Gizmos.DrawRay(leftSurface.WorldPosition, leftSurface.SurfaceNormal);
                }
            }
        }
    }
}
