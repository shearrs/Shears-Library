using NUnit.Framework.Interfaces;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public interface IState
    {
        internal IParameterProvider ParameterProvider { get; set; }
        public string Name { get; set; }
        public IState ParentState { get; set; }
        public IState DefaultSubState { get; set; }
        public IState SubState { get; set; }
        public int TransitionCount { get; }

        internal void AddTransition(Transition transition);

        internal bool EvaluateTransitions(out IState newState);

        internal void SetSubState(IState subState);

        internal void Enter();

        internal void Update();

        internal void Exit();
    }
}
