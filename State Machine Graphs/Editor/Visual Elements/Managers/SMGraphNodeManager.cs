using Shears.GraphViews;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMGraphNodeManager
    {
        private readonly List<StateNode> stateNodes = new();
        private readonly List<StateMachineNode> stateMachineNodes = new();

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
                ClearGraphData();

            this.graphData = graphData;

            if (graphData != null)
                graphData.NodeDataCreated += CreateNode;
        }

        public void ClearGraphData()
        {
            if (graphData != null)
                UnsubscribeFromGraphData();

            graphData = null;
        }

        public void ClearNodes()
        {
            foreach (var node in stateNodes)
                graphView.RemoveNode(node);

            foreach (var node in stateMachineNodes)
                graphView.RemoveNode(node);

            stateNodes.Clear();
            stateMachineNodes.Clear();
        }

        public void CreateNode(GraphNodeData nodeData)
        {
            if (nodeData is StateNodeData stateNodeData)
                CreateStateNode(stateNodeData);
            else if (nodeData is StateMachineNodeData stateMachineNodeData)
                CreateStateMachineNode(stateMachineNodeData);
        }

        private void CreateStateNode(StateNodeData nodeData)
        {
            var stateNode = new StateNode(nodeData);

            stateNodes.Add(stateNode);
            graphView.AddNode(stateNode);
        }

        private void CreateStateMachineNode(StateMachineNodeData nodeData)
        {
            var stateMachineNode = new StateMachineNode(nodeData);

            stateMachineNodes.Add(stateMachineNode);
            graphView.AddNode(stateMachineNode);
        }

        private void UnsubscribeFromGraphData()
        {
            graphData.NodeDataCreated -= CreateNode;
        }
    }
}
