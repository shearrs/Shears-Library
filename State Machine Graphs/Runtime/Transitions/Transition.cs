using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class Transition
    {
        private readonly State from;
        private readonly State to;

        private readonly List<ParameterComparison> comparisons = new();

        public State From => from;
        public State To => to;

        public Transition(State from, State to, List<ParameterComparison> comparisons)
        {
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
