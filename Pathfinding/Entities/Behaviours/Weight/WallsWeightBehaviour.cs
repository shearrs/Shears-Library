using UnityEngine;

namespace Shears.Pathfinding
{
    [System.Serializable]
    public class WallsWeightBehaviour : IPathWeightBehaviour
    {
        public int GetWeight(IPathWeightBehaviour.ExecuteData data)
        {
            var currentPosition = data.CurrentPosition;
            var targetPosition = data.TargetPosition;

            if (
                currentPosition.TryGetData(out DoorwayNodeData door)
                && targetPosition.GridPosition == door.ConnectedGridPosition
            )
                return 1000000;
            else
            {
                int bonus = 0;

                if (
                    currentPosition is SurfaceEntityPosition currentSurface
                    && targetPosition is SurfaceEntityPosition targetSurface
                    && currentSurface.SurfaceDirection != targetSurface.SurfaceDirection
                )
                    bonus = 20;

                return DefaultWeightBehaviour.GetWeightStatic(data) + bonus;
            }
        }
    }
}
