using Shears.GraphViews;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMGraphNodeManager
    {
        private readonly List<StateNode> stateNodes = new();

        private StateMachineGraph graphData;
        private readonly SMGraphView graphView;

        public SMGraphNodeManager(SMGraphView graphView)
        {
            this.graphView = graphView;
        }

        ~SMGraphNodeManager()
        {
            UnsubscribeFromGraphData();
        }

        public void SetGraphData(StateMachineGraph graphData)
        {
            if (this.graphData != null)
                UnsubscribeFromGraphData();

            this.graphData = graphData;

            if (graphData != null)
                graphData.NodeDataCreated += CreateStateNode;
        }

        public void CreateNode(GraphNodeData nodeData)
        {
            if (nodeData is StateNodeData stateNodeData)
                CreateStateNode(stateNodeData);
        }

        private void CreateStateNode(StateNodeData nodeData)
        {
            var stateNode = new StateNode(nodeData);

            stateNodes.Add(stateNode);
            graphView.ContentViewContainer.Add(stateNode);
        }

        private void UnsubscribeFromGraphData()
        {
            graphData.NodeDataCreated -= CreateStateNode;
        }
    }
}
