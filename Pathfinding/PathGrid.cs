using System;
using System.Collections.Generic;
using System.Linq;
using Shears.Logging;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Shears.Pathfinding
{
    public class PathGrid : MonoBehaviour, IPathGrid
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

        [SerializeReference]
        private List<PathNode> nodes = new();

        [SerializeField]
        private IPathGrid parent;

        public Vector3Int GridSize => gridSize;

        public float NodeSize => nodeSize;

        public IReadOnlyList<PathNode> Nodes => nodes;

        public IPathGrid Parent
        {
            get => parent;
            internal set => parent = value;
        }

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
            foreach (var node in Nodes)
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
