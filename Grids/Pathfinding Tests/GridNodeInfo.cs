using UnityEngine;

namespace Shears.Grids
{
    public readonly struct GridNodeInfo<T>
        where T : GridNodeData
    {
        public T Data { get; }
        public Vector3Int GridPosition { get; }

        public GridNodeInfo(T data, Vector3Int gridPosition)
        {
            Data = data;
            GridPosition = gridPosition;
        }
    }
}
