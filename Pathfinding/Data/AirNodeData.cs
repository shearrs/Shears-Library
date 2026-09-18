using UnityEngine;

namespace Shears.Pathfinding
{
    [System.Serializable]
    [DisallowMultipleComponent]
    public class AirNodeData : PathNodeData
    {
        protected override Color EditorColor => Color.skyBlue;
        protected override bool EditorAutomaticTarget => false;
    }
}
