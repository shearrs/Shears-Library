using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateNodeInspector : VisualElement
    {
        private readonly StateMachineGraph graphData;
        private StateNodeData nodeData;

        public StateNodeInspector(StateMachineGraph graphData)
        {
            this.graphData = graphData;
        }

        public void SetNode(StateNodeData data)
        {
            Clear();

            nodeData = data;

            var name = new TextField()
            {
                value = data.Name
            };

            Add(name);
        }
    }
}
