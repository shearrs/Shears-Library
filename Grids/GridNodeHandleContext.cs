using UnityEngine;

namespace Shears.Grids
{
    public readonly ref struct GridNodeHandleContext
    {
        public ShearsGrid Grid { get; }
        public Vector3Int NodePosition { get; }
        public float NodeSize { get; }

        public GridNodeHandleContext(ShearsGrid grid, Vector3Int nodePosition, float nodeSize)
        {
            Grid = grid;
            NodePosition = nodePosition;
            NodeSize = nodeSize;
        }
    }
}
