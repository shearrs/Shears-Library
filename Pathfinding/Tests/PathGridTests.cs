using System.Collections.Generic;
using UnityEngine;

namespace Shears.Pathfinding
{
    public class PathGridTests : MonoBehaviour
    {
        [SerializeField]
        private List<PathGrid> grids;

        [SerializeField, ReadOnly]
        private PathGrid combination;

        [ContextMenu("Combine Grids")]
        private void Combine()
        {
            combination = PathGrid.Combine(grids.ToArray());
        }
    }
}
