using Shears.GraphViews;
using Shears.GraphViews.Editor;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMGraphNodeManager
    {
        private readonly GraphView graphView;
        private GraphData graphData;

        public SMGraphNodeManager(GraphView graphView)
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
            var prop = GraphViewEditorUtil.GetElementProp(graphData, nodeData.ID);
            var stateNode = new StateNode(nodeData, prop, graphView);

            return stateNode;
        }

        private GraphNode CreateStateMachineNode(StateMachineNodeData nodeData)
        {
            var prop = GraphViewEditorUtil.GetElementProp(graphData, nodeData.ID);
            var stateMachineNode = new StateMachineNode(nodeData, prop, graphView);

            return stateMachineNode;
        }

        private GraphNode CreateExternalStateMachineNode(ExternalStateMachineNodeData nodeData)
        {
            var prop = GraphViewEditorUtil.GetElementProp(graphData, nodeData.ID);
            var stateMachineNode = new ExternalStateMachineNode(nodeData, prop, graphView);

            return stateMachineNode;
        }
    }
}
