using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachines
{
    /// <summary>
    /// Represents a transition between two states in a state machine.
    /// </summary>
    [Serializable]
    public class Transition
    {
        #if UNITY_EDITOR
        [SerializeField, HideInInspector] private bool _isFoldoutExpanded;
        #endif

        [SerializeField, ReadOnly]
        private State from;

        [SerializeField]
        private State to;

        [SerializeReference]
        private List<ParameterComparison> comparisons = new();

        internal IReadOnlyList<ParameterComparison> Comparisons => comparisons;

        public State From { get => from; set => from = value; }
        public State To { get => to; set => to = value; }

        public Transition(State from, State to)
        {
            this.from = from;
            this.to = to;
        }

        public bool Evaluate()
        {
            foreach (var comparison in comparisons)
            {
                if (!comparison.Evaluate())
                    return false;
            }

            return true;
        }

        public void AddComparison(ParameterComparison comparison)
        {
            comparisons.Add(comparison);
        }

        public void RemoveComparison(ParameterComparison comparison)
        {
            comparisons.Remove(comparison);
        }
    }
}
