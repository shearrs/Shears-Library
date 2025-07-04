using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachines
{
    public abstract class State : MonoBehaviour
    {
        [SerializeField]
        private State initialSubState;

        [SerializeField, ReadOnly]
        private State currentSubState;

        [SerializeField]
        private TransitionCollection transitions = new();

        private State parentState;

        protected StateMachineBase stateMachine;

        internal IReadOnlyList<Transition> Transitions => transitions.Transitions;
        internal State ParentState => parentState;
        internal State InitialSubState { get => initialSubState; }
        internal State SubState { get => currentSubState; set => currentSubState = value; }
        internal StateMachineBase StateMachine { get => stateMachine; set => stateMachine = value; }

        public string Name => GetType().Name;

        private void OnValidate()
        {
            InitializeTransitions();
        }

        private void Awake()
        {
            parentState = transform.parent.GetComponentInParent<State>(true);
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
            currentSubState = null;

            OnExit();
        }
        protected abstract void OnExit();
        #endregion

        #region Transitions
        public bool EvaluateTransitions(out State newState)
        {
            return transitions.Evaluate(out newState);
        }

        private void InitializeTransitions()
        {
            transitions.Initialize(this);
        }
        #endregion
    
        #region Parameter Management
        protected T GetParameter<T>(string name)
        {
            if (stateMachine != null)
                return stateMachine.GetParameter<T>(name);
            else
            {
                Debug.LogError($"StateMachine not set for state '{GetType().Name}'. Cannot get parameter '{name}'.");
                return default;
            }
        }

        protected void SetParameter<T>(string name, T value)
        {
            if (stateMachine != null)
                stateMachine.SetParameter(name, value);
            else
                Debug.LogError($"StateMachine not set for state '{GetType().Name}'. Cannot set parameter '{name}'.");
        }
        #endregion
    }
}
