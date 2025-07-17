using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMToolbar : VisualElement
    {
        private readonly SMGraphView graphView;
        private StateMachineGraph graphData;
        private SMLayerDisplay layerDisplay;
        private ObjectField dataField;

        public SMToolbar(SMGraphView graphView, StateMachineGraph graphData)
        {
            this.graphView = graphView;
            focusable = true;

            AddToClassList(SMEditorUtil.ToolbarClassName);
            this.AddStyleSheet(SMEditorUtil.ToolbarStyleSheet);
            
            CreateObjectField(graphData);
            CreateLayerDisplay();

            SetGraphData(graphData);
        }

        public void SetGraphData(StateMachineGraph graphData)
        {
            this.graphData = graphData;

            layerDisplay.SetGraphData(graphData);
            dataField.SetValueWithoutNotify(graphData);

            RegisterCallback<FocusInEvent>(OnFocusIn);
        }

        public void ClearGraphData()
        {
            graphData = null;

            layerDisplay.ClearGraphData();
            dataField.SetValueWithoutNotify(null);

            UnregisterCallback<FocusInEvent>(OnFocusIn);
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            if (graphData == null)
                return;

            graphView.Select(null);
        }

        private void CreateObjectField(StateMachineGraph data)
        {
            dataField = new ObjectField("Data")
            {
                objectType = typeof(StateMachineGraph),
                value = data
            };

            dataField.AddToClassList(SMEditorUtil.ToolbarDataFieldClassName);
            dataField.labelElement.AddToClassList(SMEditorUtil.ToolbarDataFieldLabelClassName);
            dataField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(OnDataChanged);

            Add(dataField);
        }

        private void CreateLayerDisplay()
        {
            layerDisplay = new SMLayerDisplay();

            Add(layerDisplay);
        }

        private void OnDataChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            var value = (StateMachineGraph)evt.newValue;

            if (value != null)
                graphView.SetGraphData(value);
            else
                graphView.ClearGraphData();
        }
    }
}
