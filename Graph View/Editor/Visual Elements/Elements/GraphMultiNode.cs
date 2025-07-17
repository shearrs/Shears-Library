using UnityEditor;
using UnityEngine;

namespace Shears.GraphViews.Editor
{
    public class GraphMultiNode : GraphNode
    {
        private readonly GraphMultiNodeData data;

        public GraphMultiNodeData Data => data;

        public GraphMultiNode(GraphMultiNodeData data, SerializedProperty nodeProperty, GraphView graphView) : base(data, nodeProperty, graphView)
        {
            this.data = data;

            AddToClassList(GraphViewEditorUtil.GraphMultiNodeClassName);
        }
    }
}
