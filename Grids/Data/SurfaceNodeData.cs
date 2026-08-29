using UnityEngine;

namespace Shears.Grids
{
    [System.Serializable]
    [DisallowMultipleComponent]
    public class SurfaceNodeData : GridNodeData
    {
        [SerializeField]
        private bool isWalkable = true;

        protected override Color EditorColor => Color.yellowNice;
    }
}
