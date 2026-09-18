using Shears.Grids;
using UnityEngine;

namespace Shears.Pathfinding
{
    public class AirEntityPosition : EntityPosition
    {
        public AirEntityPosition(GridNode node, Vector3Int gridPosition, Vector3 worldPosition)
            : base(node, gridPosition, worldPosition) { }
    }
}
