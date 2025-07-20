using Shears.GraphViews;
using Shears.Logging;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class StateMachine : SHMonoBehaviourLogger
    {
        [SerializeField] private StateMachineGraph graphData;

        private State initialState;

        private readonly List<ParameterComparison> instanceComparisons = new();
        private readonly List<State> stateTree = new();
        private readonly List<State> swapStateTree = new();

        private readonly Dictionary<string, State> states = new();
        private readonly Dictionary<string, Parameter> parameters = new();

        private void Awake()
        {
            CompileGraph();
        }

        private void Start()
        {
            SetState(initialState);
        }

        private void Update()
        {
            if (stateTree.Count == 0)
                return;

            EvaluateState();

            UpdateState();
        }

        private void CompileGraph()
        {
            foreach (var parameterData in graphData.GetParameters())
            {
                var parameter = CreateParameter(parameterData);

                parameters.Add(parameter.Name, parameter);
            }

            var nodeData = graphData.GetStateNodes();

            foreach (var stateNode in nodeData)
            {
                var state = CreateState(stateNode);

                states.Add(stateNode.ID, state);
            }

            foreach (var stateNode in nodeData)
            {
                CreateTransitions(stateNode, states[stateNode.ID]);
            }
        }

        private Parameter CreateParameter(ParameterData data) => data.CreateParameter();

        private State CreateState(IStateNodeData data) => data.CreateStateInstance();

        private void CreateTransitions(IStateNodeData data, State state)
        {
            var transitionIDs = data.GetTransitionIDs();
            
            foreach (var id in transitionIDs)
            {
                if (!graphData.TryGetData(id, out TransitionEdgeData transitionData))
                {
                    Log("Could not find transition with id: " + id, SHLogLevels.Error);
                    continue;
                }

                if (transitionData.FromID != data.ID)
                    continue;

                if (!states.TryGetValue(transitionData.ToID, out var toState))
                {
                    Log("Could not find target state with id: " + transitionData.ToID, SHLogLevels.Error);
                    continue;
                }

                foreach (var comparisonData in transitionData.ComparisonData)
                {
                    
                }

                //var transition = new Transition(state, toState, transitionData.)
            }
        }

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
                state.Update();
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

            State currentSubState = newStateTree[^1].InitialSubState;

            while (currentSubState != null && !stateTree.Contains(currentSubState))
            {
                stateTree.Add(currentSubState);
                currentSubState.Enter();

                currentSubState.ParentState.SetSubState(currentSubState);
                currentSubState = currentSubState.InitialSubState;
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
