using UnityEngine;

namespace Shears.Grids
{
    [System.Serializable]
    public class DefaultWeightBehaviour : IPathWeightBehaviour
    {
        private const int ENTITY_COST = 5;
        private const int AIR_COST = 100;

        public int GetWeight(IPathWeightBehaviour.ExecuteData data)
        {
            return GetWeightStatic(data);
        }

        public static int GetWeightStatic(IPathWeightBehaviour.ExecuteData data)
        {
            if (data.TargetPosition.TryGetData(out DoorwayNodeData _))
                return 0;

            int weight;
            int entityCount = data.TargetPosition.EntityCount;

            if (data.TargetPosition.ContainsEntity(data.Entity))
                entityCount--;

            entityCount = Mathf.Min(entityCount, 100);

            weight = ENTITY_COST * entityCount;

            if (data.TargetPosition is AirEntityPosition)
                weight += AIR_COST;

            return weight;
        }
    }
}
