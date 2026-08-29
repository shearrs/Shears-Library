using UnityEngine;

namespace Shears.Grids
{
    [System.Serializable]
    [DisallowMultipleComponent]
    public class AirNodeData : GridNodeData
    {
        protected override Color EditorColor => Color.skyBlue;
        protected override bool EditorAutomaticTarget => false;
    }
}
