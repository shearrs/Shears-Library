using UnityEngine;

namespace Shears.Grids
{
    [System.Serializable]
    [DisallowMultipleComponent]
    public class TargetNodeData : GridNodeData
    {
        protected override Color EditorColor => Color.green;
    }
}
