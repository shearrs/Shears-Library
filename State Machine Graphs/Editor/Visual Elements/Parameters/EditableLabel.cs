using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class EditableLabel : VisualElement
    {
        private readonly Label label;
        private readonly TextField textField;

        public string Text => textField.text;

        public Func<string, bool> ValidationCallback { get; set; }
        public event Action SuccessfulEditFinished;

        public EditableLabel(string text)
        {
            label = new(text)
            {
                pickingMode = PickingMode.Ignore
            };

            textField = new()
            {
                value = text,
                pickingMode = PickingMode.Ignore,
                isDelayed = true
            };

            label.AddToClassList(SMEditorUtil.EditableLabelLabelClassName);
            textField.AddToClassList(SMEditorUtil.EditableLabelTextFieldClassName);

            Add(label);
        }

        public void BindLabel(SerializedProperty property)
        {
            label.Unbind();
            label.BindProperty(property);
        }

        public void BeginEditing()
        {
            Remove(label);

            textField.value = label.text;
            Add(textField);

            textField.Focus();
            textField.SelectAll();

            textField.RegisterCallback<FocusOutEvent>(OnFocusOut);
        }

        public void EndEditing()
        {
            bool isValid = ValidationCallback == null || ValidationCallback(textField.text);

            if (!isValid)
                textField.value = label.text;
            else
            {
                if (textField.text == "")
                    textField.value = label.text;

                label.text = textField.text;
            }

            Remove(textField);
            Add(label);

            textField.UnregisterCallback<FocusOutEvent>(OnFocusOut);

            if (isValid)
                SuccessfulEditFinished?.Invoke();
        }

        private void OnFocusOut(FocusOutEvent evt) => EndEditing();
    }
}
