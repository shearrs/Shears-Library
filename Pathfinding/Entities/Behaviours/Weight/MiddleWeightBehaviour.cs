using UnityEngine;

namespace Shears.Pathfinding
{
    [System.Serializable]
    public class MiddleWeightBehaviour : IPathWeightBehaviour
    {
        private const int MIDDLE_COST = 0;
        private const int SIDE_COST = 100;
        private const int AIR_COST = 100;

        public int GetWeight(IPathWeightBehaviour.ExecuteData data)
        {
            if (data.TargetPosition.TryGetData(out DoorwayNodeData _))
                return 0;

            int weight;

            if (data.TargetPosition.GridPosition.z == 1)
                weight = MIDDLE_COST;
            else
                weight = SIDE_COST;

            if (data.TargetPosition is AirEntityPosition)
                weight += AIR_COST;

            return weight;
        }
    }
}
