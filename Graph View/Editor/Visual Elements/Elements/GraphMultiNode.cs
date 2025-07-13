using UnityEngine;

namespace Shears.GraphViews.Editor
{
    public class GraphMultiNode : GraphNode
    {
        private readonly GraphMultiNodeData data;

        public GraphMultiNodeData Data => data;

        public GraphMultiNode(GraphMultiNodeData data, GraphView graphView) : base(data, graphView)
        {
            this.data = data;
        }
    }
}
