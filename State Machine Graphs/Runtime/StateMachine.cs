using Shears.GraphViews;
using Shears.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class StateMachine : SHMonoBehaviourLogger, IParameterProvider
    {
        [Header("State Machine")]
        [SerializeField] private StateMachineGraph graphData;
        [SerializeField] private List<StateInjectReference> injectReferences = new();
        [SerializeReference] private List<State> stateTree = new();

#if UNITY_EDITOR
        [Header("Parameters")]
        [SerializeReference] private List<Parameter> parameterDisplay = new();
        [SerializeField] private List<LocalParameterProvider> externalParameters = new();
#endif

        private State defaultState;
        private readonly List<State> swapStateTree = new();

        private Dictionary<string, State> states;
        private Dictionary<string, Parameter> parameters;

        private void Awake()
        {
            CompileGraph();
        }

        private void Start()
        {
            SetState(defaultState);
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
            var compiledData = graphData.Compile();

            states = compiledData.StateIDs;

            foreach (var state in states.Values)
                state.ParameterProvider ??= this;

            defaultState = compiledData.DefaultState;
            parameters = compiledData.ParameterNames;

#if UNITY_EDITOR
            parameterDisplay.AddRange(parameters.Values);
            externalParameters = compiledData.ParameterProviders;
#endif
        }
        
        public bool HasInjectType(Type type)
        {
            foreach (var reference in injectReferences)
            {
                if (reference.FieldType == type)
                    return true;
            }

            return false;
        }

        public bool HasInjectTarget(StateInjectTarget target)
        {
            foreach (var reference in injectReferences)
            {
                if (reference.Targets.Contains(target))
                    return true;
            }

            return false;
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
                    SetState(newState);
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

        private void SetState(State newState)
        {
            swapStateTree.Clear();
            State currentState = newState;

            while (currentState != null)
            {
                swapStateTree.Add(currentState);
                currentState = currentState.ParentState;
            }

            swapStateTree.Reverse();

            ExitState(swapStateTree);
            EnterState(swapStateTree);
        }

        private void ExitState(List<State> newStateTree)
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

        private void EnterState(List<State> newStateTree)
        {
            if (newStateTree.Count == 0)
                return;

            for (int i = 0; i < newStateTree.Count; i++)
            {
                State currentState = newStateTree[i];

                if (!stateTree.Contains(currentState))
                {
                    stateTree.Add(currentState);
                    currentState.Enter();

                    if (i > 0)
                        newStateTree[i - 1].SetSubState(currentState);
                }
            }

            State currentSubState = newStateTree[^1].DefaultSubState;

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
