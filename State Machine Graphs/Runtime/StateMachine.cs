using Shears.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [DefaultExecutionOrder(-100)]
    public class StateMachine : SHMonoBehaviourLogger, IParameterProvider
    {
        [Header("State Machine")]
        [SerializeField] private bool useGraphData = true;
        [SerializeField] private StateMachineGraph graphData;
        [SerializeField] private bool pollTransitions = true;
        [SerializeReference] private List<State> stateTree = new();
        [SerializeField] private StateInjectReferenceDictionary injectedReferences = new();

#if UNITY_EDITOR
#pragma warning disable 0414
        [SerializeField] private bool runtimeInfoExpanded = false;
        [SerializeField] private bool referencesExpanded = false;
        [SerializeReference] private List<Parameter> parameterDisplay = new();
        [SerializeField] private List<LocalParameterProvider> externalParameters = new();
#pragma warning restore 0414
#endif

        private State defaultState;
        private readonly List<State> swapStateTree = new();
        private readonly Dictionary<Type, State> stateTypeCache = new();
        private readonly Dictionary<Guid, State> states = new();
        private readonly Dictionary<string, Guid> parameterNameCache = new();
        private readonly Dictionary<Guid, Parameter> parameters = new();
        private int stateSwapID = 0;

        public IReadOnlyCollection<State> States => states.Values;
        public bool PollTransitions { get => pollTransitions; set => pollTransitions = value; }

        public event Action<State> EnteredState;

        private void Awake()
        {
            if (!useGraphData)
                return;
            else if (graphData == null)
            {
                Log("No graph data assigned to the state machine.", SHLogLevels.Warning);
                return;
            }

            CompileGraph();
            InjectStateReferences();
        }

        private void OnValidate()
        {
            foreach (var state in states.Values)
                state.LogLevels = LogLevels;
        }

        private void OnDisable()
        {
            EnterState(null);
        }

        private void Start()
        {
            if (graphData == null)
                return;

            EnterState(defaultState);
        }

        private void Update()
        {
            if (stateTree.Count == 0)
                return;

            if (pollTransitions)
                EvaluateState();

            UpdateState();
        }

        #region Graph Compilation
        private void CompileGraph()
        {
            var compiledData = graphData.GetData();

            foreach (var stringID in compiledData.StateIDs.Keys)
            {
                var state = compiledData.StateIDs[stringID];
                var id = Guid.NewGuid();
                state.ID = id;

                states.Add(id, state);
            }

            foreach (var state in states.Values)
            {
                var type = state.GetType();

                if (!stateTypeCache.ContainsKey(type))
                    stateTypeCache[type] = state;

                state.ParameterProvider ??= this;
            }

            defaultState = compiledData.DefaultState;

            foreach (var parameterName in compiledData.ParameterNames.Keys)
            {
                var parameter = compiledData.ParameterNames[parameterName];
                var id = Guid.NewGuid();
                parameter.ID = id;

                parameterNameCache.Add(parameterName, id);
                parameters.Add(id, parameter);
            }

#if UNITY_EDITOR
            parameterDisplay.AddRange(parameters.Values);
            externalParameters = compiledData.ParameterProviders;
#endif
        }

        private void InjectStateReferences()
        {
            foreach (var state in states.Values)
            {
                if (state is not IStateInjectable injectable)
                    continue;

                foreach (var reference in injectedReferences.Values)
                {
                    if (injectable.CanInjectType(reference.FieldType))
                        injectable.InjectType(reference.Value);
                }
            }
        }
        #endregion

        #region States
        /// <summary>
        /// Evaluates state transitions from the bottom to the top of the stack.
        /// </summary>
        private void EvaluateState()
        {
            foreach (var state in stateTree)
            {
                if (state.EvaluateTransitions(out var newState))
                {
                    EnterState(newState);
                    return;
                }
            }
        }

        /// <summary>
        /// Updates states from the bottom to the top of the stack.
        /// </summary>
        private void UpdateState()
        {
            int startID = stateSwapID;

            foreach (var state in stateTree)
            {
                state.Update();

                if (stateSwapID != startID)
                    break;
            }
        }

        public State EnterStateOfType<T>()
        {
            if (!stateTypeCache.TryGetValue(typeof(T), out var state))
            {
                Log($"StateMachine on {gameObject.name} does not have state of type '{typeof(T).Name}'!", SHLogLevels.Error);
                return null;
            }

            EnterState(state);

            return state;
        }

        public void AddState(State state)
        {
            if (state == null)
            {
                Log("State cannot be null!", SHLogLevels.Error);
                return;
            }

            Guid id = Guid.NewGuid();
            state.ID = id;
            states.Add(id, state);
            state.LogLevels = LogLevels;

            var type = state.GetType();

            if (!stateTypeCache.ContainsKey(type))
                stateTypeCache[type] = state;

            state.ParameterProvider ??= this;
        }

        public void AddStates(params State[] states)
        {
            if (states == null)
                return;

            foreach (var state in states)
                AddState(state);
        }

        public void EnterState(State newState)
        {
            if (newState != null)
                Log("Enter state: " + newState.Name, SHLogLevels.Verbose);
            else
                Log("Enter null", SHLogLevels.Verbose);

            swapStateTree.Clear();
            State currentState = newState;

            while (currentState != null)
            {
                swapStateTree.Add(currentState);
                currentState = currentState.ParentState;
            }

            swapStateTree.Reverse();
            stateSwapID++;

            if (stateSwapID > 255)
                stateSwapID = 0;

            ExitStateTree(swapStateTree);
            EnterStateTree(swapStateTree);
        }

        public bool IsInStateOfType<T>() where T : State
        {
            foreach (var state in stateTree)
            {
                if (state is T)
                    return true;
            }

            return false;
        }

        public bool IsInState(State state)
        {
            foreach (var currentState in stateTree)
            {
                if (state == currentState)
                    return true;
            }

            return false;
        }

        private void ExitStateTree(List<State> newStateTree)
        {
            if (stateTree.Count == 0)
                return;

            State currentState = stateTree[^1];

            while (currentState != null && !newStateTree.Contains(currentState))
            {
                currentState.Exit();
                stateTree.RemoveAt(stateTree.Count - 1);

                currentState = currentState.ParentState;
            }
        }

        private void EnterStateTree(List<State> newStateTree)
        {
            if (newStateTree.Count == 0)
                return;

            int currentID = stateSwapID;

            for (int i = 0; i < newStateTree.Count; i++)
            {
                State currentState = newStateTree[i];

                if (!stateTree.Contains(currentState))
                {
                    stateTree.Add(currentState);
                    currentState.Enter();

                    EnteredState?.Invoke(currentState);

                    if (i > 0)
                        newStateTree[i - 1].SetSubState(currentState);

                    if (stateSwapID != currentID)
                        return;
                }
            }

            State currentSubState = newStateTree[^1].DefaultSubState;

            while (currentSubState != null && !stateTree.Contains(currentSubState))
            {
                if (currentSubState.ID == null || !states.ContainsKey(currentSubState.ID))
                    Log($"StateMachine does not contain state {currentSubState}, did you forget to add it?", SHLogLevels.Warning);

                stateTree.Add(currentSubState);
                currentSubState.Enter();

                currentSubState.ParentState.SetSubState(currentSubState);
                currentSubState = currentSubState.DefaultSubState;
            }
        }
        #endregion 

        #region Parameters
        public Guid GetParameterID(string name)
        {
            if (parameterNameCache.TryGetValue(name, out var id))
                return id;
            else
            {
                Log($"Could not find parameter with name '{name}'.", SHLogLevels.Error);
                return Guid.Empty;
            }
        }

        public T GetParameter<T>(string name) => GetParameter<T>(GetParameterID(name));

        public T GetParameter<T>(Guid id)
        {
            if (parameters.TryGetValue(id, out var parameter))
            {
                if (parameter is Parameter<T> typedParameter)
                    return typedParameter.Value;
                else
                    Log($"Parameter '{parameter.Name}' is not of type {typeof(T)}.", SHLogLevels.Error);
            }
            else
                Log($"Could not find parameter with id '{id}' in the state machine.", SHLogLevels.Error);

            return default;
        }

        public void SetParameter<T>(string name, T value) => SetParameter(GetParameterID(name), value);

        public void SetParameter<T>(Guid id, T value)
        {
            if (parameters.TryGetValue(id, out var parameter))
            {
                if (parameter is Parameter<T> typedParameter)
                    typedParameter.Value = value;
                else
                    Log($"Parameter '{parameter.Name}' is not of type {typeof(T)}.", SHLogLevels.Error);
            }
            else
                Log($"Could not find parameter with id '{id}' in the state machine.", SHLogLevels.Error);
        }
        #endregion
    }
}
