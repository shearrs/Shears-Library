using Shears.GraphViews.Editor;
using UnityEditor;

namespace Shears.StateMachineGraphs.Editor
{
    public class SubStateMachineNode : GraphMultiNode
    {
        public SubStateMachineNode(StateMachineNodeData data, SerializedProperty nodeProp, GraphView graphView) : base(data, nodeProp, graphView)
        {
        }
    }
}
