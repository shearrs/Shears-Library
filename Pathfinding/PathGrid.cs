using Shears.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Pathfinding
{
    public class PathGrid : MonoBehaviour
    {
        [SerializeField, Delayed]
        private Vector3Int gridSize = Vector3Int.one;

        [SerializeField, Delayed]
        private float nodeSize = 1.0f;

        [SerializeField]
        private List<PathNode> nodes = new();

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

                node.WorldPosition = transform.TransformPoint(localPosition);
            }

            GridChanged?.Invoke();
        }

        public PathNode GetNodeForPosition(Vector3 worldPosition)
        {
            Vector3 gridWorldSize = nodeSize * (Vector3)gridSize;
            Vector3 center = transform.position + (0.5f * nodeSize * ((Vector3)gridSize - Vector3.one));
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

                        if ((gridX < 0 || gridX > gridSize.x - 1) || (gridY < 0 || gridY > gridSize.y - 1) || (gridZ < 0 || gridZ > gridSize.z - 1))
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
            if (x > gridSize.x || y > gridSize.y || z > gridSize.z
                || x < 0 || y < 0 || z < 0)
            {
                SHLogger.Log($"Invalid coordinates for node: ({x}, {y}, {z})", SHLogLevels.Error);
                return null;
            }

            int index = (z * gridSize.y * gridSize.x) + (y * gridSize.x) + x;

            return nodes[index];
        }
    
        public PathNode GetNodeWithData<T>() where T : PathNodeData
        {
            foreach (var node in nodes)
            {
                if (node.TryGetData(out T _))
                    return node;
            }

            return null;
        }
    }
}
