using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class BoolParameterData : ParameterData<bool>
    {
        protected override string DefaultName => "Bool Parameter";

        protected override ParameterComparisonData<bool> CreateTypedComparison() => new BoolComparisonData(this);
    }
}
