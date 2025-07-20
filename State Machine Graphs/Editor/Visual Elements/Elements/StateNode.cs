using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEditor;
using UnityEditor.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateNode : GraphNode, IStateNode
    {
        private readonly StateNodeData data;

        IStateNodeData IStateNode.Data => data;

        public StateNode(StateNodeData data, SMGraphView graphView, GraphData graphData) : base(data, graphView, graphData)
        {
            this.data = data;

            data.SetAsLayerDefault += OnSetAsLayerDefault;
            data.RemovedAsLayerDefault += OnRemovedAsLayerDefault;

            if (graphView.IsLayerDefault(data))
                OnSetAsLayerDefault();
        }

        ~StateNode()
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
