using UnityEngine;

namespace Shears.Grids
{
    [System.Serializable]
    public class DefaultWeightBehaviour : IPathWeightBehaviour
    {
        private const int ENTITY_COST = 5;

        public int GetWeight(IPathWeightBehaviour.ExecuteData data)
        {
            throw new System.NotImplementedException();
        }

        public static int GetWeightStatic(IPathWeightBehaviour.ExecuteData data)
        {
            if (data.TargetPosition.TryGetData(out DoorwayNodeData _))
                return 0;

            int count = data.TargetPosition.EntityCount;

            if (data.TargetPosition.ContainsEntity(data.Entity))
                count--;

            count = Mathf.Min(count, 100);

            return ENTITY_COST * count;
        }
    }
}
