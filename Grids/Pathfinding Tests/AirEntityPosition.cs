using UnityEngine;

namespace Shears.Grids
{
    public class AirEntityPosition : EntityPosition
    {
        public AirEntityPosition(GridNode node, Vector3Int gridPosition, Vector3 worldPosition)
            : base(node, gridPosition, worldPosition) { }
    }
}
