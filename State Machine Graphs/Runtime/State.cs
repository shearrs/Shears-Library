using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public abstract class State
    {
        private readonly List<Transition> transitions = new();
        private readonly State parentState;
        private readonly State initialSubState;
        private State subState;

        public State ParentState => parentState;
        public State InitialSubState => initialSubState;
        public State SubState => subState;

        public State(State parentState, State initialSubState)
        {
            this.parentState = parentState;
            this.initialSubState = initialSubState;
        }

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
