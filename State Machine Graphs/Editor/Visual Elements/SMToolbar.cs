using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMToolbar : VisualElement
    {
        private readonly Action<StateMachineGraph> dataChangeCallback;

        public SMToolbar(StateMachineGraph data, Action<StateMachineGraph> dataChangeCallback)
        {
            AddToClassList("toolBar");

            this.dataChangeCallback = dataChangeCallback;
            this.AddStyleSheet(SMEditorUtil.ToolbarStyleSheet);

            CreateObjectField(data);
        }

        private void CreateObjectField(StateMachineGraph data)
        {
            ObjectField dataField = new("Data")
            {
                objectType = typeof(StateMachineGraph),
                value = data
            };

            dataField.AddToClassList("dataField");
            dataField.labelElement.AddToClassList("dataFieldLabel");
            dataField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(OnDataChanged);

            Add(dataField);
        }

        private void OnDataChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            dataChangeCallback?.Invoke((StateMachineGraph)evt.newValue);
        }
    }
}
