using System;
using System.Collections.Generic;
using Shears.Logging;
using UnityEngine;

namespace Shears.Pathfinding
{
    public interface IPathGrid
    {
        public IReadOnlyList<PathNode> Nodes { get; }
        public Vector3Int GridSize { get; }
        public float NodeSize { get; }
        public Transform Transform { get; }

        public event Action GridChanged;

        public void UpdateWorldPositions();

        public Vector3 GetPositionForNode(PathNode node);

        public PathNode GetNodeForPosition(Vector3 worldPosition);

        public PathNode GetNodeInBounds(Vector3 worldPosition);

        public void GetNodesInBounds(Bounds bounds, List<PathNode> boundsNodes);

        public void GetNeighbors(PathNode node, List<PathNode> neighbors);

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

        public Vector3 GetCenter();
    }
}
