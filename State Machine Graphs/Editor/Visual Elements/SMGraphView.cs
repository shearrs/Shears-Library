using Shears.GraphViews;
using Shears.GraphViews.Editor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMGraphView : GraphView
    {
        private readonly SMGraphNodeManager nodeManager;
        private readonly ContextualMenuManipulator contextMenu;
        private readonly List<ISelectable> instanceSelection = new();
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
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnUndoRedo()
        {
            parameterBar.Reload();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            bool hasSelection = GetSelection().Count > 0;


            if (hasSelection && evt.keyCode == KeyCode.F2)
                TryRenameSelection();
        }

        private void TryRenameSelection()
        {
            var selection = GetSelection();

            if (selection.Count != 1)
                return;

            if (selection[0] is ParameterUI parameter)
                parameter.RenameParameter();
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
            nodeManager.SetGraphData(stateGraphData);
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
            Debug.Log("context menu");

            var target = evt.target as VisualElement;
            Vector2 parameterMousePos = target.ChangeCoordinatesTo(parameterBar, evt.localMousePosition);

            if (parameterBar.ContainsPoint(parameterMousePos))
                return;

            Vector2 mousePos = target.ChangeCoordinatesTo(ContentViewContainer, evt.localMousePosition);

            if (graphData.SelectionCount == 1 && GetSelection()[0] is IEdgeAnchorable anchorable)
            {
                evt.menu.AppendAction("Create Transition", (action) => BeginPlacingEdge(anchorable, TryCreateTransition));

                if (GetSelection()[0] is IStateNode node)
                    evt.menu.AppendAction("Set as Layer Default State", (action) => SetAsLayerDefault(node));
            }
            evt.menu.AppendAction("Create State Node", (action) => CreateStateNode(mousePos));
            evt.menu.AppendAction("Create State Machine Node", (action) => CreateStateMachineNode(mousePos));
            evt.menu.AppendAction("Create External State Machine Node", (action) => CreateExternalStateMachineNode(mousePos));
            if (graphData.SelectionCount > 0) evt.menu.AppendAction("Delete", (action) => DeleteSelection());
        }
        #endregion

        public bool IsLayerDefault(IStateNodeData nodeData) => graphData.IsLayerDefault(nodeData);

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

        protected override IReadOnlyList<ISelectable> GetExtraSelection()
        {
            var selectionData = graphData.GetSelection();
            instanceSelection.Clear();

            foreach (var data in selectionData)
            {
                if (data is ParameterData)
                    instanceSelection.Add(parameterBar.ParameterUIs[data.ID]);
            }

            return instanceSelection;
        }
    }
}
