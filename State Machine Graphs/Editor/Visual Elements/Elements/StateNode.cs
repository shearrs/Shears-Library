using Shears.GraphViews.Editor;
using UnityEditor;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateNode : GraphNode
    {
        public StateNode(StateNodeData data, SerializedProperty nodeProperty, GraphView graphView) : base(data, nodeProperty, graphView)
        {
        }
    }
}
