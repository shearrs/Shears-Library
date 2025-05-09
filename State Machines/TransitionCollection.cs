using System.Collections.Generic;
using UnityEngine;

namespace InternProject.StateMachines
{
    [System.Serializable]
    public class TransitionCollection
    {
        [SerializeField]
        private List<Transition> _transitions = new();

        public IReadOnlyList<Transition> Transitions => _transitions;

        public void Initialize(State from)
        {
            foreach (var transition in _transitions)
                transition.From = from;
        }

        /// <summary>
        /// Evaluates whether or not there is a valid transition out of this state.
        /// </summary>
        /// <param name="newState">The new state to transition to, or null if none was found.</param>
        /// <returns>Whether or not a valid transition was found.</returns>
        public bool Evaluate(out State newState)
        {
            newState = null;

            foreach (var transition in _transitions)
            {
                if (transition.Evaluate())
                {
                    newState = transition.To;
                    return true;
                }
            }

            return false;
        }
    }
}
