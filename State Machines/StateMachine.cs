using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InternProject.StateMachines
{
    public class StateMachine : MonoBehaviour
    {
        [Header("States")]
        [SerializeField]
        private State _initialState;

#if UNITY_EDITOR
        [SerializeField, ReadOnly]
        private List<State> _stateDisplay = new();
#endif

        private readonly List<State> _stateTree = new();
        private List<State> _states;

        [SerializeReference]
        private List<Parameter> _parameters = new();

        private readonly Dictionary<string, Parameter> _parameterIDDictionary = new();
        private readonly Dictionary<string, Parameter> _parameterNameDictionary = new();

        #region Initialization
        protected virtual void Awake()
        {
            InitializeStates();
            InitializeParameters();
        }

        protected virtual void Start()
        {
            SetState(_initialState);
        }

        /// <summary>
        /// Initializes the state machine's state list.
        /// </summary>
        private void InitializeStates()
        {
            _states = GetComponentsInChildren<State>(true).ToList();

            foreach (var state in _states)
                state.StateMachine = this;
        }

        /// <summary>
        ///  Initializes both parameter dictionaries and sets the parameter references in all state transition comparisons.
        /// </summary>
        private void InitializeParameters()
        {
            foreach (var parameter in _parameters)
            {
                _parameterIDDictionary[parameter.ID] = parameter;
                _parameterNameDictionary[parameter.Name] = parameter;
            }

            foreach (var state in _states)
            {
                foreach (var transition in state.Transitions)
                {
                    foreach (var comparison in transition.Comparisons)
                    {
                        if (_parameterIDDictionary.TryGetValue(comparison.ParameterID, out var parameter))
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
            if (_stateTree.Count == 0)
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
            foreach (var state in _stateTree)
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
            foreach (var state in _stateTree)
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
            if (_stateTree.Count == 0)
                return;

            State currentState = _stateTree[^1];

            while (currentState != null && !newStateTree.Contains(currentState))
            {
                currentState.Exit();
                _stateTree.RemoveAt(_stateTree.Count - 1);

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

                if (!_stateTree.Contains(currentState))
                {
                    _stateTree.Add(currentState);
                    currentState.Enter();

                    if (i > 0)
                        newStateTree[i - 1].SubState = currentState;
                }
            }

            State currentSubState = newStateTree[^1].InitialSubState;

            while (currentSubState != null && !_stateTree.Contains(currentSubState))
            {
                _stateTree.Add(currentSubState);
                currentSubState.Enter();

                currentSubState.ParentState.SubState = currentSubState;
                currentSubState = currentSubState.InitialSubState;
            }
        }
        #endregion 

        #region Parameters
        public T GetParameter<T>(string name)
        {
            if (_parameterNameDictionary.TryGetValue(name, out var parameter))
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
            if (_parameterNameDictionary.TryGetValue(name, out var parameter))
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
            _stateDisplay.Clear();

            foreach (var state in _stateTree)
            {
                _stateDisplay.Add(state);
            }
#endif
        }
    }
}
