using Shears.GraphViews;
using Shears.GraphViews.Editor;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMGraphView : GraphView
    {
        private readonly SMGraphNodeManager nodeManager;
        private StateMachineGraph graphData;
        private SMToolbar toolbar;

        public SMGraphView() : base()
        {
            nodeManager = new(this);

            CreateToolbar();
            AddManipulators();

            GraphDataSet += OnGraphDataSet;
            GraphDataCleared += OnGraphDataCleared;
        }

        #region Initialization
        private void CreateToolbar()
        {
            var toolbarData = graphData;

            if (toolbarData == null)
                toolbarData = (StateMachineGraph)GraphEditorState.instance.GraphData;

            toolbar = new SMToolbar(toolbarData, SetGraphData, ClearGraphData);

            RootContainer.Insert(0, toolbar);
        }

        private void AddManipulators()
        {
            this.AddManipulator(new ContextualMenuManipulator(PopulateContextualMenu));
        }

        private void PopulateContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var target = evt.target as VisualElement;
            Vector2 mousePos = target.ChangeCoordinatesTo(ContentViewContainer, evt.localMousePosition);

            void perform(Action action, string actionName) => GraphViewEditorUtil.RecordAndSave(graphData, action, actionName);

            evt.menu.AppendAction("Create State Node", (action) => perform(() => graphData.CreateStateNodeData(mousePos), "Create State Node"));
            evt.menu.AppendAction("Create State Machine Node", (action) => perform(() => graphData.CreateStateMachineNodeData(mousePos), "Create State Machine Node"));
            if (graphData.SelectionCount > 0) evt.menu.AppendAction("Delete", (action) => DeleteSelection());
        }

        private void OnGraphDataSet(GraphData graphData)
        {
            if (graphData is not StateMachineGraph stateGraphData)
            {
                Debug.LogError("SMGraphView only accepts StateMachineGraph data!");
                return;
            }

            this.graphData = stateGraphData;
            toolbar.SetGraphData(stateGraphData);
        }

        private void OnGraphDataCleared()
        {
            graphData = null;
        }
        #endregion

        protected override GraphNode CreateNodeFromData(GraphNodeData data)
        {
            return nodeManager.CreateNode(data);
        }
    }
}
