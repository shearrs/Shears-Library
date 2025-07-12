using Shears.GraphViews;
using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMToolbar : VisualElement
    {
        private readonly Action<GraphData> dataSetCallback;
        private readonly Action dataClearCallback;
        private SMLayerDisplay layerDisplay;
        private ObjectField dataField;

        public SMToolbar(StateMachineGraph graphData, Action<GraphData> dataSetCallback, Action dataClearCallback)
        {
            AddToClassList(SMEditorUtil.ToolbarClassName);

            this.dataSetCallback = dataSetCallback;
            this.dataClearCallback = dataClearCallback;
            this.AddStyleSheet(SMEditorUtil.ToolbarStyleSheet);
            
            CreateObjectField(graphData);
            CreateLayerDisplay();

            SetGraphData(graphData);
        }

        public void SetGraphData(StateMachineGraph graphData)
        {
            layerDisplay.SetGraphData(graphData);
            dataField.SetValueWithoutNotify(graphData);
        }

        public void ClearGraphData()
        {
            layerDisplay.ClearGraphData();
            dataField.SetValueWithoutNotify(null);
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
                dataSetCallback?.Invoke(value);
            else
                dataClearCallback?.Invoke();
        }
    }
}
