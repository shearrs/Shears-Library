using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class Transition
    {
#if UNITY_EDITOR
        [SerializeField, ReadOnly] private string toName;
#endif
        [SerializeReference] private State from;
        [SerializeReference] private State to;
        [SerializeReference] private List<ParameterComparison> comparisons = new();

        public State From => from;
        public State To => to;

        public Transition(State from, State to, List<ParameterComparison> comparisons)
        {
#if UNITY_EDITOR
            toName = to.Name;
#endif

            this.from = from;
            this.to = to;
            this.comparisons = comparisons;
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
    }
}
