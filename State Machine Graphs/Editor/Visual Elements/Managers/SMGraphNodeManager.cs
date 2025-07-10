using Shears.GraphViews;
using Shears.GraphViews.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMGraphNodeManager
    {
        private readonly Dictionary<StateNodeData, StateNode> stateNodes = new();
        private readonly Dictionary<StateMachineNodeData, StateMachineNode> stateMachineNodes = new();

        private readonly SMGraphView graphView;

        public SMGraphNodeManager(SMGraphView graphView)
        {
            this.graphView = graphView;
            graphView.NodesCleared += OnNodesCleared;
        }

        public void OnNodesCleared()
        {
            stateNodes.Clear();
            stateMachineNodes.Clear();
        }

        public GraphNode CreateNode(GraphNodeData nodeData)
        {
            if (nodeData is StateNodeData stateNodeData)
                return CreateStateNode(stateNodeData);
            else if (nodeData is StateMachineNodeData stateMachineNodeData)
                return CreateStateMachineNode(stateMachineNodeData);

            return null;
        }

        private GraphNode CreateStateNode(StateNodeData nodeData)
        {
            var stateNode = new StateNode(nodeData);

            stateNodes.Add(nodeData, stateNode);

            return stateNode;
        }

        private GraphNode CreateStateMachineNode(StateMachineNodeData nodeData)
        {
            var stateMachineNode = new StateMachineNode(nodeData);

            stateMachineNodes.Add(nodeData, stateMachineNode);

            return stateMachineNode;
        }

        public void RemoveNode(GraphNodeData nodeData)
        {
            if (nodeData is StateNodeData stateNodeData)
                RemoveStateNode(stateNodeData);
            else if (nodeData is StateMachineNodeData stateMachineNodeData)
                RemoveStateMachineNode(stateMachineNodeData);
        }

        private void RemoveStateNode(StateNodeData nodeData)
        {
            if (!stateNodes.TryGetValue(nodeData, out var stateNode))
            {
                Debug.LogError("Can not find StateNode with ID: " + nodeData.ID);
                return;
            }

            stateNodes.Remove(nodeData);
        }

        private void RemoveStateMachineNode(StateMachineNodeData nodeData)
        {
            if (!stateMachineNodes.TryGetValue(nodeData, out var stateNode))
            {
                Debug.LogError("Can not find StateNode with ID: " + nodeData.ID);
                return;
            }

            stateMachineNodes.Remove(nodeData);
        }
    }
}
