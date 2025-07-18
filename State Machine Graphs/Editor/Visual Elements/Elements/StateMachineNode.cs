using Shears.GraphViews.Editor;
using UnityEditor;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateMachineNode : GraphMultiNode
    {
        public StateMachineNode(StateMachineNodeData data, SerializedProperty nodeProp, GraphView graphView) : base(data, nodeProp, graphView)
        {
        }
    }
}
