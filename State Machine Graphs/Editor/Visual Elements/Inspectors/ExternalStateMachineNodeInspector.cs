using Shears.GraphViews;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class ExternalStateMachineNodeInspector : GraphNodeInspector<ExternalStateMachineNodeData>
    {
        public ExternalStateMachineNodeInspector(GraphData graphData) : base(graphData)
        {
        }

        protected override void BuildInspector(VisualElement nameField, VisualElement transitions)
        {
            var externalGraphProp = nodeProp.FindPropertyRelative("externalGraphData");
            var graphField = new PropertyField();
            graphField.BindProperty(externalGraphProp);

            graphField.AddToClassList(SMEditorUtil.NodeTitleInspectorClassName);

            Add(nameField);
            Add(graphField);
            Add(transitions);
        }
    }
}
