using Shears.GraphViews;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateMachineNodeInspector : GraphNodeInspector<StateMachineNodeData>
    {
        public StateMachineNodeInspector(GraphData graphData) : base(graphData)
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
