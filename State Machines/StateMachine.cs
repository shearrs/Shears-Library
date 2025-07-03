using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shears.StateMachines
{
    public class StateMachine : MonoBehaviour
    {
        [Header("States")]
        [SerializeField]
        private State initialState;

#if UNITY_EDITOR
        [Header("Display")]
        [SerializeField, ReadOnly]
        private List<State> stateDisplay = new();
#endif

        private readonly List<State> stateTree = new();
        private List<State> states;

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
            states = GetComponentsInChildren<State>(true).ToList();

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
                            Debug.LogError($"Parameter '{comparison.ParameterID}' not found in the state machine.");
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
                state.UpdateState();
        }

        private void SetState(State newState)
        {
            List<State> newStateTree = new();
            State currentState = newState;

            while (currentState != null)
            {
                newStateTree.Add(currentState);
                currentState = currentState.ParentState;
            }

            newStateTree.Reverse();

            ExitState(newStateTree);
            EnterState(newStateTree);
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
                        newStateTree[i - 1].SubState = currentState;
                }
            }

            State currentSubState = newStateTree[^1].InitialSubState;

            while (currentSubState != null && !stateTree.Contains(currentSubState))
            {
                stateTree.Add(currentSubState);
                currentSubState.Enter();

                currentSubState.ParentState.SubState = currentSubState;
                currentSubState = currentSubState.InitialSubState;
            }
        }
        #endregion 

        #region Parameters
        public T GetParameter<T>(string name)
        {
            if (parameterNameDictionary.TryGetValue(name, out var parameter))
            {
                if (parameter is Parameter<T> typedParameter)
                    return typedParameter.Value;
                else
                    Debug.LogError($"Parameter '{name}' is not of type {typeof(T)}.");
            }
            else
                Debug.LogError($"Parameter '{name}' not found in the state machine.");

            return default;
        }

        public void SetParameter<T>(string name, T value)
        {
            if (parameterNameDictionary.TryGetValue(name, out var parameter))
            {
                if (parameter is Parameter<T> typedParameter)
                    typedParameter.Value = value;
                else
                    Debug.LogError($"Parameter '{name}' is not of type {typeof(T)}.");
            }
            else
                Debug.LogError($"Parameter '{name}' not found in the state machine.");
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
