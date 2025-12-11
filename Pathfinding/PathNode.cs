using System;
using UnityEngine;

namespace Shears.Pathfinding
{
    [Serializable]
    public class PathNode : IHeapItem<PathNode>
    {
        [SerializeField] private Vector3Int gridPosition;
        [SerializeField] private Vector3 worldPosition;
        [SerializeReference] private PathNodeData data;
        [SerializeField] private PathNodeObject nodeObject;

        private PathNode parent;
        private int gCost; // distance from starting node
        private int hCost; // distance from end node

        public PathNodeObject NodeObject => nodeObject;
        public Vector3Int GridPosition => gridPosition;
        public Vector3 WorldPosition { get => worldPosition; internal set => worldPosition = value; }
        public PathNodeData Data { get => data; set => data = value; }
        public PathNode Parent { get => parent; set => parent = value; }
        public int GCost { get => gCost; set => gCost = value; }
        public int HCost { get => hCost; set => hCost = value; }
        public int FCost => gCost + hCost;

        int IHeapItem<PathNode>.HeapIndex { get; set; }

        public PathNode(Vector3Int gridPosition, Vector3 worldPosition)
        {
            this.gridPosition = gridPosition;
            this.worldPosition = worldPosition;
        }

        public bool TryGetData<T>(out T nodeData) where T : PathNodeData
        {
            nodeData = null;

            if (data == null)
                return false;

            if (data is T tData)
            {
                nodeData = tData;
                return true;
            }

            return false;
        }

        int IComparable<PathNode>.CompareTo(PathNode other)
        {
            int compare = FCost.CompareTo(other.FCost);

            if (compare == 0)
                compare = HCost.CompareTo(other.HCost);

            return -compare;
        }
    }
}
