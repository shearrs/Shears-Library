using UnityEngine;

namespace Shears.Pathfinding
{
    public class PathNodeObject : MonoBehaviour
    {
        private PathGrid grid;
        private PathNode node;

        public PathGrid Grid { get => grid; set => grid = value; }
        public PathNode Node { get => node; set => node = value; }
    }
}
