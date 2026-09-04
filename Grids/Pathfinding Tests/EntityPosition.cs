using System;
using System.Collections.Generic;
using UnityEngine;
using static Shears.Grids.GridNode;

namespace Shears.Grids
{
    public abstract class EntityPosition
    {
        private readonly HashSet<IPathEntity> entities = new();
        private readonly HashSet<GridNode> registeredNodes = new();
        private readonly GridNode node;

        public Vector3Int GridPosition { get; }
        public Vector3 WorldPosition { get; }
        public int EntityCount => entities.Count;

        public event Action<EntityPositionUpdateData> Updated;

        public readonly struct EntityPositionUpdateData
        {
            public EntityPosition Position { get; }
            public GridNodeData Data { get; }

            public EntityPositionUpdateData(EntityPosition position, GridNodeData data)
            {
                Position = position;
                Data = data;
            }
        }

        public EntityPosition(GridNode node, Vector3Int gridPosition, Vector3 worldPosition)
        {
            this.node = node;
            GridPosition = gridPosition;
            WorldPosition = worldPosition;

            RegisterUpdateNode(node);
        }

        internal void Dispose()
        {
            foreach (var node in registeredNodes)
                node.Updated -= OnNodeUpdated;
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

        protected void RegisterUpdateNode(GridNode node)
        {
            if (registeredNodes.Contains(node))
                return;

            node.Updated += OnNodeUpdated;
            registeredNodes.Add(node);
        }

        private void OnNodeUpdated(NodeUpdateData data)
        {
            Updated?.Invoke(new EntityPositionUpdateData(this, data.Data));
        }
    }
}
