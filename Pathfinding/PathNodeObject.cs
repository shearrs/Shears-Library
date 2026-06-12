using UnityEngine;

namespace Shears.Pathfinding
{
    public class PathNodeObject : MonoBehaviour
    {
        [SerializeField]
        private InterfaceReference<IPathGrid> grid;

        [SerializeReference]
        private PathNode node;

        public IPathGrid Grid
        {
            get => grid.Value;
            internal set => grid.Value = value;
        }
        public PathNode Node
        {
            get => node;
            internal set => node = value;
        }
    }
}
