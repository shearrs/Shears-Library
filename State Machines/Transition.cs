using System;
using System.Collections.Generic;
using UnityEngine;

namespace InternProject.StateMachines
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
        private State _from;

        [SerializeField]
        private State _to;

        [SerializeReference]
        private List<ParameterComparison> _comparisons = new();

        internal IReadOnlyList<ParameterComparison> Comparisons => _comparisons;

        public State From { get => _from; set => _from = value; }
        public State To { get => _to; set => _to = value; }

        public Transition(State from, State to)
        {
            _from = from;
            _to = to;
        }

        public bool Evaluate()
        {
            foreach (var comparison in _comparisons)
            {
                if (!comparison.Evaluate())
                    return false;
            }

            return true;
        }

        public void AddComparison(ParameterComparison comparison)
        {
            _comparisons.Add(comparison);
        }

        public void RemoveComparison(ParameterComparison comparison)
        {
            _comparisons.Remove(comparison);
        }
    }
}
