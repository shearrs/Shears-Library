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
            var toolbar = new SMToolbar(graphData, SetStateMachineGraph);

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

        private void SetStateMachineGraph(StateMachineGraph graphData)
        {
            if (this.graphData == graphData)
                return;

            this.graphData = graphData;
            SetGraphData(graphData);
            nodeManager.SetGraphData(graphData);

            if (graphData != null)
                LoadGraphData();
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
