using System.Collections.Generic;
using UnityEngine;

namespace Shears.Pathfinding
{
    public class PathGridTests : MonoBehaviour
    {
        [SerializeField]
        private PathGrid baseGrid;

        [SerializeField]
        private List<PathGrid> gridsToAdd;

        [ContextMenu("Combine Grids")]
        private void Combine()
        {
            foreach (var grid in gridsToAdd)
                baseGrid.Add(grid);
        }
    }
}
