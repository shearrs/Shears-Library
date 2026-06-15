using System;
using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateNode : GraphNode, IStateNode
    {
        private readonly StateNodeData data;
        private readonly VisualElement emptyTag;

        public StateNodeData Data => data;
        IStateNodeData IStateNode.Data => data;

        public StateNode(StateNodeData data, SMGraphView graphView, GraphData graphData)
            : base(data, graphView, graphData)
        {
            this.data = data;

            data.SetAsLayerDefault += OnSetAsLayerDefault;
            data.RemovedAsLayerDefault += OnRemovedAsLayerDefault;

            if (graphView.IsLayerDefault(data))
                OnSetAsLayerDefault();

            emptyTag = CreateEmptyTag();

            Add(emptyTag);

            data.StateTypeChanged += OnStateTypeChanged;
            OnStateTypeChanged();
        }

        ~StateNode()
        {
            data.SetAsLayerDefault -= OnSetAsLayerDefault;
            data.RemovedAsLayerDefault -= OnRemovedAsLayerDefault;
            data.StateTypeChanged -= OnStateTypeChanged;
        }

        private void OnStateTypeChanged()
        {
            if (data.StateType != StateSelector.EMPTY_STATE_TYPE)
            {
                emptyTag.style.visibility = Visibility.Hidden;
                return;
            }

            emptyTag.style.visibility = Visibility.Visible;
        }

        private void OnSetAsLayerDefault()
        {
            AddToClassList(SMEditorUtil.LayerDefaultNodeClassName);
        }

        private void OnRemovedAsLayerDefault()
        {
            RemoveFromClassList(SMEditorUtil.LayerDefaultNodeClassName);
        }

        private VisualElement CreateEmptyTag()
        {
            var tag = new VisualElement() { name = "Empty Tag" };

            tag.AddToClassList(SMEditorUtil.EmptyTagClassName);

            var label = new Label("Empty");
            label.style.alignSelf = Align.Center;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;

            tag.Add(label);

            return tag;
        }
    }
}
