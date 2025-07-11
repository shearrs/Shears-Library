using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class EditableLabel : VisualElement
    {
        private readonly Label label;
        private readonly TextField textField;

        public string Text => textField.text;

        public event Action OnEndEditing;

        public EditableLabel(string text)
        {
            label = new(text)
            {
                pickingMode = PickingMode.Ignore
            };

            textField = new()
            {
                value = text,
                pickingMode = PickingMode.Ignore
            };

            label.AddToClassList(SMEditorUtil.EditableLabelLabelClassName);
            textField.AddToClassList(SMEditorUtil.EditableLabelTextFieldClassName);

            Add(label);
        }

        public void BeginEditing()
        {
            Remove(label);
            Add(textField);

            textField.Focus();
            textField.SelectAll();

            textField.RegisterCallback<FocusOutEvent>(OnFocusOut);
        }

        public void EndEditing()
        {
            if (textField.text == "")
                textField.value = label.text;

            label.text = textField.text;

            Remove(textField);
            Add(label);

            textField.UnregisterCallback<FocusOutEvent>(OnFocusOut);

            OnEndEditing?.Invoke();
        }

        private void OnFocusOut(FocusOutEvent evt) => EndEditing();
    }
}
