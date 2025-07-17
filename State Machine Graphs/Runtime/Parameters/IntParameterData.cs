using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class IntParameterData : ParameterData<int>
    {
        protected override string DefaultName => "Int Parameter";

        protected override ParameterComparisonData<int> CreateTypedComparison() => new IntComparisonData(this);
    }
}
