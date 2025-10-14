using Shears.Logging;
using UnityEngine;

namespace Shears.Pathfinding
{
    [System.Serializable]
    public class PathNode
    {
        [SerializeField] private Vector3Int gridPosition;
        [SerializeField] private Vector3 localPosition;
        [SerializeReference] private PathNodeData data;

        public Vector3Int GridPosition => gridPosition;
        public Vector3 LocalPosition => localPosition;
        public PathNodeData Data { get => data; set => data = value; }

        public PathNode(Vector3Int gridPosition, Vector3 localPosition)
        {
            this.gridPosition = gridPosition;
            this.localPosition = localPosition;
        }

        public T GetData<T>() where T : PathNodeData
        {
            if (data is T tData)
                return tData;

            SHLogger.Log($"Could not find data of type {nameof(T)}!", SHLogLevels.Error);
            return null;
        }
    }
}
