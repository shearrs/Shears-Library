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
        private readonly ContextualMenuManipulator contextMenu;
        private StateMachineGraph graphData;
        private SMToolbar toolbar;
        private SMParameterBar parameterBar;

        public SMGraphView() : base()
        {
            nodeManager = new(this);
            contextMenu = new(PopulateContextualMenu);

            CreateToolbar();
            CreateParameterBar();

            GraphDataSet += OnGraphDataSet;
            GraphDataCleared += OnGraphDataCleared;
            GraphViewEditorUtil.UndoRedoEvent += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            parameterBar.Reload();
        }

        #region Initialization
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
            AddManipulators();
        }

        private void OnGraphDataCleared()
        {
            graphData = null;
            parameterBar.ClearGraphData();
            toolbar.ClearGraphData();
            RemoveManipulators();
        }

        private void CreateToolbar()
        {
            var toolbarData = graphData;

            if (toolbarData == null)
                toolbarData = (StateMachineGraph)GraphEditorState.instance.GraphData;

            toolbar = new SMToolbar(this, toolbarData);

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
            GraphViewContainer.AddManipulator(contextMenu);
        }

        private void RemoveManipulators()
        {
            GraphViewContainer.RemoveManipulator(contextMenu);
        }

        private void PopulateContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var target = evt.target as VisualElement;
            Vector2 parameterMousePos = target.ChangeCoordinatesTo(parameterBar, evt.localMousePosition);

            if (parameterBar.ContainsPoint(parameterMousePos))
                return;

            Vector2 mousePos = target.ChangeCoordinatesTo(ContentViewContainer, evt.localMousePosition);

            void perform(Action action, string actionName) => GraphViewEditorUtil.RecordAndSave(graphData, action, actionName);

            if (graphData.SelectionCount == 1 && GetSelection()[0] is GraphNode node) evt.menu.AppendAction("Create Transition", (action) => BeginPlacingEdge(node, TryCreateTransition));
            evt.menu.AppendAction("Create State Node", (action) => perform(() => graphData.CreateStateNodeData(mousePos), "Create State Node"));
            evt.menu.AppendAction("Create State Machine Node", (action) => perform(() => graphData.CreateStateMachineNodeData(mousePos), "Create State Machine Node"));
            if (graphData.SelectionCount > 0) evt.menu.AppendAction("Delete", (action) => DeleteSelection());
        }
        #endregion

        private void TryCreateTransition(IEdgeAnchorable anchor1, IEdgeAnchorable anchor2)
        {
            var element1Data = anchor1.Element.GetData();
            var element2Data = anchor2.Element.GetData();

            if (element1Data is not ITransitionable transitionable1 || element2Data is not ITransitionable transitionable2)
                return;

            if (anchor1.HasConnectionTo(anchor2))
                return;

            GraphViewEditorUtil.Record(graphData);
            graphData.CreateTransitionData(transitionable1, transitionable2);
            GraphViewEditorUtil.Save(graphData);
        }

        protected override GraphNode CreateNodeFromData(GraphNodeData data)
        {
            return nodeManager.CreateNode(data);
        }

        protected override GraphEdge CreateEdgeFromData(GraphEdgeData data)
        {
            if (data is not TransitionEdgeData transitionData)
            {
                Debug.LogError("Received non transition edge data: " + data.ID);
                return null;
            }

            var from = GetNode(data.FromID);
            var to = GetNode(data.ToID);
            var edge = new TransitionEdge(transitionData, from, to);

            return edge;
        }
    }
}
