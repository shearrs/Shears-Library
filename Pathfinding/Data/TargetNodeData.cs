using Shears.Grids;
using UnityEngine;

namespace Shears.Pathfinding
{
    [System.Serializable]
    [DisallowMultipleComponent]
    public class TargetNodeData : GridNodeData
    {
        protected override Color EditorColor => Color.green;
    }
}
