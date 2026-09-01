using System.Collections.Generic;
using Shears.Logging;
using UnityEngine;

namespace Shears.Grids
{
    public class ShearsGrid : ShearsBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField]
        private Vector3Int size = Vector3Int.one;

        [SerializeField, Min(0.01f)]
        private float nodeSize = 1.0f;

        [SerializeField]
        private List<GridNode> nodes = new();

        public Vector3Int Size
        {
            get => size;
            set => size = value;
        }
        public float NodeSize
        {
            get => nodeSize;
            set => nodeSize = value;
        }
        public Vector3 MinExtent => transform.position;
        public Vector3 MaxExtent => nodes.Count > 0 ? GetWorldPosition(nodes[^1]) : MinExtent;
        public Vector3Int MinGridExtent => Vector3Int.zero;
        public Vector3Int MaxGridExtent =>
            nodes.Count > 0 ? nodes[^1].GridPosition : Vector3Int.zero;
        public IReadOnlyList<GridNode> Nodes => nodes;

        public bool TryGetNode(Vector3Int gridPosition, out GridNode node)
        {
            node = null;

            if (!WithinBounds(gridPosition))
                return false;

            node = GetNode(gridPosition);

            return true;
        }

        public GridNode GetNode(Vector3Int gridPosition)
        {
            int index = GetNodeIndex(gridPosition);

            return nodes[index];
        }

        public bool TryGetNodeForWorldPosition(Vector3 worldPosition, out GridNode node)
        {
            var localPosition = transform.InverseTransformPoint(worldPosition);
            var gridPosition = localPosition.RoundToInt();

            return TryGetNode(gridPosition, out node);
        }

        public GridNode GetNodeForWorldPosition(Vector3 worldPosition)
        {
            var localPosition = transform.InverseTransformPoint(worldPosition);
            var gridPosition = localPosition.RoundToInt();

            int index = GetNodeIndex(gridPosition);

            return nodes[index];
        }

        public Vector3 GetWorldPosition(GridNode node)
        {
            return transform.TransformPoint(nodeSize * (Vector3)node.GridPosition);
        }

        public Vector3 GetLocalPosition(GridNode node)
        {
            return nodeSize * (Vector3)node.GridPosition;
        }

        public Vector3 GetCenter()
        {
            return transform.TransformPoint(0.5f * (Vector3)MaxGridExtent);
        }

        public int GetNodeIndex(Vector3Int gridPosition)
        {
            return (gridPosition.z * size.y * size.x) + (gridPosition.y * size.x) + gridPosition.x;
        }

        public void InsertGrid(ShearsGrid grid, List<GridNode> clonedNodes = null)
        {
            clonedNodes?.Clear();

            if (grid.Nodes.Count == 0)
            {
                LogWarning("Grid has no nodes to add!");
                return;
            }

            CollectionUtil.GetPooled(out List<GridNode> newNodes);

            var min = VectorUtil.Min(MinExtent, grid.MinExtent);
            var max = VectorUtil.Max(MaxExtent, grid.MaxExtent);

            var newGridSize = (max - min).RoundToInt() + Vector3Int.one;
            var previousPosition = transform.position;

            transform.position = min;

            var offset = transform.position - previousPosition;

            for (int z = 0; z < newGridSize.z; z++)
            {
                for (int y = 0; y < newGridSize.y; y++)
                {
                    for (int x = 0; x < newGridSize.x; x++)
                    {
                        var gridPosition = new Vector3Int(x, y, z);
                        var worldPosition = new Vector3(
                            min.x + x * nodeSize,
                            min.y + y * nodeSize,
                            min.z + z * nodeSize
                        );
                        bool currentGridNode = false;

                        grid.TryGetNodeForWorldPosition(worldPosition, out var node);

                        if (size.sqrMagnitude > 0 && (node == null || node.DataCount == 0))
                            currentGridNode = TryGetNodeForWorldPosition(
                                worldPosition + offset,
                                out node
                            );

                        if (node == null)
                            node = new(gridPosition);
                        else
                        {
                            if (!currentGridNode)
                            {
                                node = node.Clone();
                                clonedNodes?.Add(node);
                            }

                            node.GridPosition = gridPosition;
                        }

                        if (node.NodeObject != null)
                        {
                            node.NodeObject.Grid = this;
                            node.NodeObject.GridPosition = gridPosition;
                        }

                        newNodes.Add(node);
                    }
                }
            }

            size = newGridSize;
            nodes.Clear();
            nodes.AddRange(newNodes);
            CollectionUtil.ReleasePooled(newNodes);
        }

        public void SetNode(Vector3 worldPosition, GridNode node) =>
            SetNode(WorldToGrid(worldPosition), node);

        public void SetNode(Vector3Int gridPosition, GridNode node)
        {
            if (node == null)
            {
                this.LogError($"Tried to set null node!");
                return;
            }

            if (WithinBounds(gridPosition))
                SetNodeInternal(gridPosition, node);
            else
            {
                var offset = ExpandToFit(gridPosition, gridPosition);
                SetNodeInternal(gridPosition - offset, node);
            }
        }

        public Vector3Int CropToContent()
        {
            var newMin = size;
            var newMax = Vector3Int.zero;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var gridPos = node.GridPosition;

                if (node.DataCount > 0)
                {
                    if (gridPos.x < newMin.x)
                        newMin.x = gridPos.x;
                    if (gridPos.y < newMin.y)
                        newMin.y = gridPos.y;
                    if (gridPos.z < newMin.z)
                        newMin.z = gridPos.z;
                }
            }

            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var node = nodes[i];
                var gridPos = node.GridPosition;

                if (node.DataCount > 0)
                {
                    if (gridPos.x > newMax.x)
                        newMax.x = gridPos.x;
                    if (gridPos.y > newMax.y)
                        newMax.y = gridPos.y;
                    if (gridPos.z > newMax.z)
                        newMax.z = gridPos.z;
                }
            }

            return Resize(newMin, newMax);
        }

        public void Shift(
            Vector3Int rangeStart,
            Vector3Int rangeEnd,
            Direction direction,
            int distance,
            HashSet<Vector3Int> ignorePositions = null
        )
        {
            if (nodes.Count == 0)
            {
                this.LogWarning($"Can not shift grid with no nodes.");
                return;
            }

            var directionOffset = distance * direction.ToVectorInt();
            var shiftedStart = rangeStart + directionOffset;
            var shiftedEnd = rangeEnd + directionOffset;
            var offset = ExpandToFit(shiftedStart, shiftedEnd);

            var newStart = rangeStart - offset;
            var newEnd = rangeEnd - offset;
            var newShiftedStart = newStart + directionOffset;
            var newShiftedEnd = newEnd + directionOffset;

            var min = VectorUtil.Min(newStart, newShiftedStart);
            var max = VectorUtil.Max(newEnd, newShiftedEnd);

            CollectionUtil.GetPooled(out List<GridNode> newNodes);

            for (int z = min.z; z <= max.z; z++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int x = min.x; x <= max.x; x++)
                    {
                        GridNode node;
                        var gridPosition = new Vector3Int(x, y, z);

                        if (VectorUtil.WithinRange(gridPosition, newShiftedStart, newShiftedEnd))
                        {
                            var originOffset = gridPosition - newShiftedStart;
                            var originalPosition = newStart + originOffset;

                            var targetOverrideNode = GetNode(gridPosition);

                            if (
                                ignorePositions != null
                                && ignorePositions.Contains(originalPosition)
                            )
                                node = targetOverrideNode;
                            else
                            {
                                node = GetNode(originalPosition);

                                if (
                                    !VectorUtil.WithinRange(gridPosition, newStart, newEnd)
                                    && targetOverrideNode.NodeObject != null
                                )
                                    targetOverrideNode.DestroyNodeObject();
                            }
                        }
                        else
                        {
                            if (ignorePositions != null && ignorePositions.Contains(gridPosition))
                                node = GetNode(gridPosition);
                            else
                                node = new(gridPosition);
                        }

                        node.GridPosition = gridPosition;

                        if (node.NodeObject != null)
                            node.NodeObject.transform.localPosition = GetLocalPosition(node);

                        newNodes.Add(node);
                    }
                }
            }

            foreach (var node in newNodes)
            {
                var index = GetNodeIndex(node.GridPosition);
                nodes[index] = node;
            }

            CollectionUtil.ReleasePooled(newNodes);
        }

        public bool WithinBounds(Vector3 worldPosition) => WithinBounds(WorldToGrid(worldPosition));

        public bool WithinBounds(Vector3Int gridPosition)
        {
            var x = gridPosition.x;
            var y = gridPosition.y;
            var z = gridPosition.z;

            return x < size.x && y < size.y && z < size.z && x >= 0 && y >= 0 && z >= 0;
        }

        private Vector3Int ExpandToFit(Vector3Int minBounds, Vector3Int maxBounds)
        {
            var currentMin = Vector3Int.zero;
            var currentMax = MaxGridExtent;

            var min = VectorUtil.Min(minBounds, currentMin);
            var max = VectorUtil.Max(maxBounds, currentMax);

            return Resize(min, max);
        }

        private Vector3Int Resize(Vector3Int min, Vector3Int max)
        {
            if (min == Vector3Int.zero && nodes.Count > 0 && max == nodes[^1].GridPosition)
                return Vector3Int.zero;
            else if (min == Vector3Int.zero && max == Vector3Int.zero)
                return Vector3Int.zero;

            CollectionUtil.GetPooled(out List<GridNode> newNodes);
            var newGridSize = max - min + Vector3Int.one;
            var offset = min;

            transform.localPosition += offset;

            for (int z = 0; z < newGridSize.z; z++)
            {
                for (int y = 0; y < newGridSize.y; y++)
                {
                    for (int x = 0; x < newGridSize.x; x++)
                    {
                        var nodeGridPosition = new Vector3Int(x, y, z);

                        if (TryGetNode(nodeGridPosition + offset, out var node))
                        {
                            node.GridPosition = nodeGridPosition;

                            if (node.NodeObject != null)
                                node.NodeObject.transform.localPosition = GetLocalPosition(node);
                        }
                        else
                            node = new GridNode(nodeGridPosition);

                        newNodes.Add(node);
                    }
                }
            }

            size = newGridSize;
            nodes.Clear();
            nodes.AddRange(newNodes);
            CollectionUtil.ReleasePooled(newNodes);

            return offset;
        }

        private void SetNodeInternal(Vector3Int gridPosition, GridNode node)
        {
            int index = GetNodeIndex(gridPosition);

            node.GridPosition = gridPosition;
            nodes[index] = node;
        }

        private Vector3Int WorldToGrid(Vector3 worldPosition)
        {
            var localPosition = worldPosition - transform.position;

            return localPosition.RoundToInt();
        }
    }
}
