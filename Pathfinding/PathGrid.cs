using System.Collections.Generic;
using UnityEngine;

namespace Shears.Pathfinding
{
    public class PathGrid : MonoBehaviour
    {
        [SerializeField] private Vector3Int gridSize;
        [SerializeField] private float nodeSize;
        [SerializeField, ReadOnly] private List<PathNode> nodes = new();

        public Vector3Int GridSize => gridSize;
        public float NodeSize => nodeSize;
        public IReadOnlyList<PathNode> Nodes => nodes;

        private void OnValidate()
        {
            gridSize = gridSize.ClampMax(1);
        }
    }
}
