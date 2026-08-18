using UnityEngine;

namespace Shears.Pathfinding
{
    public readonly ref struct PathNodeHandleContext
    {
        public Vector3 NodePosition { get; }
        public float NodeSize { get; }

        public PathNodeHandleContext(Vector3 nodePosition, float nodeSize)
        {
            NodePosition = nodePosition;
            NodeSize = nodeSize;
        }
    }
}
