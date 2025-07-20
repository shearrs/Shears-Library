using Shears.GraphViews;
using Shears.GraphViews.Editor;
using Shears.Logging;
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
            this.AddStyleSheet(SMEditorUtil.GraphStyleSheet);

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
            nodeManager.SetGraphData(graphData);
            AddManipulators();
        }

        private void OnGraphDataCleared()
        {
            graphData = null;
            parameterBar.ClearGraphData();
            toolbar.ClearGraphData();
            nodeManager.ClearGraphData();
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

            parameterBar = new SMParameterBar(this, parameterBarData);

            BodyContainer.Insert(0, parameterBar);
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

            if (graphData.SelectionCount == 1 && GetSelection()[0] is IStateNode node)
            {
                evt.menu.AppendAction("Create Transition", (action) => BeginPlacingEdge(node, TryCreateTransition));
                evt.menu.AppendAction("Set as Layer Default State", (action) => SetAsLayerDefault(node));
            }
            evt.menu.AppendAction("Create State Node", (action) => CreateStateNode(mousePos));
            evt.menu.AppendAction("Create State Machine Node", (action) => CreateStateMachineNode(mousePos));
            evt.menu.AppendAction("Create External State Machine Node", (action) => CreateExternalStateMachineNode(mousePos));
            if (graphData.SelectionCount > 0) evt.menu.AppendAction("Delete", (action) => DeleteSelection());
        }
        #endregion

        public bool IsLayerDefault(IStateNodeData nodeData)
        {
            if (nodeData.ParentID == GraphLayer.ROOT_ID)
                return nodeData.ID == graphData.RootDefaultStateID;
            else if (graphData.TryGetData(nodeData.ParentID, out StateMachineNodeData stateMachineData))
                return stateMachineData.DefaultStateID == nodeData.ID;
            else
            {
                SHLogger.Log("Could not find parent with ID: " +  nodeData.ParentID, SHLogLevels.Error);
                return false;
            }
        }

        private void SetAsLayerDefault(IStateNode node)
        {
            Record("Set Layer Default State");
            graphData.SetLayerDefault(node.Data);
            Save();
        }

        private void CreateStateNode(Vector2 pos)
        {
            Record("Create State Node");
            var nodeData = graphData.CreateStateNodeData(pos);
            Save();

            var node = GetNode(nodeData);
            Select(node);
        }

        private void CreateStateMachineNode(Vector2 pos)
        {
            Record("Create State Machine Node");
            var nodeData = graphData.CreateStateMachineNodeData(pos);
            Save();

            var node = GetNode(nodeData);
            Select(node);
        }

        private void CreateExternalStateMachineNode(Vector2 pos)
        {
            Record("Create External State Machine Node");
            var nodeData = graphData.CreateExternalStateMachineNode(pos);
            Save();

            var node = GetNode(nodeData);
            Select(node);
        }

        private void TryCreateTransition(IEdgeAnchorable anchor1, IEdgeAnchorable anchor2)
        {
            if (anchor1 == anchor2)
                return;

            var element1Data = anchor1.Element.GetData();
            var element2Data = anchor2.Element.GetData();

            if (element1Data is not ITransitionable transitionable1 || element2Data is not ITransitionable transitionable2)
                return;

            if (anchor1.HasConnectionTo(anchor2))
                return;

            Record("Add Transition");
            var transitionData = graphData.CreateTransitionData(transitionable1, transitionable2);
            Save();

            var edge = GetEdge(transitionData);
            Select(edge);
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
