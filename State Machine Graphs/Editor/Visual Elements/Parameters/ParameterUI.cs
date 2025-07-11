using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class ParameterUI : GraphElement
    {
        private readonly StateMachineGraph graphData;
        private readonly ParameterData parameterData;

        private EditableLabel editableLabel;

        public ParameterUI(ParameterData parameterData, StateMachineGraph graphData)
        {
            this.graphData = graphData;
            this.parameterData = parameterData;

            AddToClassList(SMEditorUtil.ParameterUIClassName);

            CreateTextField();
            CreateValueField();

            RegisterCallbacks();

            parameterData.Selected += Select;
            parameterData.Deselected += Deselect;
        }

        ~ParameterUI()
        {
            parameterData.Selected -= Select;
            parameterData.Deselected -= Deselect;
        }

        private void CreateTextField()
        {
            editableLabel = new(parameterData.Name)
            {
                pickingMode = PickingMode.Ignore
            };

            editableLabel.AddToClassList(SMEditorUtil.EditableLabelClassName);
            editableLabel.OnEndEditing += UpdateParameterName;

            Add(editableLabel);
        }

        private void CreateValueField()
        {
            if (parameterData is BoolParameterData boolParameter)
            {
                Toggle toggle = new()
                {
                    value = boolParameter.Value
                };

                toggle.AddToClassList(SMEditorUtil.ParameterUIToggleClassName);
                toggle.RegisterCallback<ChangeEvent<bool>>((evt) => SetValue(evt.newValue));

                Add(toggle);
            }
        }

        private void SetValue<T>(T value)
        {
            GraphViewEditorUtil.Record(graphData, "Set Parameter Value");
            graphData.SetParameterValue((ParameterData<T>)parameterData, value);
            GraphViewEditorUtil.Save(graphData);
        }

        private void RegisterCallbacks()
        {
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 1)
                return;

            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Rename"), false, RenameParameter);
            menu.AddItem(new GUIContent("Delete"), false, RemoveParameter);

            menu.ShowAsContext();
        }

        private void RenameParameter()
        {
            editableLabel.BeginEditing();
        }

        private void UpdateParameterName()
        {
            GraphViewEditorUtil.Record(graphData, "Set Parameter Name");
            graphData.SetParameterName(parameterData, editableLabel.Text);
            GraphViewEditorUtil.Save(graphData);
        }

        private void RemoveParameter()
        {
            GraphViewEditorUtil.Record(graphData, "Remove Parameter");
            graphData.RemoveParameter(parameterData);
            GraphViewEditorUtil.Save(graphData);
        }

        public override void Select()
        {
            AddToClassList(SMEditorUtil.ParameterUISelectedClassName);
        }

        public override void Deselect()
        {
            RemoveFromClassList(SMEditorUtil.ParameterUISelectedClassName);
        }

        public override GraphElementData GetData() => parameterData;
    }
}
