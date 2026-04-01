using Shears.Logging;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Shears.GameConsole
{
    public class GameConsoleUI : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TextMeshProUGUI consoleText;
        [SerializeField] private ResizableScrollRect scrollRect;

        public Canvas Canvas => canvas;

        private void OnEnable()
        {
            GameConsole.Enabled += Enable;
            GameConsole.Disabled += Disable;
            GameConsole.ConsoleTextChanged += UpdateConsoleText;
            GameConsole.InputRequested += OnInputRequested;
            inputField.onSubmit.AddListener(OnSubmit);
            inputField.onValidateInput += OnValidateInput;
        }

        private void OnDisable()
        {
            GameConsole.Enabled -= Enable;
            GameConsole.Disabled -= Disable;
            GameConsole.ConsoleTextChanged -= UpdateConsoleText;
            GameConsole.InputRequested -= OnInputRequested;
            inputField.onSubmit.RemoveListener(OnSubmit);
            inputField.onValidateInput -= OnValidateInput;
        }

        private void Update()
        {
            Canvas.worldCamera = Camera.main;
        }

        private void Enable()
        {
            canvas.enabled = true;
            inputField.Select();
            inputField.ActivateInputField();
        }

        private void Disable()
        {
            canvas.enabled = false;
        }

        private void OnSubmit(string input)
        {
            inputField.text = string.Empty;
            GameConsole.SubmitInput(input);

            inputField.Select();
            inputField.ActivateInputField();
        }

        private char OnValidateInput(string text, int charIndex, char addedChar)
        {
            if (addedChar != '`')
                return addedChar;

            return '\0';
        }

        private void OnInputRequested(string input)
        {
            inputField.text = input;

            CoroutineUtil.DoDeferred(() =>
            {
                inputField.caretPosition = input.Length;
                inputField.ForceLabelUpdate();
            });
        }

        private void UpdateConsoleText(string text)
        {
            consoleText.text += text + "\n";

            StopAllCoroutines();
            StartCoroutine(IEScrollToBottom());
        }

        private IEnumerator IEScrollToBottom()
        {
            yield return CoroutineUtil.WaitForEndOfFrame;

            scrollRect.verticalNormalizedPosition = 0;
        }
    }
}
