using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachines
{
    public abstract class State : MonoBehaviour
    {
        [SerializeField]
        private State _initialSubState;

        [SerializeField, ReadOnly]
        private State _currentSubState;

        [SerializeField]
        private TransitionCollection _transitions;

        private State _parentState;

        protected StateMachine _stateMachine;

        internal IReadOnlyList<Transition> Transitions => _transitions.Transitions;
        internal State ParentState => _parentState;
        internal State InitialSubState { get => _initialSubState; }
        internal State SubState { get => _currentSubState; set => _currentSubState = value; }
        internal StateMachine StateMachine { get => _stateMachine; set => _stateMachine = value; }

        public string Name => GetType().Name;

        private void OnValidate()
        {
            InitializeTransitions();
        }

        private void Awake()
        {
            _parentState = transform.parent.GetComponentInParent<State>(true);
        }

        private void Start()
        {
            InitializeTransitions();
        }

        #region State Control Flow
        public void Enter()
        {
            OnEnter();
        }
        protected abstract void OnEnter();

        public void UpdateState()
        {
            OnUpdate();
        }
        protected abstract void OnUpdate();

        public void Exit()
        {
            _currentSubState = null;

            OnExit();
        }
        protected abstract void OnExit();
        #endregion

        #region Transitions
        public bool EvaluateTransitions(out State newState)
        {
            return _transitions.Evaluate(out newState);
        }

        private void InitializeTransitions()
        {
            _transitions.Initialize(this);
        }
        #endregion
    
        #region Parameter Management
        protected T GetParameter<T>(string name)
        {
            if (_stateMachine != null)
                return _stateMachine.GetParameter<T>(name);
            else
            {
                Debug.LogError($"StateMachine not set for state '{GetType().Name}'. Cannot get parameter '{name}'.");
                return default;
            }
        }

        protected void SetParameter<T>(string name, T value)
        {
            if (_stateMachine != null)
                _stateMachine.SetParameter(name, value);
            else
                Debug.LogError($"StateMachine not set for state '{GetType().Name}'. Cannot set parameter '{name}'.");
        }
        #endregion
    }
}
