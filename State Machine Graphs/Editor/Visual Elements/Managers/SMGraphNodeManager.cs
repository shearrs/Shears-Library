using Shears.GraphViews;
using Shears.GraphViews.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMGraphNodeManager
    {
        private readonly GraphView graphView;

        public SMGraphNodeManager(GraphView graphView)
        {
            this.graphView = graphView;
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
            var stateNode = new StateNode(nodeData, graphView);

            return stateNode;
        }

        private GraphNode CreateStateMachineNode(StateMachineNodeData nodeData)
        {
            var stateMachineNode = new SubStateMachineNode(nodeData, graphView);

            return stateMachineNode;
        }
    }
}
