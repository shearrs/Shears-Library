using UnityEngine;

namespace Shears.Grids
{
    public class NodeObject : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        private ShearsGrid grid;

        [SerializeField, ReadOnly]
        private Vector3Int gridPosition;

        internal Vector3Int GridPosition
        {
            get => gridPosition;
            set => gridPosition = value;
        }
        public ShearsGrid Grid
        {
            get => grid;
            internal set => grid = value;
        }
        public GridNode Node => grid.GetNode(gridPosition);
    }
}
