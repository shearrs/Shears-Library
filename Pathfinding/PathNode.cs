using Shears.Logging;
using UnityEngine;

namespace Shears.Pathfinding
{
    [System.Serializable]
    public class PathNode
    {
        [SerializeField] private Vector3Int gridPosition;
        [SerializeField] private Vector3 worldPosition;
        [SerializeReference] private PathNodeData data;

        private PathNode parent;
        private int gCost;
        private int hCost;

        public Vector3Int GridPosition => gridPosition;
        public Vector3 WorldPosition { get => worldPosition; internal set => worldPosition = value; }
        public PathNodeData Data { get => data; set => data = value; }
        public PathNode Parent { get => parent; set => parent = value; }
        public int GCost { get => gCost; set => gCost = value; }
        public int HCost { get => hCost; set => hCost = value; }
        public int FCost => gCost + hCost;

        public PathNode(Vector3Int gridPosition, Vector3 worldPosition)
        {
            this.gridPosition = gridPosition;
            this.worldPosition = worldPosition;
        }

        public bool TryGetData<T>(out T nodeData) where T : PathNodeData
        {
            if (data is T tData)
            {
                nodeData = tData;
                return true;
            }

            nodeData = null;
            return false;
        }
    }
}
