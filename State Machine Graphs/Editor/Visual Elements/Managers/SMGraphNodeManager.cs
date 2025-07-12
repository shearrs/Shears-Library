using Shears.GraphViews;
using Shears.GraphViews.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMGraphNodeManager
    {
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

            return stateNode;
        }

        private GraphNode CreateStateMachineNode(StateMachineNodeData nodeData)
        {
            var stateMachineNode = new SubStateMachineNode(nodeData);

            return stateMachineNode;
        }
    }
}
