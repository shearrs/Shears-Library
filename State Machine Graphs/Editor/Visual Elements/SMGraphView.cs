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
        private SMParameterBar parameterBar;

        public SMGraphView() : base()
        {
            nodeManager = new(this);

            CreateToolbar();
            CreateParameterBar();
            AddManipulators();

            GraphDataSet += OnGraphDataSet;
            GraphDataCleared += OnGraphDataCleared;
            GraphViewEditorUtil.UndoRedoEvent += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            parameterBar.Reload();
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

        private void CreateParameterBar()
        {
            var parameterBarData = graphData;

            if (parameterBarData == null)
                parameterBarData = (StateMachineGraph)GraphEditorState.instance.GraphData;

            parameterBar = new SMParameterBar(parameterBarData);

            GraphViewContainer.Insert(0, parameterBar);
            parameterBar.BringToFront();
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
            parameterBar.SetGraphData(stateGraphData);
        }

        private void OnGraphDataCleared()
        {
            graphData = null;
            parameterBar.ClearGraphData();
            toolbar.ClearGraphData();
        }
        #endregion

        protected override GraphNode CreateNodeFromData(GraphNodeData data)
        {
            return nodeManager.CreateNode(data);
        }
    }
}
