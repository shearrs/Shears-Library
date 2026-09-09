using UnityEngine;

namespace Shears.Grids
{
    public class Entity : MonoBehaviour, IPathEntity
    {
        [SerializeField]
        private PathGrid testGrid;

        [SerializeField]
        private Pathfinder pathfinder;

        public Vector3 Position => transform.position;
        public EntityPosition EntityPosition { get; private set; }

        private void Start()
        {
            if (testGrid.TryGetNodeData<TargetNodeData>(out var info))
            {
                var worldPosition = testGrid.GridToWorld(info.GridPosition);
                pathfinder.CalculatePath(transform.position, worldPosition);
            }
        }
    }
}
