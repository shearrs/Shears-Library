using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMGraphInspector : VisualElement
    {
        private readonly StateMachineGraph graphData;
        private readonly StateNodeInspector stateNodeInspector;
        private readonly TransitionEdgeInspector transitionEdgeInspector;
        private readonly VisualElement selectionDisplay;

        private IVisualElementScheduledItem inspectorPoll;

        public SMGraphInspector(StateMachineGraph graphData)
        {
            this.graphData = graphData;

            AddToClassList(SMEditorUtil.SMGraphInspectorClassName);
            this.AddStyleSheet(SMEditorUtil.SMGraphInspectorStyleSheet);

            stateNodeInspector = new(graphData);
            transitionEdgeInspector = new(graphData);
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
            if (GraphViewEditorUtil.IsInspectorLocked())
            {
                inspectorPoll ??= schedule.Execute(PollInspector).Every(1);

                return;
            }
            
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
            {
                stateNodeInspector.SetNode(stateNode);
                selectionDisplay.Add(stateNodeInspector);
            }
            else if (element is StateMachineNodeData stateMachineNode)
                testLabel.text = "State Machine: " + stateMachineNode.Name;
            else if (element is TransitionEdgeData transition)
            {
                transitionEdgeInspector.SetTransition(transition);
                selectionDisplay.Add(transitionEdgeInspector);
            }
            else if (element is ParameterData parameter)
                testLabel.text = "Parameter: " + parameter.Name;
            else
                testLabel.text = "Select A Graph Element to Inspect";

            selectionDisplay.Add(testLabel);
        }
    
        private void PollInspector()
        {
            if (GraphViewEditorUtil.IsInspectorLocked())
                return;

            inspectorPoll.Pause();
            inspectorPoll = null;

            UpdateSelectionDisplay();
        }
    }
}
