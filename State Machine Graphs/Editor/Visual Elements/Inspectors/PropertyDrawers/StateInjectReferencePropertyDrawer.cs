using Shears.Editor;
using Shears.GraphViews;
using Shears.Logging;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomPropertyDrawer(typeof(StateInjectReference))]
    public class StateInjectReferencePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.AddStyleSheet(ShearsStyles.InspectorStyles);
            root.AddToClassList(ShearsStyles.DarkContainerClass);

            var reference = property.boxedValue as StateInjectReference;
            var graphSO = property.serializedObject.FindProperty("graphData");

            StateMachineGraph graphData;
            var targetGraphData = graphSO.objectReferenceValue as StateMachineGraph;

            if (reference.GraphID == targetGraphData.ID)
                graphData = targetGraphData;
            else
                graphData = targetGraphData.GetExternalGraph(reference.GraphID);

            if (graphData == null)
            {
                SHLogger.Log("Could not find graph with ID: " + reference.GraphID, SHLogLevels.Error);
                return root;
            }

            var valueField = new ObjectField
            {
                label = reference.FieldType.Name,
                objectType = reference.FieldType
            };

            valueField.BindProperty(property.FindPropertyRelative("value"));

            // draw all the states that use this reference
            var targetsLabel = new Label("Injected Into States:\n");
            targetsLabel.style.marginLeft = 12;
            targetsLabel.style.color = new StyleColor(new Color(1.0f, 1.0f, 1.0f, 0.5f));
            foreach (var targetID in reference.TargetIDs)
            {
                if (!graphData.TryGetData(targetID, out GraphNodeData nodeData))
                    continue;

                if (nodeData is not IStateNodeData stateData)
                    continue;

                targetsLabel.text += $"- {stateData.Name}\n";
            }

            root.Add(valueField);
            root.Add(targetsLabel);

            return root;
        }
    }
}
