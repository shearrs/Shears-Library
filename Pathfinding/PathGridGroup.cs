using System;
using System.Collections.Generic;
using System.Linq;
using Shears.Logging;
using UnityEngine;

namespace Shears.Pathfinding
{
    public class PathGridGroup : MonoBehaviour, IPathGrid, ISHLoggable
    {
        [field: SerializeField]
        public SHLogLevels LogLevels { get; set; } = SHLogUtil.Default;

        [SerializeField]
        private List<PathNode> nodes = new();

        [SerializeField, Delayed]
        private float nodeSize = 1.0f;

        [SerializeField, ReadOnly]
        private Vector3Int gridSize;

        private readonly List<PathNode> newNodes = new();

        public IReadOnlyList<PathNode> Nodes => nodes;
        public Vector3Int GridSize => gridSize;
        public float NodeSize => nodeSize;
        public Transform Transform => transform;

        public event Action GridChanged;

        public void UpdateWorldPositions()
        {
            if (Nodes.Count == 0)
                return;

            foreach (var node in Nodes)
            {
                Vector3 localPosition = new(
                    nodeSize * node.GridPosition.x,
                    nodeSize * node.GridPosition.y,
                    nodeSize * node.GridPosition.z
                );

                node.Size = nodeSize;
                node.WorldPosition = transform.TransformPoint(localPosition);

                if (node.NodeObject != null)
                    node.NodeObject.transform.position = node.WorldPosition;
            }

            GridChanged?.Invoke();
        }

        public Vector3 GetPositionForNode(PathNode node)
        {
            Vector3 localPosition = new(
                nodeSize * node.GridPosition.x,
                nodeSize * node.GridPosition.y,
                nodeSize * node.GridPosition.z
            );

            return transform.TransformPoint(localPosition);
        }

        public PathNode GetNodeForPosition(Vector3 worldPosition)
        {
            Vector3 gridWorldSize = nodeSize * ((Vector3)GridSize - Vector3.one);
            Vector3 center = GetCenter();
            worldPosition -= center;

            float xPercent = (worldPosition.x + (0.5f * gridWorldSize.x)) / gridWorldSize.x;
            float yPercent = (worldPosition.y + (0.5f * gridWorldSize.y)) / gridWorldSize.y;
            float zPercent = (worldPosition.z + (0.5f * gridWorldSize.z)) / gridWorldSize.z;

            xPercent = Mathf.Clamp01(xPercent);
            yPercent = Mathf.Clamp01(yPercent);
            zPercent = Mathf.Clamp01(zPercent);

            int x = Mathf.RoundToInt((GridSize.x - 1) * xPercent);
            int y = Mathf.RoundToInt((GridSize.y - 1) * yPercent);
            int z = Mathf.RoundToInt((GridSize.z - 1) * zPercent);

            return GetNode(x, y, z);
        }

        public void GetNodesInBounds(Bounds bounds, List<PathNode> boundsNodes)
        {
            boundsNodes.Clear();

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3 noZ = Vector3.one.With(z: 0);

            min -= transform.position - (0.5f * nodeSize * noZ);
            max -= transform.position + (0.5f * nodeSize * noZ);

            Vector3Int localMin = min.RoundToInt();
            Vector3Int localMax = max.RoundToInt();

            for (int x = localMin.x; x <= localMax.x; x++)
            {
                if (x < 0)
                    continue;
                else if (x >= GridSize.x)
                    break;

                for (int y = localMin.y; y <= localMax.y; y++)
                {
                    if (y < 0)
                        continue;
                    else if (y >= GridSize.y)
                        break;

                    for (int z = localMin.z; z <= localMax.z; z++)
                    {
                        if (z < 0)
                            continue;
                        else if (z >= GridSize.z)
                            break;

                        boundsNodes.Add(GetNode(x, y, z));
                    }
                }
            }
        }

        public void GetNeighbors(PathNode node, List<PathNode> neighbors)
        {
            Vector3Int gridPosition = node.GridPosition;
            neighbors.Clear();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        if (x == 0 && y == 0 && z == 0)
                            continue;

                        int gridX = gridPosition.x + x;
                        int gridY = gridPosition.y + y;
                        int gridZ = gridPosition.z + z;

                        if (
                            (gridX < 0 || gridX > GridSize.x - 1)
                            || (gridY < 0 || gridY > GridSize.y - 1)
                            || (gridZ < 0 || gridZ > GridSize.z - 1)
                        )
                            continue;

                        var neighbor = GetNode(gridX, gridY, gridZ);
                        neighbors.Add(neighbor);
                    }
                }
            }
        }

        public PathNode GetNode(Vector3Int position) => GetNode(position.x, position.y, position.z);

        public PathNode GetNode(int x, int y, int z)
        {
            if (x >= GridSize.x || y >= GridSize.y || z >= GridSize.z || x < 0 || y < 0 || z < 0)
            {
                SHLogger.Log($"Invalid coordinates for node: ({x}, {y}, {z})", SHLogLevels.Error);
                return null;
            }

            int index = (z * GridSize.y * GridSize.x) + (y * GridSize.x) + x;

            return Nodes[index];
        }

        public PathNode GetNodeWithData<T>()
            where T : PathNodeData
        {
            foreach (var node in Nodes)
            {
                if (node.TryGetData(out T _))
                    return node;
            }

            return null;
        }

        public Vector3 GetCenter()
        {
            return transform.position + (0.5f * nodeSize * ((Vector3)GridSize - Vector3.one));
        }

        public void Clear()
        {
            nodes.Clear();
            gridSize = Vector3Int.one;
        }

        public void Add(PathGrid grid, List<PathNode> clonedNodes = null)
        {
            clonedNodes?.Clear();

            if (grid.Nodes.Count == 0)
            {
                SHLogger.Log("Grid has no nodes to add!", SHLogLevels.Warning);
                return;
            }
            else if (Nodes.Count == 0)
                transform.position = grid.transform.position;

            newNodes.Clear();

            VectorUtil.Min(
                out var xMin,
                out var yMin,
                out var zMin,
                transform.position,
                grid.transform.position
            );

            var maxPos = Nodes.Count > 0 ? Nodes[^1].WorldPosition : transform.position;

            VectorUtil.Max(
                out var xMax,
                out var yMax,
                out var zMax,
                maxPos,
                grid.Nodes[^1].WorldPosition
            );

            var newGridSize =
                new Vector3(xMax - xMin, yMax - yMin, zMax - zMin).RoundToInt() + Vector3Int.one;

            var previousPosition = transform.position;
            transform.position = new Vector3(xMin, yMin, zMin);

            var offset = transform.position - previousPosition;

            for (int z = 0; z < newGridSize.z; z++)
            {
                for (int y = 0; y < newGridSize.y; y++)
                {
                    for (int x = 0; x < newGridSize.x; x++)
                    {
                        var worldPosition = new Vector3(
                            xMin + x * NodeSize,
                            yMin + y * NodeSize,
                            zMin + z * NodeSize
                        );
                        PathNode node;
                        bool currentGridNode = false;

                        node = grid.GetNodeInBounds(worldPosition);

                        if (GridSize.sqrMagnitude > 0 && (node == null || node.Data == null))
                        {
                            node = GetNodeInBounds(worldPosition + offset);
                            currentGridNode = node != null;
                        }

                        if (node == null)
                            node = new PathNode(new Vector3Int(x, y, z), worldPosition, nodeSize);
                        else
                        {
                            if (!currentGridNode) // if the node was already belonging to this grid, we don't need to clone a new one
                            {
                                node = node.Clone();
                                clonedNodes?.Add(node);
                            }

                            node.GridPosition = new Vector3Int(x, y, z);
                            node.WorldPosition = worldPosition;
                        }

                        if (node.NodeObject != null)
                        {
                            node.NodeObject.Grid = this;
                            node.NodeObject.Node = node;
                        }

                        newNodes.Add(node);
                    }
                }
            }

            nodes.Clear();
            nodes.AddRange(newNodes);
            gridSize = newGridSize;
        }

        public void Shift(
            Vector3Int rangeStart,
            Vector3Int rangeEnd,
            Direction direction,
            int distance
        )
        {
            if (Nodes.Count == 0)
            {
                this.Log("Grid has no nodes to shift.", SHLogLevels.Warning);
                return;
            }

            newNodes.Clear();

            var directionOffset = distance * direction.ToVectorInt();
            var offsetStart = rangeStart + directionOffset;
            var offsetEnd = rangeEnd + Vector3Int.one + directionOffset;

            VectorUtil.Min(out var xMin, out var yMin, out var zMin, Vector3Int.zero, offsetStart);
            VectorUtil.Max(out var xMax, out var yMax, out var zMax, GridSize, offsetEnd);

            var newGridSize = new Vector3Int(xMax - xMin, yMax - yMin, zMax - zMin);
            var minOffset = new Vector3Int(xMin, yMin, zMin);
            var targetMin = rangeStart - minOffset;
            var targetMax = rangeEnd - minOffset;
            var affectedMin = targetMin + directionOffset;
            var affectedMax = targetMax + directionOffset;

            transform.position += NodeSize * (Vector3)minOffset;

            for (int z = 0; z < newGridSize.z; z++)
            {
                for (int y = 0; y < newGridSize.y; y++)
                {
                    for (int x = 0; x < newGridSize.x; x++)
                    {
                        var gridPosition = new Vector3Int(x, y, z);
                        var worldPosition = transform.position + (NodeSize * new Vector3(x, y, z));
                        PathNode node;

                        if (VectorUtil.WithinRange(gridPosition, affectedMin, affectedMax))
                        {
                            var offset = gridPosition - affectedMin;
                            var originalPos = rangeStart + offset;
                            var originalNode = GetNode(originalPos);
                            node = originalNode;
                        }
                        else if (VectorUtil.WithinRange(gridPosition, targetMin, targetMax))
                            node = new PathNode(gridPosition, worldPosition, nodeSize);
                        else
                        {
                            node = GetNodeInBounds(worldPosition);

                            node ??= new PathNode(gridPosition, worldPosition, nodeSize);
                        }

                        node.GridPosition = gridPosition;
                        node.WorldPosition = worldPosition;

                        newNodes.Add(node);
                    }
                }
            }

            nodes.Clear();
            nodes.AddRange(newNodes);
            gridSize = newGridSize;
        }

        public PathNode GetNodeInBounds(Vector3 worldPosition)
        {
            if (Nodes.Count == 0)
                return null;

            var node = GetNodeForPosition(worldPosition);

            if (
                node != null
                && (worldPosition - GetPositionForNode(node)).sqrMagnitude
                    < 0.99 * (NodeSize * NodeSize)
            )
                return node;
            else
                return null;
        }

        [ContextMenu("Fix Missing Objects")]
        public void FixMissingObjects()
        {
            var results = new Collider[GridSize.x * GridSize.y * GridSize.z];
            var nodeObjects = new List<PathNodeObject>();

            foreach (var node in Nodes)
            {
                var position = GetPositionForNode(node);

                int hits = Physics.OverlapBoxNonAlloc(
                    position,
                    (0.1f * nodeSize) * Vector3.one,
                    results,
                    Quaternion.identity,
                    -1,
                    QueryTriggerInteraction.Ignore
                );

                if (hits == 0)
                {
                    node.NodeObject = null;

                    continue;
                }

                nodeObjects.Clear();

                for (int i = 0; i < hits; i++)
                {
                    var hit = results[i];

                    var nodeObject = hit.GetComponentInParent<PathNodeObject>();

                    if (nodeObject != null)
                        nodeObjects.Add(nodeObject);
                }

                if (nodeObjects.Count == 0)
                {
                    node.NodeObject = null;

                    continue;
                }

                var closestObj = nodeObjects
                    .OrderBy((obj) => (obj.transform.position - position).sqrMagnitude)
                    .FirstOrDefault();

                node.NodeObject = closestObj;
                closestObj.Grid = this;
                closestObj.Node = node;
            }
        }
    }
}
