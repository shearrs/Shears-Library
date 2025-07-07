using UnityEngine;

namespace Shears.GraphViews.Editor
{
    public class GraphMultiNode : GraphNode
    {
        private readonly GraphMultiNodeData data;

        public GraphMultiNodeData Data => data;

        public GraphMultiNode(GraphMultiNodeData data) : base(data)
        {
            this.data = data;
        }
    }
}
