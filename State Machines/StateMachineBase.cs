using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Shears.Logging;

namespace Shears.StateMachines
{
    public abstract class StateMachineBase : SHMonoBehaviourLogger
    {
        public abstract T GetParameter<T>(string name);

        public abstract void SetParameter<T>(string name, T value);

        public abstract void AddParameter(Parameter parameter);

        public abstract void RemoveParameter(string name);
    }

    public class StateMachineBase<StateType> : StateMachineBase where StateType: State
    {
        [Header("States")]
        [SerializeField]
        private StateType initialState;

#if UNITY_EDITOR
        [Header("Display")]
        [SerializeField, ReadOnly]
        private List<StateType> stateDisplay = new();
#endif

        private readonly List<StateType> stateTree = new();
        protected List<StateType> states;

        [Header("Parameters")]
        [SerializeReference]
        private List<Parameter> parameters = new();

        private readonly Dictionary<string, Parameter> parameterIDDictionary = new();
        private readonly Dictionary<string, Parameter> parameterNameDictionary = new();

        #region Initialization
        protected virtual void Awake()
        {
            InitializeStates();
            InitializeParameters();
        }

        protected virtual void Start()
        {
            SetState(initialState);
        }

        /// <summary>
        /// Initializes the state machine's state list.
        /// </summary>
        private void InitializeStates()
        {
            states = GetComponentsInChildren<StateType>(true).ToList();

            foreach (var state in states)
                state.StateMachine = this;
        }

        /// <summary>
        ///  Initializes both parameter dictionaries and sets the parameter references in all state transition comparisons.
        /// </summary>
        private void InitializeParameters()
        {
            foreach (var parameter in parameters)
            {
                parameterIDDictionary[parameter.ID] = parameter;
                parameterNameDictionary[parameter.Name] = parameter;
            }

            foreach (var state in states)
            {
                foreach (var transition in state.Transitions)
                {
                    foreach (var comparison in transition.Comparisons)
                    {
                        if (parameterIDDictionary.TryGetValue(comparison.ParameterID, out var parameter))
                            comparison.Parameter = parameter;
                        else
                            Log($"Parameter '{comparison.ParameterID}' not found in the state machine.", SHLogLevels.Error);
                    }
                }
            }
        }
        #endregion

        protected virtual void Update()
        {
            if (stateTree.Count == 0)
                return;

            EvaluateState();

            UpdateState();

            UpdateStateDisplay();
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
                    SetState((StateType)newState);
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
                state.UpdateState();
        }

        private void SetState(StateType newState)
        {
            List<StateType> newStateTree = new();
            StateType currentState = newState;

            while (currentState != null)
            {
                newStateTree.Add(currentState);
                currentState = (StateType)currentState.ParentState;
            }

            newStateTree.Reverse();

            ExitState(newStateTree);
            EnterState(newStateTree);
        }

        private void ExitState(List<StateType> newStateTree)
        {
            if (stateTree.Count == 0)
                return;

            StateType currentState = stateTree[^1];

            while (currentState != null && !newStateTree.Contains(currentState))
            {
                currentState.Exit();
                stateTree.RemoveAt(stateTree.Count - 1);

                currentState = (StateType)currentState.ParentState;
            }
        }

        private void EnterState(List<StateType> newStateTree)
        {
            if (newStateTree.Count == 0)
                return;

            for (int i = 0; i < newStateTree.Count; i++)
            {
                StateType currentState = newStateTree[i];

                if (!stateTree.Contains(currentState))
                {
                    stateTree.Add(currentState);
                    currentState.Enter();

                    if (i > 0)
                        newStateTree[i - 1].SubState = currentState;
                }
            }

            StateType currentSubState = (StateType)newStateTree[^1].InitialSubState;

            while (currentSubState != null && !stateTree.Contains(currentSubState))
            {
                stateTree.Add(currentSubState);
                currentSubState.Enter();

                currentSubState.ParentState.SubState = currentSubState;
                currentSubState = (StateType)currentSubState.InitialSubState;
            }
        }
        #endregion 

        #region Parameters
        public override T GetParameter<T>(string name)
        {
            if (parameterNameDictionary.TryGetValue(name, out var parameter))
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

        public override void SetParameter<T>(string name, T value)
        {
            if (parameterNameDictionary.TryGetValue(name, out var parameter))
            {
                if (parameter is Parameter<T> typedParameter)
                    typedParameter.Value = value;
                else
                    Log($"Parameter '{name}' is not of type {typeof(T)}.", SHLogLevels.Error);
            }
            else
                Log($"Parameter '{name}' not found in the state machine.", SHLogLevels.Error);
        }

        public override void AddParameter(Parameter parameter)
        {
            if (parameterNameDictionary.TryGetValue(parameter.Name, out var oldParameter))
            {
                Log($"Parameter {oldParameter.Name} already exists in the state machine.", SHLogLevels.Error);
                return;
            }

            parameters.Add(parameter);
            parameterIDDictionary[parameter.ID] = parameter;
            parameterNameDictionary[parameter.Name] = parameter;
        }

        public override void RemoveParameter(string name)
        {
            if (parameterNameDictionary.TryGetValue(name, out var parameter))
            {
                parameters.Remove(parameter);
                parameterIDDictionary.Remove(parameter.ID);
                parameterNameDictionary.Remove(parameter.Name);
            }
            else
                Log($"Parameter '{name}' not found in the state machine.", SHLogLevels.Error);
        }
        #endregion

        private void UpdateStateDisplay()
        {
#if UNITY_EDITOR
            stateDisplay.Clear();

            foreach (var state in stateTree)
            {
                stateDisplay.Add(state);
            }
#endif
        }
    }
}
