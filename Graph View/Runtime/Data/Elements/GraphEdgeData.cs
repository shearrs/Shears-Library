using UnityEngine;

namespace Shears.GraphViews
{
    [System.Serializable]
    public abstract class GraphEdgeData : GraphElementData
    {
        [SerializeField] private string fromID;
        [SerializeField] private string toID;

        public string FromID => fromID;
        public string ToID => toID;

        public GraphEdgeData(string fromID, string toID)
        {
            this.fromID = fromID;
            this.toID = toID;
        }

        public override void Select()
        {
        }

        public override void Deselect()
        {
        }
    }
}
