using Shears.GraphViews;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateNodeInspector : GraphNodeInspector<StateNodeData>
    {
        public StateNodeInspector(GraphData graphData) : base(graphData)
        {
        }

        protected override void BuildInspector(VisualElement nameField, VisualElement transitions)
        {
            var stateTypeProp = nodeProp.FindPropertyRelative("stateType");

            Add(nameField);
            Add(new StateSelector(stateTypeProp));
            Add(transitions);
        }
    }
}
