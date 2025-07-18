using Shears.GraphViews.Editor;
using UnityEditor;

namespace Shears.StateMachineGraphs.Editor
{
    public class ExternalStateMachineNode : GraphNode
    {
        public ExternalStateMachineNode(ExternalStateMachineNodeData data, SerializedProperty nodeProperty, GraphView graphView) : base(data, nodeProperty, graphView)
        {
            this.AddStyleSheet(SMEditorUtil.GraphStyleSheet);
            AddToClassList(SMEditorUtil.ExternalStateMachineNodeClassName);
        }
    }
}
