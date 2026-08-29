using UnityEngine;

namespace Shears.Grids
{
    [CreateAssetMenu(menuName = "Shears Library/Grid/Node Definition")]
    public class GridNodeDefinition : ScriptableObject
    {
        [SerializeField]
        private NodeObject nodeObject;

        [SerializeField]
        private GridNodeDataCache data = new();

        public NodeObject NodeObject => nodeObject;
        public GridNodeDataCache Data => data;
    }
}
