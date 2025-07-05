using UnityEngine;

namespace Shears.GraphViews
{
    public abstract class GraphEdgeData : GraphElementData
    {
        private readonly GraphReference<GraphNodeData> from = new();
        private readonly GraphReference<GraphNodeData> to = new();

        public string FromID { get => from.ID; set => from.ID = value; }
        public string ToID { get => to.ID; set => to.ID = value; }
        public GraphNodeData From { get => from.Data; set => from.Data = value; }
        public GraphNodeData To { get => to.Data; set => to.Data = value; }

        public override void Select()
        {
        }

        public override void Deselect()
        {
        }
    }
}
