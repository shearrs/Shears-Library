using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMGraphView : GraphView
    {
        private readonly SMGraphNodeManager nodeManager;
        private StateMachineGraph graphData;

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

            var toolbar = new SMToolbar(toolbarData, SetGraphData, ClearGraphData);

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

            evt.menu.AppendAction("Create State Node", (action) => graphData.CreateStateNodeData(mousePos));
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

            LoadGraphData();
        }

        protected override void OnGraphDataCleared()
        {
            graphData = null;
            nodeManager.ClearGraphData();
        }
        #endregion

        #region Loading
        private void LoadGraphData()
        {
            LoadNodes();
        }

        private void LoadNodes()
        {
            foreach (var nodeData in graphData.NodeData)
                nodeManager.CreateNode(nodeData);
        }
        #endregion
    }
}
