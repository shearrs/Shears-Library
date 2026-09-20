using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Shears.Logging;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public abstract class State : ISHLoggable
    {
        [SerializeField, ReadOnly]
        private string name;

        [SerializeField, ReadOnly]
        private SMID id;

        [SerializeField]
        private List<Transition> transitions = new();

        [SerializeReference]
        private State parentState;

        [SerializeReference]
        private State defaultSubState;

        private readonly List<Func<State>> manualTransitions = new();
        private IParameterProvider parameterProvider;
        private State subState;
        private bool isActive;

        internal SMID ID
        {
            get => id;
            set => id = value;
        }
        internal IParameterProvider ParameterProvider
        {
            get => parameterProvider;
            set => parameterProvider = value;
        }
        public bool IsActive
        {
            get => isActive;
            internal set => isActive = value;
        }
        public string Name
        {
            get
            {
                if (name == string.Empty)
                    name = GetType().Name;

                return name;
            }
            set => name = value;
        }
        public State ParentState
        {
            get => parentState;
            internal set => parentState = value;
        }
        public State DefaultSubState
        {
            get => defaultSubState;
            set => defaultSubState = value;
        }
        public State SubState
        {
            get => subState;
            internal set => subState = value;
        }
        public int TransitionCount => transitions.Count;

        public SHLogLevels LogLevels { get; set; } = SHLogLevels.Log | SHLogUtil.Issues;

        public void AddSubState(State state)
        {
            if (state == this)
            {
                LogError("State cannot be a substate of itself!");
                return;
            }

            state.ParentState = this;
        }

        public void AddSubStates(params State[] states)
        {
            foreach (var state in states)
                AddSubState(state);
        }

        public void AddTransition(Func<bool> manualTransition, State returnState)
        {
            manualTransitions.Add(() =>
            {
                if (manualTransition())
                    return returnState;

                return null;
            });
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

            foreach (var transition in manualTransitions)
            {
                var state = transition();

                if (state != null)
                {
                    newState = state;
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

        public virtual void DrawGizmos() { }

        protected SMID GetParameterID(string name) => parameterProvider.GetParameterID(name);

        protected T GetParameter<T>(string name) => parameterProvider.GetParameter<T>(name);

        protected T GetParameter<T>(SMID id) => parameterProvider.GetParameter<T>(id);

        protected void SetParameter(string name) => parameterProvider.SetParameter(name, true);

        protected void SetParameter<T>(string name, T value) =>
            parameterProvider.SetParameter(name, value);

        protected void SetParameter<T>(SMID id, T value)
        {
            parameterProvider.SetParameter(id, value);
        }

        /// <inheritdoc cref="ISHLoggableLogger.Log"/>
        [HideInCallstack]
        protected void Log(
            object message,
            UnityEngine.Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        ) =>
            ((ISHLoggable)this).Log(
                message,
                context,
                prefix,
                color,
                formatter,
                callerFilePath,
                callerLineNumber
            );

        /// <inheritdoc cref="ISHLoggableLogger.LogVerbose"/>
        [HideInCallstack]
        protected void LogVerbose(
            object message,
            UnityEngine.Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = default,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        ) =>
            ((ISHLoggable)this).LogVerbose(
                message,
                context,
                prefix,
                color,
                formatter,
                callerFilePath,
                callerLineNumber
            );

        /// <inheritdoc cref="ISHLoggableLogger.LogWarning"/>
        [HideInCallstack]
        protected void LogWarning(
            object message,
            UnityEngine.Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = default,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        ) =>
            ((ISHLoggable)this).LogWarning(
                message,
                context,
                prefix,
                color,
                formatter,
                callerFilePath,
                callerLineNumber
            );

        /// <inheritdoc cref="ISHLoggableLogger.LogError"/>
        [HideInCallstack]
        protected void LogError(
            object message,
            UnityEngine.Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = default,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        ) =>
            ((ISHLoggable)this).LogError(
                message,
                context,
                prefix,
                color,
                formatter,
                callerFilePath,
                callerLineNumber
            );
    }
}
