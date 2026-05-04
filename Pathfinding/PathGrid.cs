using System;
using System.Collections.Generic;
using Shears.Logging;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Shears.Pathfinding
{
    public class PathGrid : MonoBehaviour
    {
        public enum Direction
        {
            Left,
            Right,
            Up,
            Down,
            Forward,
            Backward,
        }

        [SerializeField, Delayed]
        private Vector3Int gridSize = Vector3Int.one;

        [SerializeField, Delayed]
        private float nodeSize = 1.0f;

        [SerializeField]
        private List<PathNode> nodes = new();

        private readonly List<PathNode> boundsNodes = new();

        public Vector3Int GridSize => gridSize;
        public float NodeSize => nodeSize;
        public IReadOnlyList<PathNode> Nodes => nodes;

        public event Action GridChanged;

        private void OnValidate()
        {
            gridSize = gridSize.ClampMax(1);
        }

        private void Awake()
        {
            UpdateWorldPositions();
        }

#if UNITY_EDITOR
        [ContextMenu("Update Node Objects")]
        private void UpdateNodeObjects()
        {
            foreach (var node in nodes)
            {
                if (node.NodeObject == null)
                    continue;

                node.NodeObject.Grid = this;
                node.NodeObject.Node = node;
            }
        }

        [ContextMenu("Fix Serialized References")]
        private void FixSerializedReferences()
        {
            SerializationUtility.ClearAllManagedReferencesWithMissingTypes(this);
            EditorUtility.SetDirty(this);
        }
#endif

        public void UpdateWorldPositions()
        {
            if (nodes.Count == 0)
                return;

            if (nodes[0].WorldPosition == transform.position)
                return;

            foreach (var node in nodes)
            {
                Vector3 localPosition = new(
                    nodeSize * node.GridPosition.x,
                    nodeSize * node.GridPosition.y,
                    nodeSize * node.GridPosition.z
                );

                node.Size = nodeSize;
                node.WorldPosition = transform.TransformPoint(localPosition);
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
            Vector3 gridWorldSize = nodeSize * ((Vector3)gridSize - Vector3.one);
            Vector3 center = GetCenter();
            worldPosition -= center;

            float xPercent = (worldPosition.x + (0.5f * gridWorldSize.x)) / gridWorldSize.x;
            float yPercent = (worldPosition.y + (0.5f * gridWorldSize.y)) / gridWorldSize.y;
            float zPercent = (worldPosition.z + (0.5f * gridWorldSize.z)) / gridWorldSize.z;

            xPercent = Mathf.Clamp01(xPercent);
            yPercent = Mathf.Clamp01(yPercent);
            zPercent = Mathf.Clamp01(zPercent);

            int x = Mathf.RoundToInt((gridSize.x - 1) * xPercent);
            int y = Mathf.RoundToInt((gridSize.y - 1) * yPercent);
            int z = Mathf.RoundToInt((gridSize.z - 1) * zPercent);

            return GetNode(x, y, z);
        }

        public IReadOnlyList<PathNode> GetNodesInBounds(Bounds bounds)
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
                else if (x >= gridSize.x)
                    break;

                for (int y = localMin.y; y <= localMax.y; y++)
                {
                    if (y < 0)
                        continue;
                    else if (y >= gridSize.y)
                        break;

                    for (int z = localMin.z; z <= localMax.z; z++)
                    {
                        if (z < 0)
                            continue;
                        else if (z >= gridSize.z)
                            break;

                        boundsNodes.Add(GetNode(x, y, z));
                    }
                }
            }

            return boundsNodes;
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
                            (gridX < 0 || gridX > gridSize.x - 1)
                            || (gridY < 0 || gridY > gridSize.y - 1)
                            || (gridZ < 0 || gridZ > gridSize.z - 1)
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
            if (x >= gridSize.x || y >= gridSize.y || z >= gridSize.z || x < 0 || y < 0 || z < 0)
            {
                SHLogger.Log($"Invalid coordinates for node: ({x}, {y}, {z})", SHLogLevels.Error);
                return null;
            }

            int index = (z * gridSize.y * gridSize.x) + (y * gridSize.x) + x;

            return nodes[index];
        }

        public PathNode GetNodeWithData<T>()
            where T : PathNodeData
        {
            foreach (var node in nodes)
            {
                if (node.TryGetData(out T _))
                    return node;
            }

            return null;
        }

        public Vector3 GetCenter()
        {
            return transform.position + (0.5f * nodeSize * ((Vector3)gridSize - Vector3.one));
        }

        public static PathGrid Combine(params PathGrid[] grids)
        {
            var newGrid = new GameObject("Combined PathGrid").AddComponent<PathGrid>();

            if (grids.Length == 0)
                return newGrid;

            newGrid.Copy(grids[0], true);

            if (grids.Length == 1)
                return newGrid;

            for (int i = 1; i < grids.Length; i++)
                newGrid.Add(grids[i]);

            return newGrid;
        }

        public void Copy(PathGrid other, bool reuseObjects = true)
        {
            nodes.Clear();
            gridSize = other.gridSize;
            nodeSize = other.nodeSize;
            transform.position = other.transform.position;

            foreach (var node in other.nodes)
                nodes.Add(node.Clone(this, reuseObjects));
        }

        public void Add(PathGrid other)
        {
            var minPosition = transform.position;
            var maxPosition = GetPositionForNode(nodes[^1]);
            var otherMinPosition = other.transform.position;
            var otherMaxPosition = other.GetPositionForNode(other.nodes[^1]);

            MathUtil.MinMax(
                out var xMin,
                out var xMax,
                minPosition.x,
                otherMinPosition.x,
                maxPosition.x,
                otherMaxPosition.x
            );
            MathUtil.MinMax(
                out var yMin,
                out var yMax,
                minPosition.y,
                otherMinPosition.y,
                maxPosition.y,
                otherMaxPosition.y
            );
            MathUtil.MinMax(
                out var zMin,
                out var zMax,
                minPosition.z,
                otherMinPosition.z,
                maxPosition.z,
                otherMaxPosition.z
            );
            var newGridSize =
                new Vector3(xMax - xMin, yMax - yMin, zMax - zMin).RoundToInt() + Vector3Int.one;
            var newNodes = new List<PathNode>();

            transform.position = new Vector3(xMin, yMin, zMin);

            for (int z = 0; z < newGridSize.z; z++)
            {
                for (int y = 0; y < newGridSize.y; y++)
                {
                    for (int x = 0; x < newGridSize.x; x++)
                    {
                        var worldPosition = new Vector3(xMin + x, yMin + y, zMin + z);
                        var firstNode = GetNodeForPosition(worldPosition);
                        var secondNode = other.GetNodeForPosition(worldPosition);
                        var firstDist = (
                            worldPosition - GetPositionForNode(firstNode)
                        ).sqrMagnitude;
                        var secondDist = (
                            worldPosition - other.GetPositionForNode(secondNode)
                        ).sqrMagnitude;

                        PathNode node = null;
                        PathNode newNode;
                        bool firstValid = firstDist < NodeSize * NodeSize;
                        bool secondValid = secondDist < NodeSize * NodeSize;

                        if (firstValid)
                        {
                            if (secondValid)
                            {
                                if (firstNode.Data != null)
                                    node = firstNode;
                                else
                                    node = secondNode;
                            }
                            else
                                node = firstNode;
                        }
                        else if (secondValid)
                            node = secondNode;

                        if (node == null)
                        {
                            newNode = new(new Vector3Int(x, y, z), worldPosition)
                            {
                                Size = nodeSize,
                            };
                        }
                        else
                        {
                            newNode = node.Clone(this);
                            newNode.GridPosition = new Vector3Int(x, y, z);
                            newNode.WorldPosition = worldPosition;
                        }

                        newNodes.Add(newNode);
                    }
                }
            }

            if (Application.isPlaying)
                Destroy(other.gameObject);

            gridSize = newGridSize;
            nodes = newNodes;
        }
    }
}
