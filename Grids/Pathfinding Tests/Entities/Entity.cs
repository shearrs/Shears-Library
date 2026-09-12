using UnityEngine;

namespace Shears.Grids
{
    public class Entity : MonoBehaviour, IPathEntity
    {
        [SerializeField]
        private PathGrid testGrid;

        [SerializeField]
        private Pathfinder pathfinder;

        [SerializeField]
        private bool canWalkOnWalls;

        public Vector3 Position => transform.position;
        public EntityPosition EntityPosition { get; private set; }
        public bool CanWalkOnWalls => canWalkOnWalls;

        private void Update()
        {
            if (testGrid.TryGetNodeData<TargetNodeData>(out var info))
            {
                var worldPosition = testGrid.GridToWorld(info.GridPosition);
                pathfinder.CalculatePath(transform.position, worldPosition);
            }
        }
    }
}
