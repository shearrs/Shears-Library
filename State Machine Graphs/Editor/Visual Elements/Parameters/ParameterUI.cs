using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class ParameterUI : GraphElement, ISelectable
    {
        private readonly StateMachineGraph graphData;
        private readonly ParameterData parameterData;

        private EditableLabel editableLabel;

        public ParameterUI(ParameterData parameterData, StateMachineGraph graphData)
        {
            this.graphData = graphData;
            this.parameterData = parameterData;

            AddToClassList(SMEditorUtil.ParameterUIClassName);

            CreateMovementButtons();
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

        // TODO: make moving these work
        private void CreateMovementButtons()
        {
            var buttonContainer = new VisualElement();

            var upButton = new Button
            {
                text = "↑"
            };
            var downButton = new Button
            {
                text = "↓"
            };

            buttonContainer.AddToClassList(SMEditorUtil.ParameterUIMovementButtonsClassName);
            buttonContainer.Add(upButton);
            buttonContainer.Add(downButton);

            Add(buttonContainer);
        }

        private void CreateTextField()
        {
            editableLabel = new(parameterData.Name)
            {
                pickingMode = PickingMode.Ignore
            };

            editableLabel.AddToClassList(SMEditorUtil.EditableLabelClassName);
            editableLabel.ValidationCallback = graphData.IsUsableParameterName;
            editableLabel.SuccessfulEditFinished += UpdateParameterName;

            Add(editableLabel);
        }

        private void CreateValueField()
        {
            if (parameterData is BoolParameterData boolParameter)
            {
                var toggle = new Toggle()
                {
                    value = boolParameter.Value
                };

                toggle.AddToClassList(SMEditorUtil.ParameterUIToggleClassName);
                toggle.RegisterCallback<ChangeEvent<bool>>((evt) => SetValue(evt.newValue));

                Add(toggle);
            }
            else if (parameterData is IntParameterData intParameter)
            {
                var field = new IntegerField()
                {
                    value = intParameter.Value
                };

                field.AddToClassList(SMEditorUtil.ParameterUIIntFieldClassName);
                field.RegisterCallback<ChangeEvent<int>>((evt) => SetValue(evt.newValue));

                Add(field);
            }
            else if (parameterData is FloatParameterData floatParameter)
            {
                var field = new FloatField()
                {
                    value = floatParameter.Value
                };

                field.AddToClassList(SMEditorUtil.ParameterUIIntFieldClassName);
                field.RegisterCallback<ChangeEvent<float>>((evt) => SetValue(evt.newValue));

                Add(field);
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

        public void RenameParameter()
        {
            graphData.Select(parameterData);
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

        public void Select()
        {
            AddToClassList(SMEditorUtil.ParameterUISelectedClassName);
        }

        public void Deselect()
        {
            RemoveFromClassList(SMEditorUtil.ParameterUISelectedClassName);
        }

        public override GraphElementData GetData() => parameterData;
    }
}
