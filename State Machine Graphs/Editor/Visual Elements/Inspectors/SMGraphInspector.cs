using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMGraphInspector : VisualElement
    {
        private readonly StateMachineGraph graphData;
        private readonly VisualElement selectionDisplay;

        public SMGraphInspector(StateMachineGraph graphData)
        {
            this.graphData = graphData;

            AddToClassList(SMEditorUtil.SMGraphInspectorClassName);
            this.AddStyleSheet(SMEditorUtil.SMGraphInspectorStyleSheet);

            selectionDisplay = new();
            UpdateSelectionDisplay();
            Add(selectionDisplay);

            graphData.SelectionChanged += OnSelectionChanged;
        }

        ~SMGraphInspector()
        {
            graphData.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            UpdateSelectionDisplay();
        }

        private void UpdateSelectionDisplay()
        {
            selectionDisplay.Clear();

            var testLabel = new Label();

            var selection = graphData.GetSelection();
            GraphElementData element = null;

            if (selection.Count > 0)
                element = selection[^1];

            if (element is StateNodeData stateNode)
                testLabel.text = "State Node: " + stateNode.Name;
            else if (element is StateMachineNodeData stateMachineNode)
                testLabel.text = "State Machine: " + stateMachineNode.Name;
            else if (element is TransitionEdgeData transition)
                testLabel.text = "Transition: " + transition.ID;
            else if (element is ParameterData parameter)
                testLabel.text = "Parameter: " + parameter.Name;
            else
                testLabel.text = "Select A Graph Element to Inspect";

            selectionDisplay.Add(testLabel);
        }
    }
}
