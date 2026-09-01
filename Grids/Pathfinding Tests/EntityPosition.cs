using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Grids
{
    public abstract class EntityPosition
    {
        private readonly HashSet<IPathEntity> entities = new();
        private readonly GridNode node;

        public Vector3Int GridPosition { get; }
        public Vector3 WorldPosition { get; }
        public int EntityCount => entities.Count;

        public EntityPosition(GridNode node, Vector3Int gridPosition, Vector3 worldPosition)
        {
            this.node = node;
            GridPosition = gridPosition;
            WorldPosition = worldPosition;
        }

        public bool TryGetData<T>(out T data)
            where T : GridNodeData => node.TryGetData(out data);

        public bool ContainsEntity(IPathEntity entity) => entities.Contains(entity);

        public void RegisterEntity(IPathEntity entity)
        {
            if (!entities.Contains(entity))
                entities.Add(entity);
        }

        public void UnregisterEntity(IPathEntity entity) => entities.Remove(entity);
    }
}
