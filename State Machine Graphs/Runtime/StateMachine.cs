using Shears.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class StateMachine : SHMonoBehaviourLogger, IParameterProvider
    {
        [SerializeField] private bool useGraphData = true;
        [SerializeField, ShowIf("useGraphData")] private StateMachineGraph graphData;
        [SerializeReference] private List<IState> stateTree = new();
        [SerializeField] private StateInjectReferenceDictionary injectedReferences = new();

#if UNITY_EDITOR
#pragma warning disable 0414
        [SerializeField] private bool runtimeInfoExpanded = false;
        [SerializeField] private bool referencesExpanded = false;
        [SerializeReference] private List<Parameter> parameterDisplay = new();
        [SerializeField] private List<LocalParameterProvider> externalParameters = new();
#pragma warning restore 0414
#endif

        private IState defaultState;
        private readonly List<IState> swapStateTree = new();

        private readonly Dictionary<Type, IState> stateTypes = new();
        private Dictionary<string, IState> states;
        private Dictionary<string, Parameter> parameters;

        private void Awake()
        {
            if (!useGraphData)
            {
                states = new();
                
                return;
            }
            if (graphData == null)
            {
                Log("No graph data assigned to the state machine.", SHLogLevels.Warning);
                return;
            }

            CompileGraph();
            InjectStateReferences();
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

            EvaluateState();

            UpdateState();
        }

        #region Graph Compilation
        private void CompileGraph()
        {
            var compiledData = graphData.CompilationData;

            states = compiledData.StateIDs;

            foreach (var state in states.Values)
            {
                var type = state.GetType();

                if (!stateTypes.ContainsKey(type))
                    stateTypes[type] = state;

                state.ParameterProvider ??= this;
            }

            defaultState = compiledData.DefaultState;
            parameters = compiledData.ParameterNames;

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
            foreach (var state in stateTree)
            {
                state.Update();
            }
        }

        public IState EnterStateOfType<T>()
        {
            if (!stateTypes.TryGetValue(typeof(T), out var state))
            {
                Log($"StateMachine on {gameObject.name} does not have state of type '{typeof(T).Name}'!", SHLogLevels.Error);
                return null;
            }

            EnterState(state);

            return state;
        }

        public void AddState(IState state)
        {
            states.Add(Guid.NewGuid().ToString(), state);

            var type = state.GetType();

            if (!stateTypes.ContainsKey(type))
                stateTypes[type] = state;

            state.ParameterProvider ??= this;
        }

        public void AddStates(params IState[] states)
        {
            foreach (var state in states)
                AddState(state);
        }

        public void EnterState(IState newState)
        {
            swapStateTree.Clear();
            IState currentState = newState;

            while (currentState != null)
            {
                swapStateTree.Add(currentState);
                currentState = currentState.ParentState;
            }

            swapStateTree.Reverse();

            ExitStateTree(swapStateTree);
            EnterStateTree(swapStateTree);
        }

        private void ExitStateTree(List<IState> newStateTree)
        {
            if (stateTree.Count == 0)
                return;

            IState currentState = stateTree[^1];

            while (currentState != null && !newStateTree.Contains(currentState))
            {
                currentState.Exit();
                stateTree.RemoveAt(stateTree.Count - 1);

                currentState = currentState.ParentState;
            }
        }

        private void EnterStateTree(List<IState> newStateTree)
        {
            if (newStateTree.Count == 0)
                return;

            for (int i = 0; i < newStateTree.Count; i++)
            {
                IState currentState = newStateTree[i];

                if (!stateTree.Contains(currentState))
                {
                    stateTree.Add(currentState);
                    currentState.Enter();

                    if (i > 0)
                        newStateTree[i - 1].SetSubState(currentState);
                }
            }

            IState currentSubState = newStateTree[^1].DefaultSubState;

            while (currentSubState != null && !stateTree.Contains(currentSubState))
            {
                stateTree.Add(currentSubState);
                currentSubState.Enter();

                currentSubState.ParentState.SetSubState(currentSubState);
                currentSubState = currentSubState.DefaultSubState;
            }
        }
        #endregion 

        #region Parameters
        public T GetParameter<T>(string name)
        {
            if (parameters.TryGetValue(name, out var parameter))
            {
                if (parameter is Parameter<T> typedParameter)
                    return typedParameter.Value;
                else
                    Log($"Parameter '{name}' is not of type {typeof(T)}.", SHLogLevels.Error);
            }
            else
                Log($"Parameter '{name}' not found in the state machine.", SHLogLevels.Error);

            return default;
        }

        public void SetParameter<T>(string name, T value)
        {
            if (parameters.TryGetValue(name, out var parameter))
            {
                if (parameter is Parameter<T> typedParameter)
                    typedParameter.Value = value;
                else
                    Log($"Parameter '{name}' is not of type {typeof(T)}.", SHLogLevels.Error);
            }
            else
                Log($"Parameter '{name}' not found in the state machine.", SHLogLevels.Error);
        }
        #endregion
    }
}
