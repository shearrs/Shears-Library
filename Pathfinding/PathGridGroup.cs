using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Shears.Logging;
using UnityEngine;

namespace Shears.Pathfinding
{
    public class PathGridGroup : MonoBehaviour, IPathGrid
    {
        [SerializeField]
        private List<PathGrid> subGrids = new();

        [SerializeField]
        private List<PathNode> nodes = new();

        [SerializeField, Delayed]
        private float nodeSize = 1.0f;

        [SerializeField, ReadOnly]
        private Vector3Int gridSize;

        public IReadOnlyList<PathNode> Nodes => nodes;
        public Vector3Int GridSize => gridSize;
        public float NodeSize => nodeSize;

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

        public void Add(PathGrid grid)
        {
            if (subGrids.Contains(grid))
            {
                SHLogger.Log(
                    $"Grid {grid.name} is already a sub-grid of {name}.",
                    SHLogLevels.Warning
                );
                return;
            }

            subGrids.Add(grid);
            grid.Parent = this;

            RecalculateNodes();
        }

        [ContextMenu("Recalculate Nodes")]
        private void RecalculateNodes()
        {
            nodes.Clear();

            VectorUtil.MinMax(
                out var xMin,
                out var xMax,
                out var yMin,
                out var yMax,
                out var zMin,
                out var zMax,
                subGrids,
                s => s.transform.position,
                s => s.GetPositionForNode(s.Nodes[^1])
            );

            var newGridSize =
                new Vector3(xMax - xMin, yMax - yMin, zMax - zMin).RoundToInt() + Vector3Int.one;
            var previousPosition = transform.position;

            transform.position = new Vector3(xMin, yMin, zMin);

            for (int z = 0; z < newGridSize.z; z++)
            {
                for (int y = 0; y < newGridSize.y; y++)
                {
                    for (int x = 0; x < newGridSize.x; x++)
                    {
                        var worldPosition = new Vector3(xMin + x, yMin + y, zMin + z);
                        var validNode = GetFirstValidNode(worldPosition);

                        if (validNode == null)
                        {
                            validNode = new PathNode
                            {
                                GridPosition = new Vector3Int(x, y, z),
                                WorldPosition = worldPosition,
                                Size = nodeSize,
                            };
                        }
                        else
                        {
                            validNode.GridPosition = new Vector3Int(x, y, z);
                            validNode.WorldPosition = worldPosition;
                            validNode.Size = nodeSize;
                        }

                        nodes.Add(validNode);
                    }
                }
            }

            gridSize = newGridSize;
        }

        private PathNode GetFirstValidNode(Vector3 worldPosition)
        {
            bool isValid(PathGrid grid, PathNode node) =>
                (worldPosition - grid.GetPositionForNode(node)).sqrMagnitude < NodeSize * NodeSize;

            foreach (var subGrid in subGrids)
            {
                var node = subGrid.GetNodeForPosition(worldPosition);

                if (node != null && isValid(subGrid, node))
                    return node;
            }

            return null;
        }

        [ContextMenu("Fix Missing Objects")]
        public void FixMissingObjects()
        {
            foreach (var subGrid in subGrids)
                subGrid.FixMissingObjects();
        }
    }
}
