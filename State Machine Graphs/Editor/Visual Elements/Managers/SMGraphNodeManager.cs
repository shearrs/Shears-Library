using Shears.GraphViews;
using Shears.GraphViews.Editor;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMGraphNodeManager
    {
        private readonly SMGraphView graphView;
        private GraphData graphData;

        public SMGraphNodeManager(SMGraphView graphView)
        {
            this.graphView = graphView;
        }

        public void SetGraphData(GraphData data)
        {
            graphData = data;
        }

        public void ClearGraphData()
        {
            graphData = null;
        }

        public GraphNode CreateNode(GraphNodeData nodeData)
        {
            if (nodeData is StateNodeData stateNodeData)
                return CreateStateNode(stateNodeData);
            else if (nodeData is StateMachineNodeData stateMachineNodeData)
                return CreateStateMachineNode(stateMachineNodeData);
            else if (nodeData is ExternalStateMachineNodeData external)
                return CreateExternalStateMachineNode(external);

            return null;
        }

        private GraphNode CreateStateNode(StateNodeData nodeData)
        {
            var stateNode = new StateNode(nodeData, graphView, graphData);

            return stateNode;
        }

        private GraphNode CreateStateMachineNode(StateMachineNodeData nodeData)
        {
            var stateMachineNode = new StateMachineNode(nodeData, graphView, graphData);

            return stateMachineNode;
        }

        private GraphNode CreateExternalStateMachineNode(ExternalStateMachineNodeData nodeData)
        {
            var stateMachineNode = new ExternalStateMachineNode(nodeData, graphView, graphData);

            return stateMachineNode;
        }
    }
}
