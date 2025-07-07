using Shears.GraphViews;
using Shears.GraphViews.Editor;
using System;
using System.Collections.Generic;
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

            evt.menu.AppendAction("Create State Node", (action) => PerformThenSave(() => graphData.CreateStateNodeData(mousePos)));
            evt.menu.AppendAction("Create State Machine Node", (action) => PerformThenSave(() => graphData.CreateStateMachineNodeData(mousePos)));
        }

        protected override void OnGraphDataSet(GraphData graphData)
        {
            if (graphData is not StateMachineGraph stateGraphData)
            {
                Debug.LogError("SMGraphView only accepts StateMachineGraph data!");
                return;
            }

            this.graphData = stateGraphData;
            nodeManager.SetGraphData(stateGraphData);
            toolbar.SetGraphData(stateGraphData);
        }

        protected override void OnGraphDataCleared()
        {
            graphData = null;
            nodeManager.ClearGraphData();
        }
        #endregion

        #region Loading
        protected override void LoadNodes(IReadOnlyCollection<GraphNodeData> nodeData)
        {
            foreach (var data in nodeData)
                nodeManager.CreateNode(data);
        }

        protected override void ClearNodes()
        {
            nodeManager.ClearNodes();
        }
        #endregion

        private void PerformThenSave(Action action)
        {
            action();

            GraphViewEditorUtil.Save(graphData);
        }
    }
}
