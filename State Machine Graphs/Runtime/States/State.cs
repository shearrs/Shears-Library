using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public abstract class State
    {
        [SerializeField, ReadOnly] private string name;
        [SerializeField] private List<Transition> transitions = new();

        private State initialSubState;
        private State parentState;
        private State subState;

        public string Name { get => name; set => name = value; }
        public State ParentState { get => parentState; internal set => parentState = value; }
        public State DefaultSubState { get => initialSubState; internal set => initialSubState = value; }
        public State SubState { get => subState; internal set => subState = value; }
        public int TransitionCount => transitions.Count;

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

            newState = null;
            return false;
        }

        internal void SetSubState(State subState)
        {
            this.subState = subState;
        }

        internal void Enter()
        {
            OnEnter();
        }

        internal void Update()
        {
            OnUpdate();
        }

        internal void Exit()
        {
            OnExit();
        }

        protected abstract void OnEnter();
        protected abstract void OnUpdate();
        protected abstract void OnExit();
    }
}
