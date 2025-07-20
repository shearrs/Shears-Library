using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class EmptyComparisonData : ParameterComparisonData
    {
        public override ParameterComparison CreateComparison(StateMachineGraph graphData) => null;
    }
}
