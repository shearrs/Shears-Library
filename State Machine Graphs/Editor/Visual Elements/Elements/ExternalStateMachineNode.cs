using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEditor;

namespace Shears.StateMachineGraphs.Editor
{
    public class ExternalStateMachineNode : GraphNode
    {
        public ExternalStateMachineNode(ExternalStateMachineNodeData data, GraphView graphView, GraphData graphData) : base(data, graphView, graphData)
        {
            AddToClassList(SMEditorUtil.ExternalStateMachineNodeClassName);
        }
    }
}
