using UnityEngine;

namespace Shears.Pathfinding
{
    public class PathNodeObject : MonoBehaviour
    {
        [SerializeField] private PathGrid grid;
        [SerializeReference] private PathNode node;

        public PathGrid Grid { get => grid; internal set => grid = value; }
        public PathNode Node { get => node; internal set => node = value; }
    }
}
