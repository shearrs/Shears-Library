using Shears.GraphViews;
using Shears.GraphViews.Editor;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class ParameterUI : GraphElement, ISelectable
    {
        private readonly StateMachineGraph graphData;
        private readonly ParameterData parameterData;
        private readonly Action reloadCallback;

        private EditableLabel editableLabel;
        private Button moveUpButton;
        private Button moveDownButton;

        public ParameterUI(ParameterData parameterData, StateMachineGraph graphData, Action reloadCallback)
        {
            this.graphData = graphData;
            this.parameterData = parameterData;
            this.reloadCallback = reloadCallback;

            AddToClassList(SMEditorUtil.ParameterUIClassName);

            CreateMovementButtons();
            CreateTextField();
            CreateValueField();

            RegisterCallbacks();

            parameterData.Selected += Select;
            parameterData.Deselected += Deselect;

            graphData.ParameterDataAdded += UpdateMovementButtons;
            graphData.ParameterDataRemoved += UpdateMovementButtons;
        }

        ~ParameterUI()
        {
            parameterData.Selected -= Select;
            parameterData.Deselected -= Deselect;
            graphData.ParameterDataAdded -= UpdateMovementButtons;
            graphData.ParameterDataRemoved -= UpdateMovementButtons;
        }

        private void CreateMovementButtons()
        {
            var parameters = graphData.GetParameters();
            var buttonContainer = new VisualElement();

            moveUpButton = new Button(MoveParameterUp)
            {
                text = "↑"
            };
            moveDownButton = new Button(MoveParameterDown)
            {
                text = "↓"
            };

            if (parameters[0] == parameterData)
                moveUpButton.SetEnabled(false);
            if (parameters[^1] == parameterData)
                moveDownButton.SetEnabled(false);

            buttonContainer.AddToClassList(SMEditorUtil.ParameterUIMovementButtonsClassName);
            buttonContainer.Add(moveUpButton);
            buttonContainer.Add(moveDownButton);

            Add(buttonContainer);
        }

        private void UpdateMovementButtons(ParameterData data)
        {
            var parameters = graphData.GetParameters();
            moveUpButton.SetEnabled(parameters[0] != parameterData);
            moveDownButton.SetEnabled(parameters[^1] != parameterData);
        }

        private void MoveParameterUp()
        {
            graphData.MoveParameterUp(parameterData);
            reloadCallback();
        }

        private void MoveParameterDown()
        {
            graphData.MoveParameterDown(parameterData);
            reloadCallback();
        }

        private void CreateTextField()
        {
            var parameterProp = GraphViewEditorUtil.GetElementProp(graphData, parameterData.ID);

            editableLabel = new(parameterData.Name)
            {
                pickingMode = PickingMode.Ignore
            };

            editableLabel.BindLabel(parameterProp.FindPropertyRelative("name"));
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
