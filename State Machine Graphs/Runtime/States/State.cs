using Shears.Logging;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public abstract class State : ISHLoggable
    {
        [SerializeField, ReadOnly] private string name;
        [SerializeField] private List<Transition> transitions = new();

        private IParameterProvider parameterProvider;
        private State initialSubState;
        private State parentState;
        private State subState;
        private bool isActive;

        internal IParameterProvider ParameterProvider { get => parameterProvider; set => parameterProvider = value; }
        public bool IsActive { get => isActive; internal set => isActive = value; }
        public string Name { get => name; set => name = value; }
        public State ParentState { get => parentState; internal set => parentState = value; }
        public State DefaultSubState { get => initialSubState; set => initialSubState = value; }
        public State SubState { get => subState; internal set => subState = value; }
        public int TransitionCount => transitions.Count;

        public SHLogLevels LogLevels { get; set; } = SHLogLevels.Issues;

        public void AddSubState(State state)
        {
            if (state == this)
            {
                Log("State cannot be a substate of itself!", SHLogLevels.Error);
                return;
            }

            state.ParentState = this;
        }

        internal void AddTransition(Transition transition) => transitions.Add(transition);

        internal bool EvaluateTransitions(out State newState)
        {
            foreach (var transition in transitions)
            {
                if (transition.Evaluate())
                {
                    newState = transition.To;
                    return true;
                }
            }
            
            newState = null;
            return false;
        }

        internal void SetSubState(State subState)
        {
            this.subState = subState;
        }

        internal void Enter()
        {
            isActive = true;

            OnEnter();
        }

        internal void Update()
        {
            OnUpdate();
        }

        internal void Exit()
        {
            isActive = false;

            OnExit();
        }

        protected abstract void OnEnter();
        protected abstract void OnUpdate();
        protected abstract void OnExit();

        protected T GetParameter<T>(string name) => parameterProvider.GetParameter<T>(name);
        protected void SetParameter<T>(string name, T value) => parameterProvider.SetParameter(name, value);

        /// <summary>
        /// Logs a message to the current <see cref="ISHLogger"/>.
        /// </summary>
        /// <param name="message">The log to send.</param>
        /// <param name="context">The context associated with this log. If the <see cref="SHLogger"/>'s <see cref="LogType"/> is set to <see cref="LogType.UnityConsole"/>, the context will be highlighted upon selecting the log.</param>
        /// <param name="prefix">A custom prefix for this log.</param>
        /// <param name="level">The severity/level of this log.</param>
        /// <param name="color">A custom <see cref="Color"/> for this log.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        [HideInCallstack]
        protected void Log(string message, SHLogLevels level = SHLogLevels.Log, Color color = default, Object context = null, string prefix = "", ISHLogFormatter formatter = default,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        => this.Log(new SHLog(message, context, prefix, level, color), formatter, callerFilePath, callerLineNumber);
    }
}
