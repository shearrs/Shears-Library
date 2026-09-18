using UnityEngine;

namespace Shears.Pathfinding
{
    public interface IPathWeightBehaviour : IPathfindingBehaviour
    {
        public readonly ref struct ExecuteData
        {
            public EntityPosition CurrentPosition { get; }
            public EntityPosition TargetPosition { get; }
            public IPathEntity Entity { get; }

            public ExecuteData(
                EntityPosition currentPosition,
                EntityPosition targetPosition,
                IPathEntity entity
            )
            {
                CurrentPosition = currentPosition;
                TargetPosition = targetPosition;
                Entity = entity;
            }
        }

        public int GetWeight(ExecuteData data);
    }
}
