using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class EmptyComparisonData : ParameterComparisonData
    {
        public override ParameterComparison CreateComparison(Parameter parameter) => null;
    }
}
