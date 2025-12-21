using Shears.Input;
using Shears.Logging;
using Shears.Signals;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.GameConsole
{
    public class GameConsole : PersistentProtectedSingleton<GameConsole>
    {
        #region Variables
        private const string INPUT_MAP_PATH = "Shears Library/Game Console/GameConsole_InputMap";
        private const string UI_PATH = "Shears Library/Game Console/Game Console UI";

        private static readonly Color ERROR_COLOR = new(0.8f, 0.1f, 0.1f);
        private static readonly List<string> previousInputs = new();
        private static readonly Dictionary<Type, object> storedSingletons = new();
        private static readonly List<IConsoleCommand> commands = new()
        {
            new HelpCommand(),
        };

        private bool isEnabled = false;
        private int previousInputIndex = 0;
        private ManagedInputMap inputMap;
        private IManagedInput toggleInput;
        private IManagedInput previousCommandInput;
        private IManagedInput nextCommandInput;

        internal static IReadOnlyCollection<IConsoleCommand> Commands => commands;

        public static event Action Enabled;
        public static event Action Disabled;
        public static event Action<string> ConsoleTextChanged;
        public static event Action<string> InputRequested;
        #endregion

        #region Initialization
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearData()
        {
            storedSingletons.Clear();
            commands.Clear();

            AddCommand(new HelpCommand());
        }

        protected override void Awake()
        {
            base.Awake();

            if (this != Instance)
                return;

            inputMap = Resources.Load<ManagedInputMap>(INPUT_MAP_PATH);

            var uiPrefab = Resources.Load<GameConsoleUI>(UI_PATH);
            var ui = Instantiate(uiPrefab);
            ui.transform.SetParent(transform);

            toggleInput = inputMap.GetInput("Toggle");
            previousCommandInput = inputMap.GetInput("Previous Command");
            nextCommandInput = inputMap.GetInput("Next Command");
        }

        private void OnEnable()
        {
            toggleInput.Performed += OnToggleInput;
            previousCommandInput.Performed += OnPreviousInput;
            nextCommandInput.Performed += OnNextInput;
        }

        private void OnDisable()
        {
            toggleInput.Performed -= OnToggleInput;
            previousCommandInput.Performed -= OnPreviousInput;
            nextCommandInput.Performed -= OnNextInput;
        }

        private void Enable()
        {
            if (isEnabled)
                return;

            SignalShuttle.Emit(new ToggleInputSignal(false));

            isEnabled = true;

            Enabled?.Invoke();
        }

        private void Disable()
        {
            if (!isEnabled)
                return;

            SignalShuttle.Emit(new ToggleInputSignal(true));

            isEnabled = false;

            Disabled?.Invoke();
        }
        #endregion

        #region Singletons
        public static void SubmitInput(string inputText) => Instance.InstSubmitInput(inputText);
        private void InstSubmitInput(string inputText)
        {
            if (inputText.Length == 0)
                return;

            ConsoleMessage(inputText, Color.gray7);
            bool foundValidCommand = false;

            foreach (var command in commands)
            {
                if (inputText.StartsWith(command.Command))
                {
                    command.TryExecuteCommand(inputText, ConsoleMessage, ConsoleError);
                    foundValidCommand = true;

                    break;
                }
            }

            if (!foundValidCommand)
                ConsoleError($"Could not parse command '{inputText}'. Use 'help' to see a list of commands.");

            previousInputs.Add(inputText);
            previousInputIndex = previousInputs.Count;
        }

        public static void StoreSingleton<T>(T instance) => Instance.InstStoreSingleton(instance);
        private void InstStoreSingleton<T>(T instance)
        {
            storedSingletons.Add(typeof(T), instance);
        }

        public static T GetSingleton<T>() => Instance.InstGetSingleton<T>();
        private T InstGetSingleton<T>()
        {
            if (!storedSingletons.TryGetValue(typeof(T), out var singleton))
            {
                SHLogger.Log($"Could not retrieve singleton of type: {typeof(T).Name}!", SHLogLevels.Error);
                return default;
            }

            return (T)singleton;
        }

        public static void ClearSingleton<T>() => Instance.InstClearSingleton<T>();
        private void InstClearSingleton<T>()
        {
            storedSingletons.Remove(typeof(T));
        }
        #endregion

        #region Commands
        public static void AddCommand(IConsoleCommand command) => commands.Add(command); 

        public static void RemoveCommand(IConsoleCommand command) => commands.Remove(command);
        #endregion

        #region Inputs
        private void OnToggleInput()
        {
            if (isEnabled)
                Disable();
            else
                Enable();
        }

        private void OnPreviousInput()
        {
            if (previousInputs.Count == 0)
                return;

            previousInputIndex = Mathf.Clamp(previousInputIndex - 1, 0, previousInputs.Count - 1);

            string input = previousInputs[previousInputIndex];
            InputRequested?.Invoke(input);
        }

        private void OnNextInput()
        {
            if (previousInputIndex == previousInputs.Count)
                return;
            else if (previousInputIndex == previousInputs.Count - 1)
            {
                previousInputIndex++;
                InputRequested?.Invoke(string.Empty);

                return;
            }

            previousInputIndex = Mathf.Clamp(previousInputIndex + 1, 0, previousInputs.Count - 1);

            string input = previousInputs[previousInputIndex];
            InputRequested?.Invoke(input);
        }
        #endregion

        #region Console Messages
        private void ConsoleError(string text)
        {
            ConsoleMessage(text, ERROR_COLOR);
            SHLogger.Log(text, SHLogLevels.Error);
        }

        private void ConsoleMessage(string text) => ConsoleMessage(text, Color.white);

        private void ConsoleMessage(string text, Color color)
        {
            string colorString = ColorUtility.ToHtmlStringRGB(color);
            
            ConsoleTextChanged?.Invoke($"<color=#{colorString}>{text}</color>");
        }
        #endregion
    }
}
