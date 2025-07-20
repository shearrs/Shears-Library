using UnityEditor;
using UnityEngine;

namespace Shears.GraphViews.Editor
{
    public class GraphMultiNode : GraphNode
    {
        private readonly GraphMultiNodeData data;

        public GraphMultiNodeData Data => data;

        public GraphMultiNode(GraphMultiNodeData data, GraphView graphView, GraphData graphData) : base(data, graphView, graphData)
        {
            this.data = data;

            AddToClassList(GraphViewEditorUtil.GraphMultiNodeClassName);
        }
    }
}
