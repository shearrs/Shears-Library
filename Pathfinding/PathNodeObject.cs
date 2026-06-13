using System;
using UnityEngine;

namespace Shears.Pathfinding
{
    public class PathNodeObject : MonoBehaviour
    {
        [HideInInspector, SerializeField]
        private InterfaceReference<IPathGrid> grid;

        [HideInInspector, SerializeReference]
        private PathNode node;

        public IPathGrid Grid
        {
            get => grid.Value;
            internal set
            {
                if (grid.Value != null)
                    grid.Value.GridChanged -= OnGridChanged;

                grid.Value = value;

                value.GridChanged += OnGridChanged;
            }
        }
        public PathNode Node
        {
            get => node;
            internal set => node = value;
        }

        public event Action GridChanged;

        private void OnGridChanged()
        {
            GridChanged?.Invoke();
        }
    }
}
