using System;
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
            internal set
            {
                if (grid != null)
                    grid.NodesChanged -= OnNodesChanged;

                grid = value;

                if (grid != null)
                    grid.NodesChanged += OnNodesChanged;
            }
        }
        public GridNode Node => grid.GetNode(gridPosition);

        public event Action GridChanged;

        private void OnDestroy()
        {
            if (grid != null)
                grid.NodesChanged -= OnNodesChanged;
        }

        private void OnNodesChanged()
        {
            GridChanged?.Invoke();
        }
    }
}
