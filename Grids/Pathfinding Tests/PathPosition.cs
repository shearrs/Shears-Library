using UnityEngine;

namespace Shears.Grids
{
    public readonly struct PathPosition
    {
        internal EntityPosition EntityPosition { get; }
        public Vector3 Position { get; }
        public Vector3 Normal { get; }
        public Vector3 GridPosition => EntityPosition.GridPosition;

        public PathPosition(EntityPosition entityPosition, Vector3 position, Vector3 normal)
        {
            this.EntityPosition = entityPosition;
            Position = position;
            Normal = normal;
        }

        public bool TryGetData<T>(out T data)
            where T : GridNodeData => EntityPosition.TryGetData(out data);
    }
}
