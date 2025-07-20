using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEditor;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateMachineNode : GraphMultiNode, IStateNode
    {
        private readonly StateMachineNodeData data;

        IStateNodeData IStateNode.Data => data;

        public StateMachineNode(StateMachineNodeData data, SMGraphView graphView, GraphData graphData) : base(data, graphView, graphData)
        {
            this.data = data;

            data.SetAsLayerDefault += OnSetAsLayerDefault;
            data.RemovedAsLayerDefault += OnRemovedAsLayerDefault;

            if (graphView.IsLayerDefault(data))
                OnSetAsLayerDefault();
        }

        ~StateMachineNode()
        {
            data.SetAsLayerDefault -= OnSetAsLayerDefault;
            data.RemovedAsLayerDefault -= OnRemovedAsLayerDefault;
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
