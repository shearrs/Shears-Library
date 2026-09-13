using System.Collections.Generic;
using UnityEngine;

namespace Shears.Grids
{
    public interface IPathNeighborBehaviour : IPathfindingBehaviour
    {
        public readonly ref struct ExecuteData
        {
            public PathGrid Grid { get; }
            public EntityPosition CurrentPosition { get; }
            public List<EntityPosition> Neighbors { get; }

            public ExecuteData(
                PathGrid grid,
                EntityPosition currentPosition,
                List<EntityPosition> neighbors
            )
            {
                Grid = grid;
                CurrentPosition = currentPosition;
                Neighbors = neighbors;
            }
        }

        public void GetNeighbors(ExecuteData data);
    }
}
