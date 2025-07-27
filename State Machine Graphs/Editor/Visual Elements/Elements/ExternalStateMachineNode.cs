using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEditor;

namespace Shears.StateMachineGraphs.Editor
{
    public class ExternalStateMachineNode : GraphNode, IStateNode
    {
        IStateNodeData IStateNode.Data => (IStateNodeData)GetData();

        public ExternalStateMachineNode(ExternalStateMachineNodeData data, SMGraphView graphView, GraphData graphData) : base(data, graphView, graphData)
        {
            AddToClassList(SMEditorUtil.ExternalStateMachineNodeClassName);

            data.SetAsLayerDefault += OnSetAsLayerDefault;
            data.RemovedAsLayerDefault += OnRemovedAsLayerDefault;

            if (graphView.IsLayerDefault(data))
                OnSetAsLayerDefault();
        }

        private void OnSetAsLayerDefault()
        {
            AddToClassList(SMEditorUtil.LayerDefaultNodeClassName);
        }

        private void OnRemovedAsLayerDefault()
        {
            RemoveFromClassList(SMEditorUtil.LayerDefaultNodeClassName);
        }
    }
}
